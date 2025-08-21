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
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private Transform _unlocksLayer;
    
    public event Action OnUpgradeLevelClicked;
    
    public Slider Slider => _slider;
    public TMP_Text HeaderText => _headerText;
    public TMP_Text LevelText => _levelText;
    public TMP_Text IncomeText => _incomeText;
    public Button UpgradeButton => _upgradeButton;
    public Transform UnlocksLayer => _unlocksLayer;
    
    private void Awake() => _upgradeButton.onClick.AddListener(() => OnUpgradeLevelClicked?.Invoke());
}
