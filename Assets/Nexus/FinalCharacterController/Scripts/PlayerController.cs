using UnityEngine;

namespace Nexus.FinalCharacterController
{
    [DefaultExecutionOrder(-1)]
    public class PlayerController : MonoBehaviour
    {
        #region Class Variables
        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Camera _playerCamera;
        public float RotationMismatch { get; private set; } = 0f;
        public bool IsRotatingToTarget { get; private set; } = false;

        [Header("Base Movement")]
        public float runAcceleration = 0.25f;
        public float runSpeed = 4f;
        public float sprintAcceleration = 0.5f;
        public float sprintSpeed = 7f;
        public float drag = 0.1f;
        public float gravity = 25f;
        public float jumpSpeed = 1.0f;
        public float movingThreshold = 0.01f;

        [Header("Animation")] 
        public float playerModelRotationSpeed = 10f;

        public float rotateToTargetTime = 0.25f;

        [Header("Camera Settings")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;

        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerState _playerState;

        private Vector2 _cameraRotation = Vector2.zero;
        private Vector2 _playerTargetRotation = Vector2.zero;

        private bool _isRotatingClockwise = false;
        private float _rotatingToTargetTimer = 0f;
        private float _verticalVelocity = 0f;
        #endregion
        
        #region Startup
        private void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();
        }
        #endregion
        
        #region Update Logic
        private void Update()
        {
            UpdateMovementState();
            HandleVerticalMovement();
            HandleLateralMovement();
        }

        private void UpdateMovementState()
        {
            bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
            bool isMovingLaterally = IsMovingLaterally();
            bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMovingLaterally;
            bool isGrounded = IsGrounded();

            PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting : 
                isMovingLaterally || isMovementInput
                ? PlayerMovementState.Running
                : PlayerMovementState.Idling;
            _playerState.SetPlayerMovementState(lateralState);

            if (!isGrounded && _characterController.velocity.y >= 0f)
            {
                _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
            }
            else if (!isGrounded && _characterController.velocity.y < 0f)
            {
                _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
            }
            
        }

        private void HandleVerticalMovement()
        {
            bool isGrounded = _playerState.InGroundedState();
            
            if (isGrounded && _verticalVelocity < 0f) 
                _verticalVelocity = 0f;
            
            _verticalVelocity -= gravity * Time.deltaTime;
            
            if (_playerLocomotionInput.JumpPressed && isGrounded)
            {
                _verticalVelocity = Mathf.Sqrt(jumpSpeed * 3f * gravity);
            }
        }

        private void HandleLateralMovement()
        {
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isGrounded = _playerState.InGroundedState();
            
            float lateralAcceleration = isSprinting ? sprintAcceleration : runAcceleration;
            float clampLateralMagnitude = isSprinting ? sprintSpeed : runSpeed; 
            
            Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;
        
            Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
            Vector3 newVelocity = _characterController.velocity + movementDelta;

            Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
            newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(newVelocity, clampLateralMagnitude);
            newVelocity.y += _verticalVelocity;
            

            _characterController.Move(newVelocity * Time.deltaTime);
        }

        #endregion
        
        #region Late Update Logic
        private void LateUpdate()
        {
            UpdateCameraRotation();
        }

        private void UpdateCameraRotation()
        {
            _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

            _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;
            
            bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            IsRotatingToTarget = _rotatingToTargetTimer > 0;
            float rotationTolerance = 90f;

            if (!isIdling)
            {
                RotatePlayerToTarget();
            }
            else if (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget)
            {
                UpdateIdleRotation(rotationTolerance);
            }

            _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);
            
            Vector3 camForwardProjectedXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
            RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);
        }

        private void UpdateIdleRotation(float rotationToTolerance)
        {
            if (Mathf.Abs(RotationMismatch) > rotationToTolerance)
            {
                _rotatingToTargetTimer = rotateToTargetTime;
                _isRotatingClockwise = RotationMismatch > rotationToTolerance;
            }
            _rotatingToTargetTimer -= Time.deltaTime;

            if (_isRotatingClockwise && RotationMismatch > 0f || !_isRotatingClockwise && RotationMismatch < 0f)
            {
                RotatePlayerToTarget();
            }
        }

        private void RotatePlayerToTarget()
        {
            Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
        }
        #endregion
        
        #region State Checks
        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);

            return lateralVelocity.magnitude > movingThreshold;
        }

        private bool IsGrounded()
        {
            return _characterController.isGrounded;
        }
        #endregion
    }
}
