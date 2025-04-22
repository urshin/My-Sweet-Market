using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShowBasket : MonoBehaviour
{

    GameManager GM;
    Player targetPlayer;

    [Header("Customer")]
    [Tooltip("고객의 서야 할 위치")]
    [SerializeField] Transform[] positions;
    Queue<Customer> customer_Queue = new Queue<Customer>();
    [SerializeField] float takeOutDelay = 0.2f;

    [Header("Croassant")]
    public Stack<GameObject> croassant_Stack = new Stack<GameObject>();
    [SerializeField] Transform croassant_position;
    [SerializeField] int maxCroasant;
    [SerializeField] float collectDelay = 0.5f;  // 크로아상 줍기 간격


    WaitForSeconds takeOutWait;

    [Space(10)]
    //트리거
    [SerializeField] GameObject triggerSizeImage;
    [SerializeField] bool playerInRange = false;

    //코루틴
    Coroutine giveCoroutine;
    Coroutine collectCoroutine;
    Coroutine triggerSizeCoroutine;

    private void Awake()
    {
        takeOutWait = new WaitForSeconds(takeOutDelay);
    }

    private void Start()
    {
        GM = GameManager.Instance;

        foreach (Transform t in positions)
        {
            GM.showBasket_Dic.Add(t, false);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            if (TutorialManager.Instance.currentStep == TutorialStep.CroissantSpawner)
            {
                TutorialManager.Instance.currentStep = TutorialStep.ShowBasket;
                TutorialManager.Instance.UpdatePositionArrow();
            }


            targetPlayer = other.GetComponent<Player>();
            playerInRange = true;

            if (triggerSizeCoroutine != null)
                StopCoroutine(triggerSizeCoroutine);
            triggerSizeCoroutine = StartCoroutine(GM.ScaleTriggerImage(triggerSizeImage, Vector3.one * 1.1f));

            if (collectCoroutine == null)
            {
                collectCoroutine = StartCoroutine(CollectCroassantRoutine());
            }
        }
        if (other.CompareTag("Customer"))
        {
            Customer customer = other.GetComponent<Customer>();

            customer_Queue.Enqueue(customer);

            //if (giveCoroutine == null)
            //{
            //    giveCoroutine = StartCoroutine(GiveCroassantRoutine());
            //}

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            targetPlayer = null;

            if (triggerSizeCoroutine != null)
                StopCoroutine(triggerSizeCoroutine);
            triggerSizeCoroutine = StartCoroutine(GM.ScaleTriggerImage(triggerSizeImage, Vector3.one));  // 원래 크기(1배)로 0.3초 동안 축소



        }
        if (other.CompareTag("Customer"))
        {
            Customer customer = other.GetComponent<Customer>();

        }
    }


    private void Update()
    {
        if (customer_Queue.Count > 0 && croassant_Stack.Count > 0)

        {
            if (giveCoroutine == null)
            {
                giveCoroutine = StartCoroutine(GiveCroassantRoutine());

            }
        }

    }
    private IEnumerator CollectCroassantRoutine()
    {
        while (playerInRange && targetPlayer != null)
        {
            if (croassant_Stack.Count < maxCroasant && Player.Instance.player_Croassant_Stack.Count > 0)
            {
                GameObject croassant = Player.Instance.OutPutCroassant();
                int index = croassant_Stack.Count; // Push 전 인덱스
                croassant_Stack.Push(croassant);
                if (croassant != null)
                {
                    float gridy = 0.5f;  // 세로 간격
                    float gridx = 0.7f;    // 가로 간격


                    int row = index / 2;
                    int col = index % 2;

                    // 최종 목적 위치 계산
                    Vector3 localTargetPos = new Vector3(row * gridy, 0, col * gridx);
                    Vector3 worldTargetPos = croassant_position.TransformPoint(localTargetPos);

                    Vector3 start = croassant.transform.position;
                    Vector3 control = croassant_position.position + Vector3.up * 4f;

                    // Bezier 이동
                    yield return StartCoroutine(Utils.MoveBezier(croassant.transform, start, control, worldTargetPos, 0.1f));
                    SoundManager.instance.PlayAudio(audioSource.Put_Object);

                    // 위치 보정
                    croassant.transform.SetParent(croassant_position, true);
                    croassant.transform.localPosition = localTargetPos;
                    croassant.transform.localRotation = Quaternion.Euler(0, 45f, 0);


                }
            }

            yield return new WaitForSeconds(collectDelay);
        }

        collectCoroutine = null;
    }


    public GameObject RemoveCroassant()
    {
        if (croassant_Stack.Count > 0)
        {
            GameObject removed = croassant_Stack.Pop();
            return removed;
        }
        return null;
    }
    private IEnumerator GiveCroassantRoutine()
    {
        Customer customer = customer_Queue.Peek();

        if (playerInRange || customer_Queue.Peek().isMoving)
        {
            yield break;
        }


        if (customer != null && !customer.isBoxFull)
        {
            GameObject croassant = RemoveCroassant();
            if (croassant != null)
            {
                yield return StartCoroutine(Utils.MoveBezier(croassant.transform, croassant.transform.position, customer.croassantBox.position + Vector3.up * 4, customer.croassantBox.position, 0.1f));
                customer.InPutCroassant(croassant);
                SoundManager.instance.PlayAudio(audioSource.Get_Object);

            }

        }

        if (customer != null && customer.isBoxFull)
        {

            if (customer.isToGo)
            {
                GM.AddCustomerPos(customer);
            }
            else if (!customer.isToGo)
            {
                if (GM.isUnlockSeat)
                {
                    GM.AddCustomeTable(customer);
                }
                else if (!GM.isUnlockSeat)
                {
                    GM.AddCustomerComplain(customer);
                    customer.isMoving = true;
                    StartCoroutine(customer.WaitingSeat());
                }
            }

            customer_Queue.Dequeue();

        }



        yield return takeOutWait;  // 0.2초 대기

        giveCoroutine = null;
    }

}
