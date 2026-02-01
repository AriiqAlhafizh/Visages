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

    public MaskType currentMaskType = MaskType.Base;

    private VisualElement root;
    public List<UIMaskPlaceholderSO> maskData;

    public static UIController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        root = UImaskPlaceholder.rootVisualElement.Q<VisualElement>("root");
        changeMask();

    }

    public void changeMask()
    {
        root.dataSource = maskData[(int)currentMaskType];
    }
}
