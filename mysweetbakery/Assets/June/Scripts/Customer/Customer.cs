using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.U2D;
using UnityEngine.UI;

//��ǥ ��ġ
public enum customer_Destination
{
    showBasket,
    entrance,
    posTable,
    tableSeat,
    middlePoint,
    turnPoint,
    complain,
}

public class Customer : MonoBehaviour
{
    GameManager GM;

    [Header("WayPoints")]
    public Queue<Transform> wayPointQueue = new Queue<Transform>();
    /// <summary>
    /// ���� �� �� ���� üũ. 
    /// </summary>
    public Dictionary<Transform, System.Func<bool>> waitConditions = new Dictionary<Transform, System.Func<bool>>();
    public Transform currentTarget;

    //���� ��ġ ���
    [SerializeField] private Transform assignedPosition;

    //croassant
    public Stack<GameObject> currentCroassantStak = new Stack<GameObject>();
    public Transform croassantBox;
    public int croassantNeed;
    [SerializeField] int maxCroassant;

    [Space(10)]
    [Header("Chcker")]

    private bool isNew = true;

    public bool isStack;

    public bool isToGo;

    public bool isSeat = false;

    public bool isBillFinish;

    public bool isMoving;

    public bool isBoxFull;




    //UI
    [SerializeField] private TextMeshProUGUI croassantNumber;
    [SerializeField] GameObject countImageParent;
    [SerializeField] private Image currentImage;
    [SerializeField] private SpriteAtlas spriteAtlas;
    [SerializeField] private Image whatBread;
    [SerializeField] private ParticleSystem smileEffect;
    [SerializeField] private Image backGroundImage;

