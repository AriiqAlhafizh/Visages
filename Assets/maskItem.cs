using UnityEngine;

public enum MaskType { Red, Blue, Yellow }

public class MaskItem : MonoBehaviour
{
    public MaskType maskType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Tell the player to equip this mask
            collision.GetComponent<PlayerController>().EquipMask(maskType);
            Destroy(gameObject); // Remove the item from the ground
        }
    }
}