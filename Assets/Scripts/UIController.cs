using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] private UIDocument UImaskPlaceholder;

    private VisualElement root;
    public List<UIMaskPlaceholderSO> maskData;
    private void Start()
    {
        root = UImaskPlaceholder.rootVisualElement.Q<VisualElement>("root");
    }




}
