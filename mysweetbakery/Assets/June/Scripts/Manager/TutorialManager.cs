using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Ʃ�丮�� ����
public enum TutorialStep
{
    None,
    CroissantSpawner,
    ShowBasket,
    PosTable,
    ReceiveMoney,
    UnlockSeat,
    Complete
}
public class TutorialManager : MonoBehaviour
{

    public static TutorialManager Instance { get; private set; }

    [Header("Tutorial Targets")]
    public TutorialStep currentStep = TutorialStep.None;

    [Space(10)]
    [Header("UI")]
    [SerializeField] GameObject tutorialUI;
    [SerializeField] GameObject tutorialDragImage;
    Vector3 originalDragImageSize;
    [Space(5)]
    [SerializeField] RectTransform tutorialHand;
    [SerializeField] float loopSpeed = 4.0f;      // �ӵ� ����
    [SerializeField] Vector2 loopSize = new Vector2(160, 160); // �� ũ�� ���� (x, y �ݰ�)
    private float elapsedTime = 0f;
    [SerializeField] bool isDrag = false;
    public GameObject playerArrow;


    [Space(10)]
    [Header("ArrowPosition")]
    public Transform tutorialCroassantSpawner;
    public Transform tutorialShowBasket;
    public Transform tutorialPosTable;
    public Transform tutorialMoney;
    public Transform tutorialSeatTable;
    public GameObject arrowPrefab;
    public Vector3 padding = Vector3.up * 2;


    [Space(10)]
    [Header("Checker")]
    public bool isUnlockSeatShow = false;
    public bool isWallShow = false;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        originalDragImageSize = tutorialDragImage.transform.localScale;
        UpdatePositionArrow();
    }

    private void Update()
    {
        UpdatePositionArrow();
    }
    private void LateUpdate()
    {
        HandImageRound();

        SizeUpDown();

        UpdatePlayerArrow();

    }

    private void HandImageRound()
    {
        elapsedTime += Time.deltaTime * loopSpeed;

        // ���Ѵ� ���
        float x = Mathf.Cos(elapsedTime) * loopSize.x;
        float y = Mathf.Sin(elapsedTime * 2f) * loopSize.y * 0.5f;

        tutorialHand.anchoredPosition = new Vector2(x, y);
    }

    private void SizeUpDown()
    {
        if (!isDrag)
        {
            // �ð��� ���� 0.9~1.1 ���� ũ�� �ݺ�
            float scale = 1.0f + Mathf.Sin(Time.time * 3.0f) * 0.1f;
            tutorialDragImage.transform.localScale = originalDragImageSize * scale;
        }
    }

    //ȭ��ǥ ��� ��ġ�� �˷��ִ� ȭ��ǥ
    private void UpdatePlayerArrow()
    {
        if (playerArrow != null && arrowPrefab != null)
        {
            // y�� ����
            Vector3 targetPosition = arrowPrefab.transform.position;
            targetPosition.y = playerArrow.transform.position.y - 90;

            playerArrow.transform.LookAt(targetPosition);
        }
    }


    //Ʃ�丮��� ȭ��ǥ ��ġ
    public void UpdatePositionArrow()
    {

        switch (currentStep)
        {
            case TutorialStep.None:
                arrowPrefab.transform.position = tutorialCroassantSpawner.transform.position;
                break;
            case TutorialStep.CroissantSpawner:
                arrowPrefab.transform.position = tutorialShowBasket.transform.position;
                break;
            case TutorialStep.ShowBasket:
                arrowPrefab.transform.position = tutorialPosTable.transform.position;
                break;
            case TutorialStep.PosTable:
                if (GameManager.Instance.finishCustomerCount > 0)
                {
                    arrowPrefab.transform.position = tutorialMoney.transform.position;
                }
                break;
            case TutorialStep.ReceiveMoney:
                if (GameManager.Instance.playerMoneyCount >= 30)
                    arrowPrefab.transform.position = tutorialSeatTable.transform.position;
                break;
            case TutorialStep.UnlockSeat:
                playerArrow.SetActive(false);
                arrowPrefab.SetActive(false);
                gameObject.SetActive(false);
                break;
            case TutorialStep.Complete:
                break;
        }
    }

    public void isTouch()
    {
        isDrag = true;
        tutorialUI.SetActive(false);
    }

}
