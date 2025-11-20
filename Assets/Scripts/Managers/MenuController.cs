using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;

    private void Start()
    {
        menuCanvas.SetActive(false);
    }

    public void OnToggleMenu(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            menuCanvas.SetActive(!menuCanvas.activeSelf);
    }
}
