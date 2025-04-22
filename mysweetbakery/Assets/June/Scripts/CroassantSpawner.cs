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



    // 캐싱
    WaitForSeconds bakeWait;
    WaitForSeconds delayWait;
    WaitForSeconds takeOutWait;

    //트리거 관련
    [Space(10)]
    [SerializeField] GameObject triggerSizeImage;
    private bool playerInRange = false;
    private Player targetPlayer = null;

    //코루틴
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
        //초기화
        GM = GameManager.Instance;

        //초기 테스트
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
    /// 크로아상 생성 코루틴
    /// 최대 생성수 만큼 반복
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
    /// 크로아상 스택에서 꺼내기
    /// </summary>
    /// <returns></returns>
    public GameObject RemoveCroassant()
    {
        if (croassant_Queue.Count > 0)
        {
            GameObject removed = croassant_Queue.Dequeue();
            // 큐 상태가 바뀌면 스폰을 다시 시작할 수 있도록
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
            //튜토리얼
            if (TutorialManager.Instance.currentStep == TutorialStep.None)
            {
                TutorialManager.Instance.currentStep = TutorialStep.CroissantSpawner;
                TutorialManager.Instance.UpdatePositionArrow();
            }

            //트리거
            targetPlayer = other.GetComponent<Player>();
            playerInRange = true;


            //트리거 가시성 on
            if (triggerSizeCoroutine != null)
                StopCoroutine(triggerSizeCoroutine);
            triggerSizeCoroutine = StartCoroutine(GM.ScaleTriggerImage(triggerSizeImage, Vector3.one * 1.1f));

            //코루틴 중복 방지
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
            //트리거 관련
            playerInRange = false;
            targetPlayer = null;

            //트리거 가시성 off
            if (triggerSizeCoroutine != null)
                StopCoroutine(triggerSizeCoroutine);
            triggerSizeCoroutine = StartCoroutine(GM.ScaleTriggerImage(triggerSizeImage, Vector3.one));  // 원래 크기(1배)로 0.3초 동안 축소

            if (giveCoroutine != null)
            {
                StopCoroutine(giveCoroutine);
                giveCoroutine = null;
            }
        }
    }

    /// <summary>
    /// 플레이어에게 크로아상 주기
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
