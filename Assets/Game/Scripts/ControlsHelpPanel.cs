using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ControlsHelpPanel : MonoBehaviour
{
    [SerializeField] private GameObject controlsPanel;

    private void Start()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.hKey.wasPressedThisFrame)
            TogglePanel();
    }

    private void TogglePanel()
    {
        if (controlsPanel == null) return;

        controlsPanel.SetActive(!controlsPanel.activeSelf);
    }
}