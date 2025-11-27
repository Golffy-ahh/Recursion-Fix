using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ChoiceCard : MonoBehaviour
{
    [Header("Assign at least Artwork + Button")]
    public Image artwork;
    public Text  titleText;   // optional
    public Text  descText;    // optional
    public Button button;

    public void Bind(MapNode node, UnityAction onClick)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (artwork)   artwork.sprite = node.Image;
        if (titleText) titleText.text = node.Title;
        if (descText)  descText.text  = node.Description;

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
        }
    }

    public void Clear()
    {
        if (button) button.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }
}
