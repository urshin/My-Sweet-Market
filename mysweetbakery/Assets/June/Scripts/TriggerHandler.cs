using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerHandler : MonoBehaviour
{
    GameManager GM;

    [Header("Lock")]
    [SerializeField] GameObject[] before;
    [SerializeField] GameObject[] after;

    [Space(10)]
    public bool isON;
    [SerializeField] int currentMoney;
    [SerializeField] bool isPlayerDetected;
    [SerializeField] bool isColllectAll; //돈을 조금씩 가져갈지 한번에 가져갈지
    Coroutine collectMoney;
    //ui
    [Space(10)]
    [Header("UI")]
    [SerializeField] Text moneyText;

    private void Start()
    {
        GM = GameManager.Instance;
        isON = false;
        moneyText.text = currentMoney.ToString();


        foreach (GameObject b in before)
        {

            b.SetActive(true);
        }
        foreach (GameObject a in after)
        {

            a.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && this.enabled)
        {

            isPlayerDetected = true;
            if (collectMoney == null)
            {
                if (isColllectAll)
                {
                    collectMoney = StartCoroutine(collecMoneyAllRoutine());
                }
                else
                {

                    collectMoney = StartCoroutine(collecMoneyRoutine());
                }
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            isPlayerDetected = false;


        }
    }

    //돈 한번에 가져가기
    IEnumerator collecMoneyAllRoutine()
    {

        if (GM.playerMoneyCount >= currentMoney)
        {
            int count = currentMoney;
            for (int i = 0; i < count; i++)
            {
                GameObject money = ObjectPool.Instance.GetObject(DataManager.Instance.money_Prefab, Player.Instance.transform.position, Quaternion.identity);
                yield return (Utils.MoveBezier(money.transform, Player.Instance.transform.position, transform.position + Vector3.up * 4f, transform.position, 0.02f));

                //돈 계산
                currentMoney--;
                GM.playerMoneyCount--;
                moneyText.text = currentMoney.ToString();
                ObjectPool.Instance.ReturnObject(DataManager.Instance.money_Prefab, money);

                //잠금 해제
                if (currentMoney == 0)
                {
                    isON = true;
                    //GM.UnlockSeat = true;
                    foreach (GameObject b in before)
                    {

                        b.SetActive(false);
                    }
                    foreach (GameObject a in after)
                    {

                        a.SetActive(true);
                    }
                    break;
                }
            }
        }
        collectMoney = null;

        yield return null;
    }

    //돈 하나씩 가져가기.
    IEnumerator collecMoneyRoutine()
    {
        //잔여물 
        GameObject activeMoneys = new GameObject();

        while (isPlayerDetected && GM.playerMoneyCount > 0)
        {
            GameObject money = ObjectPool.Instance.GetObject(DataManager.Instance.money_Prefab, Player.Instance.transform.position, Quaternion.identity);

            activeMoneys = money;

            yield return (Utils.MoveBezier(money.transform, Player.Instance.transform.position, transform.position + Vector3.up * 4f, transform.position, 0.05f));
            money.SetActive(false);
            ObjectPool.Instance.ReturnObject(DataManager.Instance.money_Prefab, money);


            currentMoney--;
            GM.playerMoneyCount--;
            moneyText.text = currentMoney.ToString();
            if (currentMoney == 0)
            {
                isON = true;
                foreach (GameObject b in before)
                {

                    b.SetActive(false);
                }
                foreach (GameObject a in after)
                {

                    a.SetActive(true);
                }
            }
        }
        //남은 돈 반환 처리
        if (activeMoneys != null)
        {
            activeMoneys.SetActive(false);

            ObjectPool.Instance.ReturnObject(DataManager.Instance.money_Prefab, activeMoneys);

        }

        collectMoney = null;
    }
}
