using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public enum MaskType
{
    Base,
    Dash,
    DoubleJump,
    WallClimb,
    DashJump,
    DashClimb,
    JumpClimb,
}
public class UIController : MonoBehaviour
{
    [SerializeField] private UIDocument UImaskPlaceholder;

    private MaskType currentMaskType = MaskType.Base;

    private VisualElement root;
    public List<UIMaskPlaceholderSO> maskData;
    private void Start()
    {
        root = UImaskPlaceholder.rootVisualElement.Q<VisualElement>("root");
        changeMask(0);

    }

    public void changeMask(int index)
    {
        switch (index)
        {
            case 0:
                currentMaskType = MaskType.Base;
                break;
            case 1:
                currentMaskType = MaskType.Dash;
                break;
            case 2:
                currentMaskType = MaskType.DoubleJump;
                break;
            case 3:
                currentMaskType = MaskType.WallClimb;
                break;
            case 4:  
                currentMaskType = MaskType.DashJump;
                break;
            case 5:
                currentMaskType = MaskType.DashClimb;
                break;
            case 6:
                currentMaskType = MaskType.JumpClimb;
                break;
        }

        root.dataSource = maskData[(int)currentMaskType];
    }
}
