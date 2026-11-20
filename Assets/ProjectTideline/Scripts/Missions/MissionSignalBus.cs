using System;
using System.Collections.Generic;

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
            try
            {
                while (true)
                {
                    while (PendingSignals.Count > 0)
                    {
                        PendingSignal next = PendingSignals.Dequeue();
                        Raised?.Invoke(next.SignalId, next.Amount);
                    }

                    DispatchCompleted?.Invoke();
                    if (PendingSignals.Count == 0)
                        break;
                }
            }
            finally
            {
                IsDispatching = false;
            }
        }
    }
}