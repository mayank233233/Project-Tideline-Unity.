using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Small timed signal controller for the Old Quay blockout junction.
    /// The two road groups receive exclusive green phases with all-red clearance.
    /// </summary>
    public sealed class TrafficSignalController : MonoBehaviour
    {
        [SerializeField, Min(4f)] private float mainRoadGreenDuration = 16f;
        [SerializeField, Min(1f)] private float firstClearanceDuration = 2f;
        [SerializeField, Min(4f)] private float crossStreetGreenDuration = 12f;
        [SerializeField, Min(1f)] private float secondClearanceDuration = 2f;
        [SerializeField, Min(4f)] private float pedestrianWalkDuration = 12f;

        private Renderer _mainRed;
        private Renderer _mainGreen;
        private Renderer _crossRed;
        private Renderer _crossGreen;
        private Renderer _pedestrianRed;
        private Renderer _pedestrianGreen;
        private int _lastPhase = -1;

        public bool IsMainRoadGreen
        {
            get
            {
                float phaseTime = GetCycleTime();
                return phaseTime < mainRoadGreenDuration;
            }
        }

        public bool IsPedestrianWalkAllowed
        {
            get
            {
                float phaseTime = GetCycleTime();
                float mainClearanceEnd = mainRoadGreenDuration + firstClearanceDuration;
                float pedestrianStart = mainClearanceEnd + crossStreetGreenDuration + secondClearanceDuration + pedestrianWalkDuration;
                return phaseTime >= pedestrianStart && phaseTime < pedestrianStart + pedestrianWalkDuration;
            }
        }

        public bool IsCrossStreetGreen
        {
            get
            {
                float phaseTime = GetCycleTime();
                float crossStart = mainRoadGreenDuration + firstClearanceDuration;
                return phaseTime >= crossStart && phaseTime < crossStart + crossStreetGreenDuration;
            }
        }

        public void Configure(Renderer mainRed, Renderer mainGreen, Renderer crossRed, Renderer crossGreen,
            float mainGreenSeconds, float firstClearanceSeconds, float crossGreenSeconds, float secondClearanceSeconds)
        {
            _mainRed = mainRed;
            _mainGreen = mainGreen;
            _crossRed = crossRed;
            _crossGreen = crossGreen;
            mainRoadGreenDuration = Mathf.Max(4f, mainGreenSeconds);
            firstClearanceDuration = Mathf.Max(1f, firstClearanceSeconds);
            crossStreetGreenDuration = Mathf.Max(4f, crossGreenSeconds);
            secondClearanceDuration = Mathf.Max(1f, secondClearanceSeconds);
            ApplyPhase(-1);
        }

        public void ConfigurePedestrianSignals(Renderer red, Renderer green)
        {
            _pedestrianRed = red;
            _pedestrianGreen = green;
            ApplyPhase(_lastPhase);
        }

        private float GetCycleTime()
        {
            float cycle = mainRoadGreenDuration + firstClearanceDuration
                + crossStreetGreenDuration + secondClearanceDuration;
            return Mathf.Repeat(Time.time, Mathf.Max(1f, cycle));
        }

        private void Update()
        {
            float phaseTime = GetCycleTime();
            float mainEnd = mainRoadGreenDuration;
            float crossStart = mainEnd + firstClearanceDuration;
            float crossEnd = crossStart + crossStreetGreenDuration;
            float pedestrianStart = crossEnd + secondClearanceDuration;
            int phase = phaseTime < mainEnd ? 0
                : phaseTime < crossStart ? 1
                : phaseTime < crossEnd ? 2
                : phaseTime < pedestrianStart ? 3 : 4;
            if (phase != _lastPhase) ApplyPhase(phase);
        }

        private void ApplyPhase(int phase)
        {
            _lastPhase = phase;
            bool mainGreen = phase == 0;
            bool crossGreen = phase == 2;
            if (_mainRed != null) _mainRed.enabled = !mainGreen;
            if (_mainGreen != null) _mainGreen.enabled = mainGreen;
            if (_crossRed != null) _crossRed.enabled = !crossGreen;
            if (_crossGreen != null) _crossGreen.enabled = crossGreen;
            bool pedestrianWalk = phase == 4;
            if (_pedestrianRed != null) _pedestrianRed.enabled = !pedestrianWalk;
            if (_pedestrianGreen != null) _pedestrianGreen.enabled = pedestrianWalk;
        }
    }
}
