using UnityEngine;
using System.Collections;

public class DayNightController : MonoBehaviour
{
    public Light directionalLight;
    public float transitionDuration = 5f;
    public float maxIntensity = 1f;

    private float _currentX;
    private float _initialY;
    private float _initialZ;

    private void Start()
    {
        _currentX = directionalLight.transform.eulerAngles.x;
        _initialY = directionalLight.transform.eulerAngles.y;
        _initialZ = directionalLight.transform.eulerAngles.z;
    }

    public IEnumerator RotateSun(bool toNight)
    {
        float startX = _currentX;
        float targetX = startX - 180f; 
        
        float timeElapsed = 0f;

        while (timeElapsed < transitionDuration)
        {
            float t = timeElapsed / transitionDuration;
            
            _currentX = Mathf.Lerp(startX, targetX, t);

            directionalLight.transform.rotation = Quaternion.Euler(_currentX, _initialY, _initialZ);

            float sunAngle = Vector3.Dot(directionalLight.transform.forward, Vector3.down);
            directionalLight.intensity = Mathf.Clamp01(sunAngle * 5f) * maxIntensity;
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        _currentX = targetX;
        if (_currentX <= -360f) 
        {
            _currentX += 360f;
        }
        
        directionalLight.transform.rotation = Quaternion.Euler(_currentX, _initialY, _initialZ);
        
        float finalAngle = Vector3.Dot(directionalLight.transform.forward, Vector3.down);
        directionalLight.intensity = Mathf.Clamp01(finalAngle * 5f) * maxIntensity;
    }
}