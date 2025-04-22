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

    //위치 관련
    [SerializeField] Transform chair;
    Quaternion originalChairRotation;
    [SerializeField] Transform croassant_Position;
    [SerializeField] GameObject trash;

    //대기 관련
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


    //효과
    [Tooltip("젤리 튕기듯 효과 적용할 오브젝트 배열")]
    [SerializeField] GameObject[] popUpOjects;
  //  [SerializeField] ParticleSystem cleanParticle;
  //  [SerializeField] ParticleSystem appearParticle;


    private Coroutine seatCoroutine;
    private Coroutine collectingMoneyCoroutine;

    //트리거 핸들러
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

            //unlock 효과
            SoundManager.instance.PlayAudio(audioSource.Success);

            //appearParticle.Play();
            StartCoroutine(Utils.PopUp(popUpOjects));

            //튜토리얼
            if (!TutorialManager.Instance.isWallShow)
            {
                TutorialManager.Instance.isWallShow = true;
                GM.CM.Show(CamEnum.wall);
            }

            //트리거 꺼주기
            seatTriggerHandler.isON = false;

        }



        //돈 걷기
        if (moneyZone != null && moneyZone.isPlayerDeteced && collectingMoneyCoroutine == null)
        {
            collectingMoneyCoroutine = StartCoroutine(Utils.PlayerTakeMoneyRoutine(
                () => moneyStack.Count > 0 && moneyZone.isPlayerDeteced,
                () => moneyStack.Pop(),
                money => { ObjectPool.Instance.ReturnObject(DataManager.Instance.money_Prefab, money); },
                () => collectingMoneyCoroutine = null

                ));

        }

        //먹고 가기 체크
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
            //튜토리얼
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

        //크로아상 비주얼 보이기
        croassant_Position.gameObject.SetActive(true);

        for (int i = 0; i < current_customer.currentCroassantStak.Count + 1; i++)
            ObjectPool.Instance.ReturnObject(DataManager.Instance.croassant_Prefab, current_customer.currentCroassantStak.Pop());

        //고객 옮기기
        current_customer.AddFreePosition(chair);
        current_customer.transform.localRotation = Quaternion.identity;

        yield return max_Time;

        //계산
        current_customer.isBillFinish = true;
        GM.finishCustomerCount++;
        Utils.CalculateMoney(current_customer.croassantNeed, moneyZone.transform, moneyStack);

        //집으로 돌려 보내기
        current_customer.GoToHome();

        //초기화
        croassant_Position.gameObject.SetActive(false);
        trash.SetActive(true);
        GM.tableCustomerList.Remove(current_customer);
        current_customer.isSeat = false;
        chair.transform.Rotate(new Vector3(0, 45, 0));
        yield return wait_Next_Customer;

        //대기줄 업데이트
        GM.SortCustomersByDistance(GM.tableCustomerList, GM.seatPosition, orderDiction.down_left);
        isUsingTable = false;
        isClean = false;
        isSeat = false;
        current_customer = null;
        seatCoroutine = null;



    }
    public void CleanTrash()
    {
        // 정리하기
        isClean = true;
        trash.SetActive(false); // 쓰레기 사라지게

        //SoundManager.instance.PlayAudio(audioSource.trash);
        SoundManager.instance.PlayAudio(audioSource.clean);
        //cleanParticle.Play();

    }

}

