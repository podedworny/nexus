using Nexus.FinalCharacterController;
using UnityEngine;

public class AimingUpDown : MonoBehaviour
{
    public Transform koscKregoslupa;
    public Transform glownaKamera;
    public PlayerController kontroler;
    public Vector3 osWyginania = new Vector3(1, 0, 0);
    public float mnoznik = 1f;

    void LateUpdate()
    {
        if (kontroler != null && kontroler.IsAiming && koscKregoslupa != null && glownaKamera != null)
        {
            float katGoraDol = glownaKamera.eulerAngles.x;
            if (katGoraDol > 180f) katGoraDol -= 360f;

            koscKregoslupa.localRotation *= Quaternion.AngleAxis(katGoraDol * mnoznik, osWyginania);
        }
    }
}