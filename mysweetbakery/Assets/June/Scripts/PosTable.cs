using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using static GameManager;

public class PosTable : MonoBehaviour
{
    GameManager GM;
    [SerializeField] MoneyZone moneyZone;
    [SerializeField] Customer target_customer;

    //잔고
    Stack<GameObject> moneyStack = new Stack<GameObject>();
    //돈 생성위치
    [SerializeField] private Transform money_Posision;

    //가방 생성 위치
    [SerializeField] private Transform bag_Position;



    [SerializeField] bool player_InRange;
    [SerializeField] bool isCheckingProcess;

    [SerializeField] float after_bag_Spawn;
    [SerializeField] float next_Customer;
    [SerializeField] float timewaitChecking;

    WaitForSeconds waitChecking;
    WaitForSeconds wait_After_Bag_Spawn;
    WaitForSeconds wait_Next_Customer;




    //계산 코루틴
    Coroutine check_Bill_Corutine;
    //플레이어에게 돈 옮기기 코루틴
    Coroutine collectingMoneyCoroutine;

    //트리거 관련
    [Space(10)]
    [SerializeField] GameObject triggerSizeImage;
    Coroutine triggerSizeCoroutine;


    private void Start()
    {
        GM = GameManager.Instance;

        AddTime();
    }

    //캐싱
    private void AddTime()
    {
        waitChecking = new WaitForSeconds(timewaitChecking);
        wait_After_Bag_Spawn = new WaitForSeconds(after_bag_Spawn);
        wait_Next_Customer = new WaitForSeconds(next_Customer);
    }

    private void Update()
    {
        if (player_InRange)
        {
            if (GM.postableCustomerList.Count > 0 && !isCheckingProcess && check_Bill_Corutine == null)
            {
                check_Bill_Corutine = StartCoroutine(CheckBillRoutine());
                isCheckingProcess = true;
            }
        }

        //돈 하나씩 옮기기
        if (moneyZone != null && moneyZone.isPlayerDeteced && collectingMoneyCoroutine == null && moneyStack.Count > 0)
        {
            collectingMoneyCoroutine = StartCoroutine(Utils.PlayerTakeMoneyRoutine(() => moneyStack.Count > 0 && moneyZone.isPlayerDeteced, () => moneyStack.Pop(), money => { ObjectPool.Instance.ReturnObject(DataManager.Instance.money_Prefab, money); }, () => collectingMoneyCoroutine = null));
        }



    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //튜토리얼
            if (TutorialManager.Instance.currentStep == TutorialStep.ShowBasket)
            {
                TutorialManager.Instance.currentStep = TutorialStep.PosTable;
            }

            player_InRange = true;

            //가시성 on
            if (triggerSizeCoroutine != null)
                StopCoroutine(triggerSizeCoroutine);
            triggerSizeCoroutine = StartCoroutine(GM.ScaleTriggerImage(triggerSizeImage, Vector3.one * 1.1f));

        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player_InRange = false;

            //가시성 off
            if (triggerSizeCoroutine != null)
                StopCoroutine(triggerSizeCoroutine);
            triggerSizeCoroutine = StartCoroutine(GM.ScaleTriggerImage(triggerSizeImage, Vector3.one));  // 원래 크기(1배)로 0.3초 동안 축소
        }
    }



    //계산 코루틴
    IEnumerator CheckBillRoutine()
    {
        //게임 매니저에서 관리하는 리스트에서 가져오기
        target_customer = GM.postableCustomerList[0];

        while (true)
        {
            yield return waitChecking; //대기 시간
            //고객이 움직이면 건너뛰기
            if (target_customer.isMoving)
            {
                yield return null;
                continue;
            }
            else
            {
                GM.postableCustomerList.Remove(target_customer);

                //가방생성
                GameObject bag = ObjectPool.Instance.GetObject(DataManager.Instance.bag_Prefab, bag_Position.position, Quaternion.Euler(0, 0, 0));
                yield return wait_After_Bag_Spawn;

                //크로아상 움직이기, 삭제시키기
                int croassantCount = target_customer.currentCroassantStak.Count;
                for (int i = 0; i < croassantCount; i++)
                {
                    GameObject target_Croassant = target_customer.currentCroassantStak.Pop();
                    Vector3 start = target_Croassant.transform.position;
                    Vector3 end = bag_Position.position;
                    Vector3 control = end + Vector3.up * 4f;
                    yield return StartCoroutine(Utils.MoveBezier(target_Croassant.transform, start, control, end, 0.1f));
                    ObjectPool.Instance.ReturnObject(DataManager.Instance.croassant_Prefab, target_Croassant);
                }

                //가방 움직이기
                yield return StartCoroutine(Utils.MoveBezier(bag.transform, bag.transform.position, target_customer.croassantBox.position + Vector3.up * 4f, target_customer.croassantBox.position, 0.1f));
                Utils.GetTheBag(bag, target_customer.croassantBox, Quaternion.identity);

                //고객 설정
                target_customer.isStack = true;
                target_customer.isBillFinish = true;
                isCheckingProcess = false;

                //계산 끝난 고객 추가
                GM.finishCustomerCount++;

                //다음 위치 지정
                target_customer.AddPosition(customer_Destination.turnPoint);
                target_customer.GoToHome();

                //계산
                Utils.CalculateMoney(croassantCount, money_Posision, moneyStack);

                //고객 대기줄 재계산
                GM.SortCustomersByDistance(GM.postableCustomerList, GM.posPosition, orderDiction.up_left);


                //계산 끝나고 다음 사람 기다리기
                yield return wait_Next_Customer;
                check_Bill_Corutine = null;
                break;

            }
        }


    }


}
