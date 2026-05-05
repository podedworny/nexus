using UnityEngine;

public class WaveButton : MonoBehaviour
{
    public GameObject tooltipUI;
    public Material defaultMaterial;
    public Material highlightedMaterial;
    
    private MeshRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponentInChildren<MeshRenderer>();
        if (tooltipUI != null) tooltipUI.SetActive(false);
        if (_renderer != null && defaultMaterial != null) _renderer.material = defaultMaterial;
    }

    public void SetHoverState(bool isHovered)
    {
        if (WaveManager.Instance != null && WaveManager.Instance.currentState != WaveManager.GameState.Day)
        {
            if (tooltipUI != null) tooltipUI.SetActive(false);
            if (_renderer != null && defaultMaterial != null) _renderer.material = defaultMaterial;
            return;
        }

        if (tooltipUI != null) tooltipUI.SetActive(isHovered);
        
        if (_renderer != null && highlightedMaterial != null && defaultMaterial != null)
        {
            _renderer.material = isHovered ? highlightedMaterial : defaultMaterial;
        }
    }

    public void PressButton()
    {
        if (WaveManager.Instance != null && WaveManager.Instance.currentState == WaveManager.GameState.Day)
        {
            WaveManager.Instance.StartWaveTransition();
            SetHoverState(false);
        }
    }
}