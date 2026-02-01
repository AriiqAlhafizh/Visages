using UnityEditor.Animations;
using UnityEngine;

public class MaskController : MonoBehaviour
{
    private Animator pa; // player animator
    public int currentMaskIndex = 0;
    public AnimatorController[] activeMasks;

    private void Start()
    {
        pa = GetComponent<Animator>();
    }
    public void SetActiveMask(int id)
    {
        pa.runtimeAnimatorController = activeMasks[id];
    }
}
