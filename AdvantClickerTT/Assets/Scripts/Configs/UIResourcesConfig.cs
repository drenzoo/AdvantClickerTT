using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Configs/UI/UI Resources config")]
public class UIResourcesConfig : ScriptableObject
{
    [SerializeField] private BusinessViewReference _businessViewPrefab;
    [SerializeField] private BusinessUpgradeButtonViewReference _upgradeButtonViewPrefab;
    [SerializeField] private MainScreenViewReference _mainScreenViewPrefab;

    public BusinessViewReference BusinessViewPrefab => _businessViewPrefab;
    public BusinessUpgradeButtonViewReference UpgradeButtonViewPrefab => _upgradeButtonViewPrefab;
    public MainScreenViewReference MainScreenViewPrefab => _mainScreenViewPrefab;
}