using UnityEngine;
using UnityEngine.InputSystem;
using TheUnique.Core.Player;

namespace TheUnique.Core.Interaction
{
    public class InteractionHandler : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private float _interactionRadius = 1.5f;
        [SerializeField] private LayerMask _interactableLayer;
        
        private PlayerEntity _player;
        public IInteractable CurrentInteractable { get; private set; }

        private void Awake()
        {
            _player = GetComponent<PlayerEntity>();
        }

        private void Update()
        {
            FindInteractable();
        }
        
        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                PerformInteraction();
            }
        }

        private void FindInteractable()
        {
            Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, _interactionRadius, _interactableLayer);

            IInteractable closest = null;
            float minDistance = float.MaxValue;

            foreach (var col in results)
            {
                if (col.TryGetComponent(out IInteractable interactable))
                {
                    float dist = Vector2.Distance(transform.position, col.transform.position);

                    if (dist <= interactable.GetInteractionDistance() && dist < minDistance)
                    {
                        minDistance = dist;
                        closest = interactable;
                    }
                }
            }

            CurrentInteractable = closest;
        }

        private void PerformInteraction()
        {
            if (CurrentInteractable != null)
            {
                CurrentInteractable.Interact(_player);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}