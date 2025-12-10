using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRectClipper : MonoBehaviour
{
    private static readonly int ClipRectID = Shader.PropertyToID("_ClipRect");
    private static readonly int ClipRectEnabledID = Shader.PropertyToID("_ClipRectEnabled");
    
    private SpriteRenderer parentRenderer;
    private Bounds parentBounds;
    private MaterialPropertyBlock propertyBlock;
    private Material clipMaterial;
    
    private void Awake()
    {
        parentRenderer = GetComponent<SpriteRenderer>();
        clipMaterial = parentRenderer.sharedMaterial;
        
        UpdateBounds();
    }
    
    private void UpdateBounds()
    {
        if (parentRenderer != null && parentRenderer.sprite != null)
        {
            parentBounds = parentRenderer.bounds;
            
            Vector4 clipRect = new Vector4(
                parentBounds.min.x,
                parentBounds.min.y,
                parentBounds.max.x,
                parentBounds.max.y
            );
            
            clipMaterial.SetVector(ClipRectID, clipRect);
            clipMaterial.SetFloat(ClipRectEnabledID, 1f);
        }
    }
}

