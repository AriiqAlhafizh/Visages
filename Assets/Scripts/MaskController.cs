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
        pa.runtimeAnimatorController = activeMasks[0];
    }
    public void SetActiveMask(int id)
    {
        pa.runtimeAnimatorController = activeMasks[id];
    }

    public void SetActiveMask(MaskType maskType)
    {
        currentMaskIndex = (int)maskType;
        pa.runtimeAnimatorController = activeMasks[currentMaskIndex];
    }
}
