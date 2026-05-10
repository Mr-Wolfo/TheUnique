using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheUnique.Core.Environment
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Освещение")]
        [SerializeField] private Light2D _globalLight;
        [SerializeField] private Gradient _lightColor;
        [SerializeField] private AnimationCurve _lightIntensity;

        [Header("Время")]
        [SerializeField] private float _dayDurationInSeconds = 120f;
        
        [Range(0f, 1f)]
        public float TimeOfDay;

        private void Update()
        {
            TimeOfDay += Time.deltaTime / _dayDurationInSeconds;
            if (TimeOfDay >= 1f) TimeOfDay = 0f;

            if (_globalLight != null)
            {
                _globalLight.color = _lightColor.Evaluate(TimeOfDay);
                _globalLight.intensity = _lightIntensity.Evaluate(TimeOfDay);
            }
        }
    }
}