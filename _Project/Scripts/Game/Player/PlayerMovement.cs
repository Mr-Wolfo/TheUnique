using UnityEngine;
using UnityEngine.InputSystem;
using TheUnique.Core.Attributes;

namespace TheUnique.Core.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(AttributeSystem))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Настройки бега")]
        [SerializeField] private float _sprintSpeedMultiplier = 1.5f;
        [SerializeField] private float _staminaDrainRate = 10f;

        [Header("Анимации")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Rigidbody2D _rb;
        private AttributeSystem _attributes;
        private Vector2 _moveInput;
        private Vector2 _lastMoveDir = Vector2.down;
        private bool _isSprintPressed;
        public bool _isSprinting;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _attributes = GetComponent<AttributeSystem>();
            
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();

        public void OnSprint(InputValue value) => _isSprintPressed = value.isPressed;

        private void FixedUpdate()
        {
            HandleSprinting();
            Move();
        }

        private void Update()
        {
            UpdateAnimations();
        }

        private void HandleSprinting()
        {
            bool isMoving = _moveInput.magnitude > 0.1f;
            bool hasStamina = _attributes.Stamina.CurrentValue > 1f;

            _isSprinting = _isSprintPressed && isMoving && hasStamina;

            _attributes.PauseStaminaRegen = _isSprinting;

            if (_isSprinting)
            {
                _attributes.Stamina.ApplyChange(-_staminaDrainRate * Time.fixedDeltaTime);
            }
        }

        private void Move()
        {
            float speed = _attributes.MoveSpeed.Value;
            if (_isSprinting) speed *= _sprintSpeedMultiplier;

            _rb.linearVelocity = _moveInput * speed;
        }

        private void UpdateAnimations()
        {
            if (_animator == null || _spriteRenderer == null) return;

            bool isMoving = _moveInput.magnitude > 0.1f;

            if (isMoving)
            {
                _lastMoveDir = _moveInput.normalized;
                
                if (_moveInput.x < -0.1f) _spriteRenderer.flipX = true;
                else if (_moveInput.x > 0.1f) _spriteRenderer.flipX = false;
            }

            _animator.SetFloat("DirX", _lastMoveDir.x);
            _animator.SetFloat("DirY", _lastMoveDir.y);
            _animator.SetBool("IsMoving", isMoving);
            _animator.SetBool("IsSprinting", _isSprinting);
        }
    }
}