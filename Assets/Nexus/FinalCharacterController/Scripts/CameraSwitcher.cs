using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nexus.FinalCharacterController
{
    public class CameraSwitcher : MonoBehaviour, PlayerControls.IThirdPersonMapActions
    {
        [SerializeField] private GameObject _tppCamera;
        [SerializeField] private GameObject _fppCamera;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private LayerMask _tppMask;
        [SerializeField] private LayerMask _fppMask;
        
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private PlayerState _playerState;
        [SerializeField] private WeaponManager _weaponManager;

        [SerializeField] private float _hideHeadDelay = 0.3f;

        private Coroutine _hideHeadCoroutine;

        private void OnEnable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null) return;
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Enable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null) return;
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.RemoveCallbacks(this);
        }

        private void Update()
        {
            bool shouldBeAiming = _playerController != null && _playerController.IsAiming && (_weaponManager != null && _weaponManager.CurrentWeaponIndex > 0);

            if (shouldBeAiming && !_fppCamera.activeSelf)
            {
                EnterAimCamera();
            }
            else if (!shouldBeAiming && _fppCamera.activeSelf)
            {
                ExitAimCamera();
            }
        }

        public void OnAim(InputAction.CallbackContext context)
        {
        }

        private void EnterAimCamera()
        {
            _tppCamera.SetActive(false);
            _fppCamera.SetActive(true);
            
            if (_hideHeadCoroutine != null) StopCoroutine(_hideHeadCoroutine);
            _hideHeadCoroutine = StartCoroutine(HideHeadAfterDelay());
        }

        private void ExitAimCamera()
        {
            _fppCamera.SetActive(false);
            _tppCamera.SetActive(true);
            
            if (_hideHeadCoroutine != null) StopCoroutine(_hideHeadCoroutine);
            _mainCamera.cullingMask = _tppMask;
        }

        private IEnumerator HideHeadAfterDelay()
        {
            yield return new WaitForSeconds(_hideHeadDelay);
            _mainCamera.cullingMask = _fppMask;
        }

        public void OnScrollCamera(InputAction.CallbackContext context) {}
    }
}