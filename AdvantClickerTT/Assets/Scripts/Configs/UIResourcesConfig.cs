using UnityEngine;

[CreateAssetMenu(menuName = "Configs/UI/UI Resources config")]
public class UIResourcesConfig : ScriptableObject
{
    [SerializeField] private BusinessViewReference _businessViewPrefab;
    [SerializeField] private BusinessesGradeUpButtonViewReference _gradeButtonViewPrefab;
    [SerializeField] private MainScreenViewReference _mainScreenViewPrefab;

    public BusinessViewReference BusinessViewPrefab => _businessViewPrefab;
    public BusinessesGradeUpButtonViewReference GradeButtonViewPrefab => _gradeButtonViewPrefab;
    public MainScreenViewReference MainScreenViewPrefab => _mainScreenViewPrefab;
}