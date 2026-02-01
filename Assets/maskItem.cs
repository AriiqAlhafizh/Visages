using UnityEngine;

public class MaskItem : MonoBehaviour
{
    public MaskType maskType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Tell the player to equip this mask
            collision.GetComponent<PlayerController>().EquipMask(maskType);
            UIController.instance.currentMaskType = maskType;
            UIController.instance.changeMask();
            Destroy(gameObject); // Remove the item from the ground
        }
    }

}