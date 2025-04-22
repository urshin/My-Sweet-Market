using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.U2D;
using UnityEngine.UI;

//목표 위치
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
    /// 다음 갈 곳 조건 체크. 
    /// </summary>
    public Dictionary<Transform, System.Func<bool>> waitConditions = new Dictionary<Transform, System.Func<bool>>();
    public Transform currentTarget;

    //본인 위치 기억
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

    //초기화
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



        //크로아상 UI관련
        croassantNumber.text = croassantNeed.ToString();
        countImageParent.SetActive(true);
        backGroundImage.gameObject.SetActive(true);
        //이미지 바꾸기
        if (isToGo)
        {
            currentImage.sprite = spriteAtlas.GetSprite("Pay");
        }
        else if (!isToGo)
        {
            currentImage.sprite = spriteAtlas.GetSprite("TableChair");
        }

        currentImage.gameObject.SetActive(false);


        //경로 지정
        waitConditions.Clear();

        //조건 체크. 이미 점유한 곳을 제외하고 가기
        if (assignedPosition != null && GM.showBasket_Dic.ContainsKey(assignedPosition))
        {
            GM.showBasket_Dic[assignedPosition] = false;
            assignedPosition = null;
        }

        AddPosition(customer_Destination.middlePoint);
        AddPosition(customer_Destination.showBasket);



    }
    /// <summary>
    /// 자유 포지션 지정
    /// </summary>
    /// <param name="trans"></param>
    public void AddFreePosition(Transform trans)
    {
        wayPointQueue.Enqueue(trans);
        waitConditions[trans] = () => true;  // 즉시 통과
    }
    /// <summary>
    /// 미리 지정해둔 포지션으로 이동하기
    /// </summary>
    /// <param name="destination"></param>
    public void AddPosition(customer_Destination destination)
    {
        switch (destination)
        {
            case customer_Destination.entrance:
                wayPointQueue.Enqueue(GM.entrancePosition);
                waitConditions[GM.entrancePosition] = () => true;  // 즉시 통과
                break;

            case customer_Destination.middlePoint:
                wayPointQueue.Enqueue(GM.middlePosition);
                waitConditions[GM.middlePosition] = () => true;  // 즉시 통과
                break;

            case customer_Destination.turnPoint:
                GameObject tempPoint = ObjectPool.Instance.GetObject(DataManager.Instance.wayPointPrefab, this.transform.position + Vector3.right * 2, Quaternion.identity);
                wayPointQueue.Enqueue(tempPoint.transform);
                waitConditions[tempPoint.transform] = () => true;  // 즉시 통과
                ObjectPool.Instance.ReturnObject(DataManager.Instance.wayPointPrefab, tempPoint);
                break;

            case customer_Destination.complain:
                wayPointQueue.Enqueue(GM.complainPosition);
                waitConditions[GM.complainPosition] = () => true;  // 즉시 통과
                break;

            case customer_Destination.showBasket:
                foreach (var position in GM.showBasket_Dic)
                {
                    if (!position.Value)
                    {
                        wayPointQueue.Enqueue(position.Key);
                        waitConditions[position.Key] = () => true;  // isBoxFull == true가 될 때까지 대기
                        GM.showBasket_Dic[position.Key] = true;
                        assignedPosition = position.Key;   // 자리 저장

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

    //움직임 관련
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
            //회전값 설정
            if (direction.sqrMagnitude > 0.001f)
            {
                direction.y = 0;
                transform.rotation = Quaternion.LookRotation(direction);
            }

            //움직이기
            rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, 5 * Time.fixedDeltaTime));

            // 목표 지점 도착 처리
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.rotation = currentTarget.localRotation;

                // 도착했을 때 조건 확인
                if (waitConditions.ContainsKey(currentTarget))
                {
                    //갈곳이 없으면 멈춤 처리
                    if (wayPointQueue.Count == 0)
                    {
                        isMoving = false;
                    }
                    if (!waitConditions[currentTarget].Invoke())
                    {
                        // 조건 충족 전까지 대기
                        return;
                    }

                }
                currentTarget = null;
            }
        }
    }

    //애니메이션 처리
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


    //크로아상 가져오기
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

        //한개 이상 들고 있으면 체크
        isStack = (currentCroassantStak.Count > 0);

        croassantNumber.text = (croassantNeed - currentCroassantStak.Count).ToString();

        //다 담았을 때
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
        // UnlockSeat가 열릴 때까지 기다림
        while (!GM.isUnlockSeat)
        {
            if (!isMoving && !GM.isUnlockSeat && !TutorialManager.Instance.isUnlockSeatShow)
            {
                TutorialManager.Instance.isUnlockSeatShow = true;
                GM.CM.Show(CamEnum.table);
            }


            yield return null;
        }

        // 자리가 열렸을 때 호출
        GM.AddCustomeTable(this);
    }


    /// <summary>
    /// 출입구로 가기
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