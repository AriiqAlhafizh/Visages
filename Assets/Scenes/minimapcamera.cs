using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        if (player != null)
        {
            // Follow player but stay high up on the Z axis (or Y if 3D)
            transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
        }
    }
}