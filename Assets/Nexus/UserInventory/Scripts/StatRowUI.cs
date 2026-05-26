using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatRowUI : MonoBehaviour
{
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI valueText;
    public Image fillImage;

    public void Setup(ItemStat stat)
    {
        if (labelText != null) labelText.text = stat.statName;
        if (valueText != null) valueText.text = stat.statValue;
        
        if (fillImage != null)
        {
            fillImage.gameObject.SetActive(stat.showBar);
            fillImage.fillAmount = stat.fillPercentage;
            fillImage.color = stat.barColor;
        }
    }
}