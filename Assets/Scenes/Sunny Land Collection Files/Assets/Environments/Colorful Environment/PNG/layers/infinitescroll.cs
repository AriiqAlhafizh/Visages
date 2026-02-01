using UnityEngine;

public class InfiniteScroller : MonoBehaviour
{
    [Header("Settings")]
    public float scrollSpeed = 0.5f; // Adjust for speed

    private Material mat;
    private Vector2 offset;

    void Start()
    {
        // Grab the material from the renderer
        // Note: This works best if your background is a 'Quad' 
        // or a Sprite using a material that supports tiling.
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // Calculate the offset based on time
        // x is for horizontal movement
        offset.x += scrollSpeed * Time.deltaTime;

        // Apply the offset to the texture
        mat.mainTextureOffset = offset;
    }
}