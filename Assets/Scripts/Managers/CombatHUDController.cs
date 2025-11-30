using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CombatHUDController : MonoBehaviour
{
    public RectTransform handPointer;
    public Vector2 offset = new Vector2(-40f, 0f);

    public Button moveButton;
    public Button attackButton;

    public bool moveModeActive = false;


    void Start()
    {
        AddHover(moveButton);
        AddHover(attackButton);

        handPointer.gameObject.SetActive(false);
    }

    void AddHover(Button btn)
    {
        EventTrigger trigger = btn.GetComponent<EventTrigger>();
        if (!trigger)
            trigger = btn.gameObject.AddComponent<EventTrigger>();

        // Hover Enter
        var enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) =>
            OnOptionHover(btn.GetComponent<RectTransform>())
        );
        trigger.triggers.Add(enter);

        // Hover Exit (opcional, mas recomendado)
        var exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) =>
            handPointer.gameObject.SetActive(false)
        );
        trigger.triggers.Add(exit);
    }

    void OnOptionHover(RectTransform target)
    {
        handPointer.gameObject.SetActive(true);

        // Torna a mão filha do botão
        handPointer.SetParent(target, false);

        // Alinha verticalmente e coloca um pouco à esquerda
        handPointer.anchorMin = new Vector2(0, 0.5f);
        handPointer.anchorMax = new Vector2(0, 0.5f);
        handPointer.pivot = new Vector2(0.5f, 0.5f);

        handPointer.anchoredPosition = offset;
    }
    public void OnMoveButton()
    {
        moveModeActive = !moveModeActive;

        Player p = Player.ActivePlayer;

        foreach (Unit u in p.playerUnits)
            u.SetHighlight(moveModeActive);
    }
}
