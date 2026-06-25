using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ControlType
    {
        Left,
        Right,
        Jump
    }

    [SerializeField] private ControlType controlType;

    private PlayerMovement player;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Initialize(PlayerMovement targetPlayer, ControlType type)
    {
        player = targetPlayer;
        controlType = type;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (player == null)
        {
            return;
        }

        if (controlType == ControlType.Left)
        {
            player.SetMobileHorizontal(-1f);
        }
        else if (controlType == ControlType.Right)
        {
            player.SetMobileHorizontal(1f);
        }
        else
        {
            player.QueueMobileJump();
        }

        transform.localScale = new Vector3(0.94f, 0.94f, 1f);
        if (image != null)
        {
            image.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (player == null)
        {
            return;
        }

        if (controlType != ControlType.Jump)
        {
            player.SetMobileHorizontal(0f);
        }

        transform.localScale = Vector3.one;
        if (image != null)
        {
            image.color = new Color(1f, 1f, 1f, 0.85f);
        }
    }
}
