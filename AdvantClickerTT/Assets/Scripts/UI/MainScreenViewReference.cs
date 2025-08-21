using TMPro;
using UnityEngine;

public class MainScreenViewReference : MonoBehaviour
{
    [SerializeField] private TMP_Text _headerText;
    [SerializeField] private Transform _businessesWidgetLayer;
    
    public TMP_Text HeaderText => _headerText;
    public Transform UnlocksLayer => _businessesWidgetLayer;
}