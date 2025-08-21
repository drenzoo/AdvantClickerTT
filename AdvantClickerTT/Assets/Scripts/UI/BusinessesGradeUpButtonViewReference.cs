using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BusinessesGradeUpButtonViewReference : MonoBehaviour
{
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private Button _button;
    
    public event Action OnUnlockButtonClicked;
    
    public TMP_Text IncomeText => _buttonText;
    public Button UpgradeButton => _button;
    
    private void Awake() => _button.onClick.AddListener(() => OnUnlockButtonClicked?.Invoke());
}
