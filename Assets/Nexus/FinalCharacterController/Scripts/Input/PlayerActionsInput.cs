using UnityEngine;
using UnityEngine.InputSystem;

namespace Nexus.FinalCharacterController
{
    [DefaultExecutionOrder(-2)]
    public class PlayerActionsInput : MonoBehaviour, PlayerControls.IPlayerActionMapActions
    {
        public bool AttackPressed { get; private set; }
        public bool InteractPressed { get; private set; }

        private PlayerLocomotionInput _playerLocomotionInput;
        private WaveButton _currentHoveredButton;

        private void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        }

        private void OnEnable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Enable();
            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Disable();
            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.RemoveCallbacks(this);
        }

        private void Update()
        {
            if (_playerLocomotionInput.MovementInput != Vector2.zero)
            {
                InteractPressed = false;
            }

            CheckForInteractables();
        }

        private void CheckForInteractables()
        {
            if (Camera.main == null) return;

            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                WaveButton button = hit.collider.GetComponentInParent<WaveButton>();
                if (button != null)
                {
                    if (_currentHoveredButton != button)
                    {
                        if (_currentHoveredButton != null) _currentHoveredButton.SetHoverState(false);
                        _currentHoveredButton = button;
                        _currentHoveredButton.SetHoverState(true);
                    }
                    return;
                }
            }

            if (_currentHoveredButton != null)
            {
                _currentHoveredButton.SetHoverState(false);
                _currentHoveredButton = null;
            }
        }

        public void SetAttackPressedFalse()
        {
            AttackPressed = false;
        }

        public void SetInteractPressedFalse()
        {
            InteractPressed = false;
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            AttackPressed = true;
        }


        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            InteractPressed = true;

            if (_currentHoveredButton != null)
            {
                _currentHoveredButton.PressButton();
            }
        }

        public void OnOpenShop(InputAction.CallbackContext context)
        {
        }
    }
}
