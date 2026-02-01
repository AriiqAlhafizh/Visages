using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    public AudioSource jumpSFX;
    public AudioSource dashSFX;

    public void PlayJumpSFX()
    {
        jumpSFX.Play();
    }
    public void PlayDashSFX()
    {
        dashSFX.Play();
    }

}
