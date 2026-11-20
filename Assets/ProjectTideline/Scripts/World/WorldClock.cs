using UnityEngine;

namespace ProjectTideline
{
    public sealed class WorldClock : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private float startHour = 3.2833f;
        [SerializeField] private float realSecondsPerGameDay = 900f;
        [SerializeField, Range(0f, 1f)] private float ambientNightIntensity = 0.22f;
        [Header("Atmosphere")]
        [SerializeField] private Color daySunColor = new Color(1f, 0.82f, 0.68f);
        [SerializeField] private Color nightSunColor = new Color(0.28f, 0.39f, 0.58f);
        [SerializeField] private Color daySkyColor = new Color(0.32f, 0.38f, 0.45f);
        [SerializeField] private Color nightSkyColor = new Color(0.055f, 0.075f, 0.115f);
        [SerializeField] private Color dayEquatorColor = new Color(0.2f, 0.22f, 0.24f);
        [SerializeField] private Color nightEquatorColor = new Color(0.035f, 0.045f, 0.065f);
        [SerializeField] private Color dayGroundColor = new Color(0.09f, 0.1f, 0.11f);
        [SerializeField] private Color nightGroundColor = new Color(0.018f, 0.022f, 0.032f);
        [SerializeField] private Color dayFogColor = new Color(0.32f, 0.38f, 0.4f);
        [SerializeField] private Color nightFogColor = new Color(0.035f, 0.055f, 0.08f);
        public float CurrentHour { get; private set; }

        private void Start() => SetCurrentHour(startHour);

        public void SetCurrentHour(float hour)
        {
            CurrentHour = Mathf.Repeat(hour, 24f);
            ApplySunState();
        }

        private void Update()
        {
            if (realSecondsPerGameDay <= 0f) return;
            CurrentHour = Mathf.Repeat(CurrentHour + Time.deltaTime * 24f / realSecondsPerGameDay, 24f);
            ApplySunState();
        }

        private void ApplySunState()
        {
            if (sun == null) return;
            float normalized = CurrentHour / 24f;
            sun.transform.rotation = Quaternion.Euler(normalized * 360f - 90f, 165f, 0f);
            float daylight = Mathf.Clamp01(Mathf.Sin(normalized * Mathf.PI * 2f - Mathf.PI * 0.5f) * 0.5f + 0.5f);
            float blend = Mathf.SmoothStep(0f, 1f, daylight);
            sun.intensity = Mathf.Lerp(ambientNightIntensity, 1.15f, blend);
            sun.color = Color.Lerp(nightSunColor, daySunColor, blend);
            RenderSettings.ambientSkyColor = Color.Lerp(nightSkyColor, daySkyColor, blend);
            RenderSettings.ambientEquatorColor = Color.Lerp(nightEquatorColor, dayEquatorColor, blend);
            RenderSettings.ambientGroundColor = Color.Lerp(nightGroundColor, dayGroundColor, blend);
            RenderSettings.fogColor = Color.Lerp(nightFogColor, dayFogColor, blend);
        }
    }
}