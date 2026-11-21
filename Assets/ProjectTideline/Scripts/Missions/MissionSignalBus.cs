using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTideline
{
    public static class MissionSignalBus
    {
        private readonly struct PendingSignal
        {
            public readonly string SignalId;
            public readonly int Amount;

            public PendingSignal(string signalId, int amount)
            {
                SignalId = signalId;
                Amount = amount;
            }
        }

        private const int MaxSignalsPerBatch = 4096;
        private static readonly Queue<PendingSignal> PendingSignals = new Queue<PendingSignal>();
        private static bool IsDispatching;

        public static event Action<string, int> Raised;

        /// <summary>
        /// Raised after the current signal and every signal published by its listeners have drained.
        /// Mission progression is committed here so nested side-effects cannot leak into the next mission.
        /// </summary>
        public static event Action DispatchCompleted;

        public static void Publish(string signalId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(signalId) || amount <= 0)
                return;

            PendingSignals.Enqueue(new PendingSignal(signalId, amount));
            if (IsDispatching)
                return;

            IsDispatching = true;
            int dispatched = 0;
            try
            {
                while (PendingSignals.Count > 0)
                {
                    while (PendingSignals.Count > 0)
                    {
                        if (dispatched >= MaxSignalsPerBatch)
                        {
                            Debug.LogError("MissionSignalBus aborted a signal batch after " +
                                MaxSignalsPerBatch + " events. Check for a cyclic signal publisher.");
                            PendingSignals.Clear();
                            return;
                        }

                        PendingSignal next = PendingSignals.Dequeue();
                        dispatched++;
                        InvokeRaisedSafely(next.SignalId, next.Amount);
                    }

                    InvokeDispatchCompletedSafely();
                    // A DispatchCompleted listener may publish another signal; drain it
                    // in the same batch before allowing another interaction to begin.
                }
            }
            finally
            {
                IsDispatching = false;
            }
        }

        private static void InvokeRaisedSafely(string signalId, int amount)
        {
            Action<string, int> handlers = Raised;
            if (handlers == null) return;

            foreach (Delegate subscriber in handlers.GetInvocationList())
            {
                try
                {
                    ((Action<string, int>)subscriber).Invoke(signalId, amount);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static void InvokeDispatchCompletedSafely()
        {
            Action handlers = DispatchCompleted;
            if (handlers == null) return;

            foreach (Delegate subscriber in handlers.GetInvocationList())
            {
                try
                {
                    ((Action)subscriber).Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }
}