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

    [Tooltip("Czas w sekundach po którym głowa zniknie. Ustaw na taki sam, jak czas przejścia w Cinemachine Brain.")]
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

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.started) 
        {
            _tppCamera.SetActive(false);
            _fppCamera.SetActive(true);
            
            if (_playerController != null) _playerController.IsAiming = true;
            
            if (_hideHeadCoroutine != null) StopCoroutine(_hideHeadCoroutine);
            _hideHeadCoroutine = StartCoroutine(HideHeadAfterDelay());
        }
        else if (context.canceled) 
        {
            _fppCamera.SetActive(false);
            _tppCamera.SetActive(true);
            
            if (_playerController != null) _playerController.IsAiming = false;
            
            if (_hideHeadCoroutine != null) StopCoroutine(_hideHeadCoroutine);
            
            _mainCamera.cullingMask = _tppMask;
        }
    }

    private IEnumerator HideHeadAfterDelay()
    {
        yield return new WaitForSeconds(_hideHeadDelay);
        _mainCamera.cullingMask = _fppMask;
    }

    public void OnScrollCamera(InputAction.CallbackContext context) {}
}
}