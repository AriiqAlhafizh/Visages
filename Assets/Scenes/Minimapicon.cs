using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    public enum IconType { Player, Enemy, MapElement }
    public IconType type;
    public Sprite iconSprite; // Use a simple square or circle sprite

    void Start()
    {
        // Create a child object that only the Minimap Camera can see
        GameObject icon = new GameObject("MinimapIcon");
        icon.transform.SetParent(this.transform);
        icon.transform.localPosition = new Vector3(0, 0, 0);
        icon.layer = LayerMask.NameToLayer("Minimap");

        SpriteRenderer sr = icon.AddComponent<SpriteRenderer>();
        sr.sprite = iconSprite;

        // Set the colors as requested
        if (type == IconType.Player) sr.color = Color.blue;
        else if (type == IconType.Enemy) sr.color = Color.red;
        else if (type == IconType.MapElement) sr.color = Color.white;

        // Ensure the icon stays at a consistent size on the map
        icon.transform.localScale = Vector3.one * 1.5f;
    }
}