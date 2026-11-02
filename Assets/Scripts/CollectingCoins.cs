using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectingCoins : MonoBehaviour
{
    public int NumberOfCoins { get; private set; } = 0;
    
    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private GameObject _coinsPlate;

    private void ActivateAndUpdateUI()
    {
        if (NumberOfCoins <= 0)
        {
            _coinsPlate.SetActive(false);
        }
        else
        {
            _text.text = NumberOfCoins.ToString();
            _coinsPlate.SetActive(true);
        }
    }

    public void IncreaseNumberOfCoins(int coin)
    {
        NumberOfCoins += coin;
        ActivateAndUpdateUI();
    }
}
