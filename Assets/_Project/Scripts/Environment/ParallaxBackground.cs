using UnityEngine;

/// <summary>
/// Implements a high-performance 4-layer parallax scrolling background.
/// Custom-tailored for mobile iOS gameplay using seamless sprite repetition and screen-teleportation.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public struct ParallaxLayer
    {
        [Header("Layer Configuration")]
        public string name;
        [Tooltip("The parent GameObject containing the sprites for this layer")]
        public GameObject layerParent;
        [Tooltip("Left tile sprite renderer")]
        public SpriteRenderer spriteA;
        [Tooltip("Right tile sprite renderer")]
        public SpriteRenderer spriteB;
        [Tooltip("Parallax coefficient: 0 = static (moves with camera), 1 = full movement")]
        public float parallaxFactor;
        
        [HideInInspector] public float startX;
        [HideInInspector] public float spriteWidth;
    }

    [Header("Target Tracking")]
    [SerializeField] private Transform targetCamera;

    [Header("Parallax Layers (Indices: 0=Floor, 1=Far, 2=MidArch, 3=Props)")]
    [SerializeField] private ParallaxLayer[] layers = new ParallaxLayer[4];

    private void Start()
    {
        // Fallback to Main Camera if not assigned in Inspector
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }

        // Initialize layers
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerParent == null || layers[i].spriteA == null || layers[i].spriteB == null)
                continue;

            layers[i].startX = layers[i].layerParent.transform.position.x;
            float localWidth = layers[i].spriteA.bounds.size.x;
            float scaleX = layers[i].layerParent.transform.lossyScale.x;
            if (scaleX > 0.0001f)
            {
                localWidth /= scaleX;
            }
            layers[i].spriteWidth = localWidth;

            // Position Sprite B immediately adjacent to Sprite A
            layers[i].spriteA.transform.localPosition = Vector3.zero;
            layers[i].spriteB.transform.localPosition = new Vector3(layers[i].spriteWidth, 0f, 0f);
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        float camX = targetCamera.position.x;

        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerParent == null || layers[i].spriteA == null)
                continue;

            // Floor layer (Index 0) is static horizontally and anchored to bottom
            if (i == 0)
            {
                // Floor moves 1:1 with camera X so it stays centered, and maintains static Y
                Vector3 floorPos = layers[i].layerParent.transform.position;
                layers[i].layerParent.transform.position = new Vector3(camX, floorPos.y, floorPos.z);
                continue;
            }

            // Normal parallax layers (horizontal scrolling with loop)
            float factor = layers[i].parallaxFactor;
            float width = layers[i].spriteWidth;

            // Relative distance scrolled relative to the layer start
            float temp = camX * (1 - factor);
            float dist = camX * factor;

            // Update parent X position
            Vector3 pos = layers[i].layerParent.transform.position;
            layers[i].layerParent.transform.position = new Vector3(layers[i].startX + dist, pos.y, pos.z);

            // Teleport layer origin for infinite horizontal tile scrolling
            if (temp > layers[i].startX + width)
            {
                layers[i].startX += width;
            }
            else if (temp < layers[i].startX - width)
            {
                layers[i].startX -= width;
            }
        }
    }
}
