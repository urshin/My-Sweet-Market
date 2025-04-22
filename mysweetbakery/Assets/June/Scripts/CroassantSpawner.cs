using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CroassantSpawner : MonoBehaviour
{
    GameManager GM;

    [Header("Croassant")]
    [SerializeField] private Transform spawnPosition;
    public Queue<GameObject> croassant_Queue = new Queue<GameObject>();
    [SerializeField] private int maxCroassant;
    [SerializeField] private float bakeTime = 2.0f;
    [SerializeField] private float spawnDelay = 0.3f;
    [SerializeField] private float takeOutDelay = 0.2f;
    [SerializeField] private float ejectionSpeed = 7;

    private bool IsFull => croassant_Queue.Count >= maxCroassant;



    // ĳ��
    WaitForSeconds bakeWait;
    WaitForSeconds delayWait;
    WaitForSeconds takeOutWait;

    //Ʈ���� ����
    [Space(10)]
    [SerializeField] GameObject triggerSizeImage;
    private bool playerInRange = false;
    private Player targetPlayer = null;

    //�ڷ�ƾ
    Coroutine triggerSizeCoroutine;
    Coroutine spawnCoroutine;
    Coroutine giveCoroutine;




    private void Awake()
    {
        bakeWait = new WaitForSeconds(bakeTime);
        delayWait = new WaitForSeconds(spawnDelay);
        takeOutWait = new WaitForSeconds(takeOutDelay);
    }

    private void Start()
    {
        //�ʱ�ȭ
        GM = GameManager.Instance;

        //�ʱ� �׽�Ʈ
        if (GM.isTesting)
        {
            for (int i = 0; i < maxCroassant; i++)
            {

                croassant_Queue.Enqueue(GM.testCroassant[i]);
            }
        }
        else if (!GM.isTesting)
        {
            for (int i = 0; i < GM.testCroassant.Count; i++)
            {
                Destroy(GM.testCroassant[i]);
            }
            GM.testCroassant.Clear();
        }


        StartSpawning();
    }


    private void StartSpawning()
    {
        if (spawnCoroutine == null && !IsFull)
        {
            spawnCoroutine = StartCoroutine(SpawnCroassantCoroutine());
        }
    }


    /// <summary>
    /// ũ�ξƻ� ���� �ڷ�ƾ
    /// �ִ� ������ ��ŭ �ݺ�
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnCroassantCoroutine()
    {
        while (!IsFull)
        {

            GameObject newCroassant = ObjectPool.Instance.GetObject(DataManager.Instance.croassant_Prefab, spawnPosition.position, spawnPosition.rotation);

            yield return delayWait;

            Rigidbody rb = newCroassant.GetComponent<Rigidbody>();
            newCroassant.GetComponent<Collider>().enabled = true;
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = spawnPosition.forward * -ejectionSpeed;
            }
            yield return delayWait;
            croassant_Queue.Enqueue(newCroassant);
            yield return bakeWait;
        }
        spawnCoroutine = null;
    }


    /// <summary>
    /// ũ�ξƻ� ���ÿ��� ������
    /// </summary>
    /// <returns></returns>
    public GameObject RemoveCroassant()
    {
        if (croassant_Queue.Count > 0)
        {
            GameObject removed = croassant_Queue.Dequeue();
            // ť ���°� �ٲ�� ������ �ٽ� ������ �� �ֵ���
            if (!IsFull)
            {
                StartSpawning();
            }
            return removed;
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Ʃ�丮��
            if (TutorialManager.Instance.currentStep == TutorialStep.None)
            {
                TutorialManager.Instance.currentStep = TutorialStep.CroissantSpawner;
                TutorialManager.Instance.UpdatePositionArrow();
            }

            //Ʈ����
            targetPlayer = other.GetComponent<Player>();
            playerInRange = true;


            //Ʈ���� ���ü� on
            if (triggerSizeCoroutine != null)
                StopCoroutine(triggerSizeCoroutine);
            triggerSizeCoroutine = StartCoroutine(GM.ScaleTriggerImage(triggerSizeImage, Vector3.one * 1.1f));

            //�ڷ�ƾ �ߺ� ����
            if (giveCoroutine == null)
            {
                giveCoroutine = StartCoroutine(GiveCroassant());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Ʈ���� ����
            playerInRange = false;
            targetPlayer = null;

            //Ʈ���� ���ü� off
            if (triggerSizeCoroutine != null)
                StopCoroutine(triggerSizeCoroutine);
            triggerSizeCoroutine = StartCoroutine(GM.ScaleTriggerImage(triggerSizeImage, Vector3.one));  // ���� ũ��(1��)�� 0.3�� ���� ���

            if (giveCoroutine != null)
            {
                StopCoroutine(giveCoroutine);
                giveCoroutine = null;
            }
        }
    }

    /// <summary>
    /// �÷��̾�� ũ�ξƻ� �ֱ�
    /// </summary>
    /// <returns></returns>
    private IEnumerator GiveCroassant()
    {
        while (playerInRange && targetPlayer != null && !targetPlayer.isBoxFull)
        {
            GameObject croassant = RemoveCroassant();
            if (croassant == null)
            {
                yield return takeOutWait;
                continue;
            }

            Rigidbody rb = croassant.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;
            }

            Collider col = croassant.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
            targetPlayer.InPutCroassant(croassant);
            SoundManager.instance.PlayAudio(audioSource.Get_Object);
            yield return takeOutWait;
        }
        giveCoroutine = null;
    }
}
