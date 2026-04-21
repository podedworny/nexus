using UnityEngine;

public class AutoLeftHandIK : MonoBehaviour
{
    private Animator _animator;
    private Transform _leftHandGrip;
    public float ikSmoothness = 10f;
    private float _currentWeight = 0f;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Szukamy LeftGrip TYLKO wewnątrz obiektu gracza (w jego hierarchii).
        // Ignorujemy resztę świata. Jak WeaponManager zespawnuje broń w ręce, to ją znajdzie.
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("LeftGrip"))
            {
                _leftHandGrip = child;
                break;
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator != null && _leftHandGrip != null)
        {
            bool celuje = _animator.GetBool("isAiming");
            float targetWeight = celuje ? 1f : 0f;

            _currentWeight = Mathf.Lerp(_currentWeight, targetWeight, Time.deltaTime * ikSmoothness);

            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _currentWeight);
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandGrip.position);
            
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
        }
    }
}