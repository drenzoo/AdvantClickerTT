using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MainScreenViewReference : MonoBehaviour
{
    [SerializeField] private TMP_Text _headerText;
    [SerializeField] private Transform businessWidgetLayer;
    
    public TMP_Text HeaderText => _headerText;
    public Transform BusinessLayer => businessWidgetLayer;
}