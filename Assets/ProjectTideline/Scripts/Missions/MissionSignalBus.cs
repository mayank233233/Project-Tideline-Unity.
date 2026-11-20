using System;

namespace ProjectTideline
{
    public static class MissionSignalBus
    {
        public static event Action<string, int> Raised;

        public static void Publish(string signalId, int amount = 1)
        {
            if (!string.IsNullOrWhiteSpace(signalId) && amount > 0)
                Raised?.Invoke(signalId, amount);
        }
    }
}