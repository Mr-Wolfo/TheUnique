using TheUnique.Core.Items;
using UnityEngine;
using UnityEngine.InputSystem; // Используем новую систему ввода
using TheUnique.Core.Player;

namespace TheUnique.Core.Interaction
{
    public class MouseInteractionHandler : MonoBehaviour
    {
        [SerializeField] private PlayerEntity _player;
        [SerializeField] private LayerMask _interactableLayer;

        private IInteractable _hoveredInteractable;
        private SpriteRenderer _hoveredSprite;
        private Color _originalColor;

        private void Update()
        {
            HandleMouseHover();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseClick();
            }
        }

        private void HandleMouseHover()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, _interactableLayer);

            if (hit.collider != null && hit.collider.TryGetComponent(out IInteractable interactable))
            {
                float distance = Vector2.Distance(_player.transform.position, hit.collider.transform.position);
                if (distance <= interactable.GetInteractionDistance())
                {
                    if (_hoveredInteractable != interactable)
                    {
                        RemoveHighlight();
                        _hoveredInteractable = interactable;
                        _hoveredSprite = hit.collider.GetComponentInChildren<SpriteRenderer>();
                        
                        if (_hoveredSprite != null && _hoveredSprite.gameObject != null && _hoveredSprite.gameObject.activeInHierarchy)
                        {
                            _originalColor = _hoveredSprite.color;
                            _hoveredSprite.color = new Color(1.5f, 1.5f, 1.5f);
                        }
                    }
                    return; 
                }
            }

            RemoveHighlight();
        }

        private void RemoveHighlight()
        {
            if (_hoveredInteractable != null)
            {
                if (_hoveredSprite != null && _hoveredSprite.gameObject != null && _hoveredSprite.gameObject.activeInHierarchy)
                {
                    _hoveredSprite.color = _originalColor;
                }
                
                _hoveredInteractable = null;
                _hoveredSprite = null;
            }
        }

        private void HandleMouseClick()
        {
            if (_hoveredInteractable != null && _hoveredInteractable as MonoBehaviour != null)
            {
                _hoveredInteractable.Interact(_player);
                RemoveHighlight(); 
            }
            else
            {
                ItemSO activeItem = _player.Inventory.GetActiveItem();
                if (activeItem != null)
                {
                    _player.UseItem(activeItem);
                }
            }
        }
    }
}