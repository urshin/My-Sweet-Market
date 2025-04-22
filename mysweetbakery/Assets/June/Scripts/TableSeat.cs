using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class TableSeat : MonoBehaviour
{
    GameManager GM;
    [SerializeField] MoneyZone moneyZone;


    [SerializeField] Customer current_customer;
    [SerializeField] Stack<GameObject> moneyStack = new Stack<GameObject>();

    //��ġ ����
    [SerializeField] Transform chair;
    Quaternion originalChairRotation;
    [SerializeField] Transform croassant_Position;
    [SerializeField] GameObject trash;

    //��� ����
    [SerializeField] private float max_Seat_Time;
    [SerializeField] private float next_Customer;
    private WaitForSeconds max_Time;
    private WaitForSeconds wait_Next_Customer;


    [Space(10)]
    [Header("Checker")]
    bool isSeat;
    bool isUsingTable;
    [SerializeField] bool isClean;

    [SerializeField] GameObject lockSeat;
    [SerializeField] GameObject unlockSeat;


    //ȿ��
    [Tooltip("���� ƨ��� ȿ�� ������ ������Ʈ �迭")]
    [SerializeField] GameObject[] popUpOjects;
  //  [SerializeField] ParticleSystem cleanParticle;
  //  [SerializeField] ParticleSystem appearParticle;


    private Coroutine seatCoroutine;
    private Coroutine collectingMoneyCoroutine;

    //Ʈ���� �ڵ鷯
    [SerializeField] TriggerHandler seatTriggerHandler;


    void Start()
    {
        GM = GameManager.Instance;
        moneyZone = GetComponentInChildren<MoneyZone>();

        max_Time = new WaitForSeconds(max_Seat_Time);
        wait_Next_Customer = new WaitForSeconds(next_Customer);

        isSeat = false;
        isClean = true;
        isUsingTable = false;
        originalChairRotation = chair.rotation;
    }

    private void Update()
    {

        if (seatTriggerHandler != null && seatTriggerHandler.isON)
        {
            GM.isUnlockSeat = true;

            //unlock ȿ��
            SoundManager.instance.PlayAudio(audioSource.Success);

            //appearParticle.Play();
            StartCoroutine(Utils.PopUp(popUpOjects));

            //Ʃ�丮��
            if (!TutorialManager.Instance.isWallShow)
            {
                TutorialManager.Instance.isWallShow = true;
                GM.CM.Show(CamEnum.wall);
            }

            //Ʈ���� ���ֱ�
            seatTriggerHandler.isON = false;

        }



        //�� �ȱ�
        if (moneyZone != null && moneyZone.isPlayerDeteced && collectingMoneyCoroutine == null)
        {
            collectingMoneyCoroutine = StartCoroutine(Utils.PlayerTakeMoneyRoutine(
                () => moneyStack.Count > 0 && moneyZone.isPlayerDeteced,
                () => moneyStack.Pop(),
                money => { ObjectPool.Instance.ReturnObject(DataManager.Instance.money_Prefab, money); },
                () => collectingMoneyCoroutine = null

                ));

        }

        //�԰� ���� üũ
        if (GM.tableCustomerList.Count > 0 && !isSeat)
        {
            current_customer = GM.tableCustomerList[0];
            if (!isUsingTable && isClean && !current_customer.isMoving)
            {
                isUsingTable = true;
                if (seatCoroutine == null)
                    seatCoroutine = StartCoroutine(SeatTimer());
            }

        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Ʃ�丮��
            if (TutorialManager.Instance.currentStep == TutorialStep.ReceiveMoney)
            {
                TutorialManager.Instance.currentStep = TutorialStep.UnlockSeat;
                TutorialManager.Instance.UpdatePositionArrow();
            }
            if (!isClean)
            {
                CleanTrash();
            }
        }
    }

    private IEnumerator SeatTimer()
    {

        yield return new WaitForSeconds(0.2f);

        isSeat = true;
        current_customer.isSeat = true;
        chair.transform.rotation = originalChairRotation;

        //ũ�ξƻ� ���־� ���̱�
        croassant_Position.gameObject.SetActive(true);

        for (int i = 0; i < current_customer.currentCroassantStak.Count + 1; i++)
            ObjectPool.Instance.ReturnObject(DataManager.Instance.croassant_Prefab, current_customer.currentCroassantStak.Pop());

        //�� �ű��
        current_customer.AddFreePosition(chair);
        current_customer.transform.localRotation = Quaternion.identity;

        yield return max_Time;

        //���
        current_customer.isBillFinish = true;
        GM.finishCustomerCount++;
        Utils.CalculateMoney(current_customer.croassantNeed, moneyZone.transform, moneyStack);

        //������ ���� ������
        current_customer.GoToHome();

        //�ʱ�ȭ
        croassant_Position.gameObject.SetActive(false);
        trash.SetActive(true);
        GM.tableCustomerList.Remove(current_customer);
        current_customer.isSeat = false;
        chair.transform.Rotate(new Vector3(0, 45, 0));
        yield return wait_Next_Customer;

        //����� ������Ʈ
        GM.SortCustomersByDistance(GM.tableCustomerList, GM.seatPosition, orderDiction.down_left);
        isUsingTable = false;
        isClean = false;
        isSeat = false;
        current_customer = null;
        seatCoroutine = null;



    }
    public void CleanTrash()
    {
        // �����ϱ�
        isClean = true;
        trash.SetActive(false); // ������ �������

        //SoundManager.instance.PlayAudio(audioSource.trash);
        SoundManager.instance.PlayAudio(audioSource.clean);
        //cleanParticle.Play();

    }

}

