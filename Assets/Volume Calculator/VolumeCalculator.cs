using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeCalculator : MonoBehaviour
{
    [SerializeField] private TMP_InputField stoplossDistanceInput;
    [SerializeField] private TMP_InputField stoplossAmountInput;
    [SerializeField] private TMP_Text output;
    [SerializeField] private Button calculateButton;

    void Awake()
    {
        calculateButton.onClick.AddListener(Calculate);
    }

    private void Calculate()
    {
        float stoplossDistance = float.Parse(stoplossDistanceInput.text);
        float stoplossAmount = float.Parse(stoplossAmountInput.text);

        float profitPerVolumePerPriceDistance = 100;

        // stoplossAmount = profitPerVolumePerPriceDistance * volume * priceDistance
        // volume = stoplossAmount / (profitPerVolumePerPriceDistance * priceDistance)

        float volume = stoplossAmount / (profitPerVolumePerPriceDistance * stoplossDistance);

        output.text = $"Volume: {volume:F2}";
    }
}
