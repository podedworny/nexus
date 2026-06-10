using System.Linq;
using UnityEngine;

namespace Nexus.FinalCharacterController
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private float locomotionBlendSpeed = 4f;

        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerState _playerState;
        private PlayerController _playerController;
        private PlayerActionsInput _playerActionsInput;

        private static int inputXHash = Animator.StringToHash("inputX");
        private static int inputYHash = Animator.StringToHash("inputY");
        private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
        private static int isIdlingHash = Animator.StringToHash("isIdling");
        private static int isGroundedHash = Animator.StringToHash("isGrounded");
        private static int isFallingHash = Animator.StringToHash("isFalling");
        private static int isJumpingHash = Animator.StringToHash("isJumping");
        private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
        private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");
        private static int isAttackingHash = Animator.StringToHash("isAttacking");
        private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");
        private static int hitHash = Animator.StringToHash("Hit");
        private static int dieHash = Animator.StringToHash("Die");

        private int[] actionHashes = new int[0];

        private Vector3 _currentBlendInput = Vector3.zero;

        private float _sprintMaxBlendValue = 1.5f;
        private float _runMaxBlendValue = 1.0f;
        private float _walkMaxBlendValue = 0.5f;

        private bool _isDead = false;

        private void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();
            _playerController = GetComponent<PlayerController>();
            _playerActionsInput = GetComponent<PlayerActionsInput>();
        }

        private void Update()
        {
            if (_isDead) return;
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            bool isRunning = _playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
            bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
            bool isGrounded = _playerState.InGroundedState();
            bool isPlayingAction = actionHashes.Any(hash => _animator.GetBool(hash));

            bool isRunBlendValue = isRunning || isJumping || isFalling;

            Vector2 inputTarget = isSprinting ? _playerLocomotionInput.MovementInput * _sprintMaxBlendValue :
                                  isRunBlendValue ? _playerLocomotionInput.MovementInput * _runMaxBlendValue :
                                                    _playerLocomotionInput.MovementInput * _walkMaxBlendValue;

            _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

            _animator.SetBool(isGroundedHash, isGrounded);
            _animator.SetBool(isIdlingHash, isIdling);
            _animator.SetBool(isFallingHash, isFalling);
            _animator.SetBool(isJumpingHash, isJumping);
            _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);

            if (_playerActionsInput != null)
            {
                _animator.SetBool(isAttackingHash, _playerActionsInput.AttackPressed);
            }

            _animator.SetBool(isPlayingActionHash, isPlayingAction);

            _animator.SetFloat(inputXHash, _currentBlendInput.x);
            _animator.SetFloat(inputYHash, _currentBlendInput.y);
            _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
            _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
        }

        public void TriggerHit()
        {
            _animator.SetTrigger(hitHash);
        }

        public void TriggerDie()
        {
            _isDead = true;

            _animator.SetBool(isIdlingHash, false);
            _animator.SetBool(isFallingHash, false);
            _animator.SetBool(isJumpingHash, false);
            _animator.SetBool(isGroundedHash, true);
            _animator.SetBool(isRotatingToTargetHash, false);
            _animator.SetBool(isAttackingHash, false);
            _animator.SetBool(isPlayingActionHash, false);
            _animator.SetFloat(inputXHash, 0f);
            _animator.SetFloat(inputYHash, 0f);
            _animator.SetFloat(inputMagnitudeHash, 0f);
            _animator.SetFloat(rotationMismatchHash, 0f);

            _animator.ResetTrigger(hitHash);
            _animator.SetTrigger(dieHash);

            int deathLayerIndex = _animator.GetLayerIndex("Death");

            for (int i = 1; i < _animator.layerCount; i++)
            {
                if (i == deathLayerIndex)
                {
                    _animator.SetLayerWeight(i, 1f);
                }
                else
                {
                    _animator.SetLayerWeight(i, 0f);
                }
            }
        }

        public void TriggerRespawn()
        {
            _isDead = false;
            _animator.Rebind();
            _animator.Update(0f);

            int deathLayerIndex = _animator.GetLayerIndex("Death");

            for (int i = 1; i < _animator.layerCount; i++)
            {
                if (i == deathLayerIndex)
                {
                    _animator.SetLayerWeight(i, 0f);
                }
                else
                {
                    _animator.SetLayerWeight(i, 1f);
                }
            }
        }
    }
}
