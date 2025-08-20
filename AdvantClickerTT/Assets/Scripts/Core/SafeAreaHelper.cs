using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaHelper : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _safeAreaTransform;
    
    private void OnEnable()
    {
        ApplySafeArea();
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        _safeAreaTransform = this.GetComponent<RectTransform>();
        _canvas = _safeAreaTransform.root.GetComponent<Canvas>();
    }
#endif
    
    private void ApplySafeArea()
    {
        if(_safeAreaTransform == null)
        {
            return;
        }

        var safeArea = Screen.safeArea;
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= _canvas.pixelRect.width;
        anchorMin.y /= _canvas.pixelRect.height;
        anchorMax.x /= _canvas.pixelRect.width;
        anchorMax.y /= _canvas.pixelRect.height;

        _safeAreaTransform.anchorMin = anchorMin;
        _safeAreaTransform.anchorMax = anchorMax;
    }
}
