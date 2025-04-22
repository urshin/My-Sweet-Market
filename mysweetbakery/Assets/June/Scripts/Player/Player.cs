using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private static Player instance = null;


    //üĿ
    [Header("Checker")]
    [SerializeField] bool isStack = false;

    //playerMovement
    [Space(10)]
    [Header("player Movement")]
    [SerializeField] private JoyStick JoyStick;
    [SerializeField] private float player_Movement_Speed;
    private Quaternion last_Player_LookRotation;
    private CharacterController cc;

    //player_Animation
    [Space(10)]
    [Header("Animation")]
    [SerializeField] Animator animator;


    //Croassant info
    [Space(10)]
    [Header("Croassant")]
    public Stack<GameObject> player_Croassant_Stack = new Stack<GameObject>();
    public bool isBoxFull;
    [SerializeField] Transform croassantBox;
    private int maxCroassant = 8;

    //ui
    [Space(10)]
    [Header("UI")]
    [SerializeField] GameObject isMaxText;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public static Player Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }
    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        //��� �ִ���
        if (player_Croassant_Stack.Count > 0)
        {
            isStack = true;
        }
        else
        {
            isStack = false;
        }

        //IsMax �ؽ�Ʈ 
        if (isBoxFull)
        {
            isMaxText.SetActive(true);
            isMaxText.transform.localPosition = new Vector3(0, maxCroassant * 0.5f + 2, 0);
        }
        else
        {
            isMaxText.SetActive(false);

        }


        PlayerMovement();
    }
    private void LateUpdate()
    {
        PlayerAnimation();
    }

    //�ִϸ��̼�
    private void PlayerAnimation()
    {
        string targetState;

        if (cc.velocity.magnitude > 0)
        {
            targetState = isStack ? "Stack_Move" : "Default_Move";
        }
        else
        {
            targetState = isStack ? "Stack_Idle" : "Default_Idle";
        }

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (!currentState.IsName(targetState))
        {
            animator.Play(targetState);
        }

    }

    //������
    private void PlayerMovement()
    {
        //���̽�ƽ ���� �о����
        Vector3 movementVec3 = new Vector3(JoyStick.joySticVector.x, 0, JoyStick.joySticVector.y);

        //������ ����
        cc.Move(movementVec3 * player_Movement_Speed * Time.deltaTime);

        //ȸ�� ����
        if (movementVec3.sqrMagnitude > 0.1)
        {
            last_Player_LookRotation = Quaternion.LookRotation(movementVec3);
        }

        transform.rotation = last_Player_LookRotation;
    }

    //ũ�ξƻ� �߰� �ϱ�
    public void InPutCroassant(GameObject croassant)
    {
        if (player_Croassant_Stack.Count < maxCroassant)
        {
            StartCoroutine(InPutCroassantRoutine(croassant));
        }
    }

    private IEnumerator InPutCroassantRoutine(GameObject croassant)
    {
        int index = player_Croassant_Stack.Count;
        Vector3 localTargetPos = new Vector3(0, index * 0.5f, 0);  // 0.5 => ũ�οͻ� ����
        Vector3 worldTargetPos = croassantBox.TransformPoint(localTargetPos);
        Vector3 control = croassantBox.position + Vector3.up * 2 + new Vector3(0, index * 0.5f, 0);

        yield return StartCoroutine(Utils.MoveBezier(croassant.transform, croassant.transform.position, control, worldTargetPos, 0.1f));

        Utils.AddCroassant(player_Croassant_Stack, croassant, croassantBox, Quaternion.identity);

        //üũ
        if (player_Croassant_Stack.Count >= maxCroassant)
            isBoxFull = true;
    }

    //ũ�ξƻ� �ѱ��
    public GameObject OutPutCroassant()
    {
        isBoxFull = false;
        return player_Croassant_Stack.Pop();


    }

}
