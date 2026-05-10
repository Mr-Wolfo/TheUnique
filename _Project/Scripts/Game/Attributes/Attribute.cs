using System;
using UnityEngine;

namespace TheUnique.Core.Attributes
{
    public enum AttributeType { Health, Hunger, Stamina }

    [Serializable]
    public class Attribute : BaseStat
    {
        public float CurrentValue { get; private set; }

        public event Action<Attribute> OnValueChanged;

        public Attribute(float baseValue) : base(baseValue)
        {
            CurrentValue = baseValue;
        }

        public void SetCurrentValue(float newValue)
        {
            float clamped = Mathf.Clamp(newValue, 0, Value);
            if (Mathf.Approximately(clamped, CurrentValue)) return;

            CurrentValue = clamped;
            OnValueChanged?.Invoke(this);
        }

        public void ApplyChange(float amount)
        {
            SetCurrentValue(CurrentValue + amount);
        }

        public void UpdateAttribute(float tickAmount)
        {
            ApplyChange(tickAmount);
        }
    }
}