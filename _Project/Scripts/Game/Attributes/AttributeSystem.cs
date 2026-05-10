using UnityEngine;
using System;
using TheUnique.Core.Player;

namespace TheUnique.Core.Attributes
{
    public class AttributeSystem : MonoBehaviour
    {
        [Header("Основные статы")]
        public Attribute Health = new Attribute(100f);
        public Attribute Hunger = new Attribute(100f);
        public Attribute Stamina = new Attribute(100f);

        [Header("Параметры движения")]
        public BaseStat MoveSpeed = new BaseStat(5f);

        [Header("Настройки выживания")]
        public float hungerDecayRate = 0.5f;
        public float healthRegenRate = 1.0f;
        public float staminaRegenRate = 5.0f;
        
        [Tooltip("Урон здоровью в секунду при нулевом голоде")]
        public float starvationDamageRate = 2.0f;
        [Tooltip("Минимальный уровень голода для регенерации здоровья")]
        public float healthRegenHungerThreshold = 70f;

        [HideInInspector] public bool PauseStaminaRegen = false;
        
        public event Action OnDeath;

        private void Awake()
        {
            Health.OnValueChanged += (a) => { if (a.CurrentValue <= 0) OnDeath?.Invoke(); };
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Уменьшение голода
            Hunger.ApplyChange(-hungerDecayRate * dt);

            // 2. Регенерация здоровья
            if (Hunger.CurrentValue > healthRegenHungerThreshold && Health.CurrentValue < Health.Value)
                Health.ApplyChange(healthRegenRate * dt);
            
            // 3. Урон от голода
            if (Hunger.CurrentValue <= 0)
                Health.ApplyChange(-starvationDamageRate * dt);

            // 4. Регенерация стамины
            float staminaEfficiency = Mathf.Clamp01(Hunger.CurrentValue / 50f); 
            
            if (!PauseStaminaRegen && Stamina.CurrentValue < Stamina.Value)
            {
                Stamina.ApplyChange(staminaRegenRate * staminaEfficiency * dt);
            }
        }
    }
}