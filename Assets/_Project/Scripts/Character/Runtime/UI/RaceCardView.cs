using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceCardView : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI descriptionLabel;
    [SerializeField] private Button button;

    public void Setup(RaceDefinitionSO race, Action onSelected)
    {
        portraitImage.sprite = race.portraitIcon;
        nameLabel.text = race.displayName;
        descriptionLabel.text = race.description;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected());
    }
}