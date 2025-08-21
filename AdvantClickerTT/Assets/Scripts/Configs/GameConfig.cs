using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game/Game Config")]
public sealed class GameConfig : ScriptableObject
{
    [SerializeField] private BusinessConfig[] _businesses;
    public BusinessConfig[] Businesses => _businesses;
}