using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BusinessUpgradeButtonViewReference : MonoBehaviour
{
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private Button _button;
    
    public event Action OnUnlockButtonClicked;
    
    public TMP_Text ButtonText => _buttonText;
    public Button UpgradeButton => _button;
    
    private void Awake() => _button.onClick.AddListener(() => OnUnlockButtonClicked?.Invoke());
}
