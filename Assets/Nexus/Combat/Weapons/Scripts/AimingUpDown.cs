using Nexus.FinalCharacterController;
using UnityEngine;
using UnityEngine.Serialization;

public class AimingUpDown : MonoBehaviour
{
    [FormerlySerializedAs("koscKregoslupa")]
    public Transform spineBone;

    [FormerlySerializedAs("glownaKamera")]
    public Transform mainCamera;

    [FormerlySerializedAs("kontroler")]
    public PlayerController controller;

    [FormerlySerializedAs("osWyginania")]
    public Vector3 bendAxis = new Vector3(1, 0, 0);

    [FormerlySerializedAs("mnoznik")]
    public float bendMultiplier = 1f;

    private void LateUpdate()
    {
        if (controller != null && controller.IsAiming && spineBone != null && mainCamera != null)
        {
            float verticalAimAngle = mainCamera.eulerAngles.x;
            if (verticalAimAngle > 180f) verticalAimAngle -= 360f;

            spineBone.localRotation *= Quaternion.AngleAxis(verticalAimAngle * bendMultiplier, bendAxis);
        }
    }
}
