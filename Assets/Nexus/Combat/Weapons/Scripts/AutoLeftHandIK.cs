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
        if (_animator == null || _leftHandGrip == null) return;

        int weaponType = _animator.GetInteger("WeaponType");
        bool isAiming = _animator.GetBool("isAiming");
        bool isReloading = _animator.GetBool("isReloading");

        float targetWeight = 0f;

        if (weaponType == 1)
            targetWeight = isAiming ? 1f : 0f;
        else if (weaponType == 3)
            targetWeight = isReloading ? 0f : 1f;

        _currentWeight = Mathf.Lerp(_currentWeight, targetWeight, Time.deltaTime * ikSmoothness);

        _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _currentWeight);
        _animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandGrip.position);
        _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
    }
}