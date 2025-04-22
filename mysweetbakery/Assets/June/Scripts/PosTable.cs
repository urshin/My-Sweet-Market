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

    //�ܰ�
    Stack<GameObject> moneyStack = new Stack<GameObject>();
    //�� ������ġ
    [SerializeField] private Transform money_Posision;

    //���� ���� ��ġ
    [SerializeField] private Transform bag_Position;



    [SerializeField] bool player_InRange;
    [SerializeField] bool isCheckingProcess;

    [SerializeField] float after_bag_Spawn;
    [SerializeField] float next_Customer;
    [SerializeField] float timewaitChecking;

    WaitForSeconds waitChecking;
    WaitForSeconds wait_After_Bag_Spawn;
    WaitForSeconds wait_Next_Customer;




    //��� �ڷ�ƾ
    Coroutine check_Bill_Corutine;
    //�÷��̾�� �� �ű�� �ڷ�ƾ
    Coroutine collectingMoneyCoroutine;

    //Ʈ���� ����
    [Space(10)]
    [SerializeField] GameObject triggerSizeImage;
    Coroutine triggerSizeCoroutine;


    private void Start()
    {
        GM = GameManager.Instance;

        AddTime();
    }

    //ĳ��
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

        //�� �ϳ��� �ű��
        if (moneyZone != null && moneyZone.isPlayerDeteced && collectingMoneyCoroutine == null && moneyStack.Count > 0)
        {
            collectingMoneyCoroutine = StartCoroutine(Utils.PlayerTakeMoneyRoutine(() => moneyStack.Count > 0 && moneyZone.isPlayerDeteced, () => moneyStack.Pop(), money => { ObjectPool.Instance.ReturnObject(DataManager.Instance.money_Prefab, money); }, () => collectingMoneyCoroutine = null));
        }



    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Ʃ�丮��
            if (TutorialManager.Instance.currentStep == TutorialStep.ShowBasket)
            {
                TutorialManager.Instance.currentStep = TutorialStep.PosTable;
            }

            player_InRange = true;

            //���ü� on
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

            //���ü� off
            if (triggerSizeCoroutine != null)
                StopCoroutine(triggerSizeCoroutine);
            triggerSizeCoroutine = StartCoroutine(GM.ScaleTriggerImage(triggerSizeImage, Vector3.one));  // ���� ũ��(1��)�� 0.3�� ���� ���
        }
    }



    //��� �ڷ�ƾ
    IEnumerator CheckBillRoutine()
    {
        //���� �Ŵ������� �����ϴ� ����Ʈ���� ��������
        target_customer = GM.postableCustomerList[0];

        while (true)
        {
            yield return waitChecking; //��� �ð�
            //���� �����̸� �ǳʶٱ�
            if (target_customer.isMoving)
            {
                yield return null;
                continue;
            }
            else
            {
                GM.postableCustomerList.Remove(target_customer);

                //�������
                GameObject bag = ObjectPool.Instance.GetObject(DataManager.Instance.bag_Prefab, bag_Position.position, Quaternion.Euler(0, 0, 0));
                yield return wait_After_Bag_Spawn;

                //ũ�ξƻ� �����̱�, ������Ű��
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

                //���� �����̱�
                yield return StartCoroutine(Utils.MoveBezier(bag.transform, bag.transform.position, target_customer.croassantBox.position + Vector3.up * 4f, target_customer.croassantBox.position, 0.1f));
                Utils.GetTheBag(bag, target_customer.croassantBox, Quaternion.identity);

                //�� ����
                target_customer.isStack = true;
                target_customer.isBillFinish = true;
                isCheckingProcess = false;

                //��� ���� �� �߰�
                GM.finishCustomerCount++;

                //���� ��ġ ����
                target_customer.AddPosition(customer_Destination.turnPoint);
                target_customer.GoToHome();

                //���
                Utils.CalculateMoney(croassantCount, money_Posision, moneyStack);

                //�� ����� ����
                GM.SortCustomersByDistance(GM.postableCustomerList, GM.posPosition, orderDiction.up_left);


                //��� ������ ���� ��� ��ٸ���
                yield return wait_Next_Customer;
                check_Bill_Corutine = null;
                break;

            }
        }


    }


}
