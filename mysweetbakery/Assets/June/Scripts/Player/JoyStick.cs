using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Space(10)]
    [Header("���̽�ƽ")]
    [SerializeField] private RectTransform joyStick_BG;
    [SerializeField] private RectTransform joyStick_Main;

    public Vector2 joySticVector;

    private float joystick_Radius;

    void Start()
    {

        joystick_Radius = joyStick_BG.rect.width * 0.35f;

    }

    void OnTouch(Vector2 touchVe2)
    {
        //���̽�ƽ ����
        Vector2 vec = new Vector2(touchVe2.x - joyStick_BG.position.x, touchVe2.y - joyStick_BG.position.y);
        //���̽�ƽ �ݰ� ����
        vec = Vector2.ClampMagnitude(vec, joystick_Radius);

        joyStick_Main.localPosition = vec;

        float temp = (joyStick_BG.position - joyStick_Main.position).sqrMagnitude / (joystick_Radius * joystick_Radius);

        joySticVector = vec.normalized;


    }
    public void OnPointerDown(PointerEventData eventData)
    {
        //Ʃ�丮��
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.isTouch();
        }
        joyStick_BG.gameObject.SetActive(true);
        joyStick_BG.position = eventData.position;
        OnTouch(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnTouch(eventData.position);

    }


    public void OnPointerUp(PointerEventData eventData)
    {
        joyStick_BG.gameObject.SetActive(false);

        joySticVector = Vector2.zero;
    }
}
