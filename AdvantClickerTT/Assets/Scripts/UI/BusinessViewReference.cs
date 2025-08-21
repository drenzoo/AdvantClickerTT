using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BusinessViewReference : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_Text _headerText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _incomeText;
    [SerializeField] private TMP_Text _buttontext;
    [SerializeField] private Button _levelUpButton;
    [SerializeField] private Transform _unlocksLayer;
    
    public event Action OnLevelUpClicked;
    
    public Slider Slider => _slider;
    public TMP_Text HeaderText => _headerText;
    public TMP_Text LevelText => _levelText;
    public TMP_Text IncomeText => _incomeText;
    public TMP_Text ButtonText => _buttontext;
    public Button LevelUpButton => _levelUpButton;
    public Transform UnlocksLayer => _unlocksLayer;
    
    private void Awake() => _levelUpButton.onClick.AddListener(() => OnLevelUpClicked?.Invoke());
}