    //animator
    [SerializeField] private Animator animator;
    Rigidbody rb;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        GM = GameManager.Instance;
        isNew = false;
        InitializeCustomer();

    }

    private void OnEnable()
    {
        if (!isNew)
        {
            InitializeCustomer();
        }
    }

    //�ʱ�ȭ
    private void InitializeCustomer()
    {
        isBillFinish = false;
        isBoxFull = false;
        isStack = false;
        isSeat = false;
        isMoving = false;
        croassantNeed = Random.Range(1, maxCroassant + 1);


        if (GM.customercount <= 3)
        {

            isToGo = true;
        }
        else
        {
            isToGo = (Random.value >= 0.5f);
        }



        //ũ�ξƻ� UI����
        croassantNumber.text = croassantNeed.ToString();
        countImageParent.SetActive(true);
        backGroundImage.gameObject.SetActive(true);
        //�̹��� �ٲٱ�
        if (isToGo)
        {
            currentImage.sprite = spriteAtlas.GetSprite("Pay");
        }
        else if (!isToGo)
        {
            currentImage.sprite = spriteAtlas.GetSprite("TableChair");
        }

        currentImage.gameObject.SetActive(false);


        //��� ����
        waitConditions.Clear();

        //���� üũ. �̹� ������ ���� �����ϰ� ����
        if (assignedPosition != null && GM.showBasket_Dic.ContainsKey(assignedPosition))
        {
            GM.showBasket_Dic[assignedPosition] = false;
            assignedPosition = null;
        }

        AddPosition(customer_Destination.middlePoint);
        AddPosition(customer_Destination.showBasket);



    }
    /// <summary>
    /// ���� ������ ����
    /// </summary>
    /// <param name="trans"></param>
    public void AddFreePosition(Transform trans)
    {
        wayPointQueue.Enqueue(trans);
        waitConditions[trans] = () => true;  // ��� ���
    }
    /// <summary>
    /// �̸� �����ص� ���������� �̵��ϱ�
    /// </summary>
    /// <param name="destination"></param>
    public void AddPosition(customer_Destination destination)
    {
        switch (destination)
        {
            case customer_Destination.entrance:
                wayPointQueue.Enqueue(GM.entrancePosition);
                waitConditions[GM.entrancePosition] = () => true;  // ��� ���
                break;

            case customer_Destination.middlePoint:
                wayPointQueue.Enqueue(GM.middlePosition);
                waitConditions[GM.middlePosition] = () => true;  // ��� ���
                break;

            case customer_Destination.turnPoint:
                GameObject tempPoint = ObjectPool.Instance.GetObject(DataManager.Instance.wayPointPrefab, this.transform.position + Vector3.right * 2, Quaternion.identity);
                wayPointQueue.Enqueue(tempPoint.transform);
                waitConditions[tempPoint.transform] = () => true;  // ��� ���
                ObjectPool.Instance.ReturnObject(DataManager.Instance.wayPointPrefab, tempPoint);
                break;

            case customer_Destination.complain:
                wayPointQueue.Enqueue(GM.complainPosition);
                waitConditions[GM.complainPosition] = () => true;  // ��� ���
                break;

            case customer_Destination.showBasket:
                foreach (var position in GM.showBasket_Dic)
                {
                    if (!position.Value)
                    {
                        wayPointQueue.Enqueue(position.Key);
                        waitConditions[position.Key] = () => true;  // isBoxFull == true�� �� ������ ���
                        GM.showBasket_Dic[position.Key] = true;
                        assignedPosition = position.Key;   // �ڸ� ����

                        break;
                    }
                }
                break;



        }


    }



    void Update()
    {
        WayProcess();

        CustomerAnimation();
    }

    //������ ����
    public void WayProcess()
    {
        if (currentTarget == null && wayPointQueue.Count > 0)
        {
            currentTarget = wayPointQueue.Dequeue();
        }

        if (currentTarget != null)
        {
            Vector3 targetPosition = new Vector3(currentTarget.position.x, 0, currentTarget.position.z);
            Vector3 direction = targetPosition - transform.position;
            isMoving = true;
            //ȸ���� ����
            if (direction.sqrMagnitude > 0.001f)
            {
                direction.y = 0;
                transform.rotation = Quaternion.LookRotation(direction);
            }

            //�����̱�
            rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, 5 * Time.fixedDeltaTime));

            // ��ǥ ���� ���� ó��
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.rotation = currentTarget.localRotation;

                // �������� �� ���� Ȯ��
                if (waitConditions.ContainsKey(currentTarget))
                {
                    //������ ������ ���� ó��
                    if (wayPointQueue.Count == 0)
                    {
                        isMoving = false;
                    }
                    if (!waitConditions[currentTarget].Invoke())
                    {
                        // ���� ���� ������ ���
                        return;
                    }

                }
                currentTarget = null;
            }
        }
    }

    //�ִϸ��̼� ó��
    private void CustomerAnimation()
    {
        string targetState;

        if (isMoving)
        {
            targetState = isStack ? "Stack_Walk" : "Default_Walk";
        }
        else
        {
            targetState = isStack ? "Stack_Idle" : "Default_Idle";
        }
        if (isSeat)
        {
            targetState = "Sitting_Talking";
        }
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (!currentState.IsName(targetState))
        {
            animator.Play(targetState);
        }
    }


    //ũ�ξƻ� ��������
    public void InPutCroassant(GameObject croassant)
    {
        if (currentCroassantStak.Count < croassantNeed)
        {
            MoveAndAdd(croassant);
        }
    }

    private void MoveAndAdd(GameObject croassant)
    {
        Utils.AddCroassant(currentCroassantStak, croassant, croassantBox, Quaternion.Euler(0, 90, 0));

        //�Ѱ� �̻� ��� ������ üũ
        isStack = (currentCroassantStak.Count > 0);

        croassantNumber.text = (croassantNeed - currentCroassantStak.Count).ToString();

        //�� ����� ��
        if (currentCroassantStak.Count == croassantNeed)
        {
            if (assignedPosition != null && GM.showBasket_Dic.ContainsKey(assignedPosition))
            {
                GM.showBasket_Dic[assignedPosition] = false;
                assignedPosition = null;
            }
            isBoxFull = true;
            countImageParent.SetActive(false);
            currentImage.gameObject.SetActive(true);

        }

    }


    public IEnumerator WaitingSeat()
    {
        // UnlockSeat�� ���� ������ ��ٸ�
        while (!GM.isUnlockSeat)
        {
            if (!isMoving && !GM.isUnlockSeat && !TutorialManager.Instance.isUnlockSeatShow)
            {
                TutorialManager.Instance.isUnlockSeatShow = true;
                GM.CM.Show(CamEnum.table);
            }


            yield return null;
        }

        // �ڸ��� ������ �� ȣ��
        GM.AddCustomeTable(this);
    }


    /// <summary>
    /// ���Ա��� ����
    /// </summary>
    public void GoToHome()
    {
        if (isBillFinish)
        {
            GM.currentCustomerCount--;
            backGroundImage.gameObject.SetActive(false);
            smileEffect.Play();
            AddPosition(customer_Destination.entrance);
        }
    }


}