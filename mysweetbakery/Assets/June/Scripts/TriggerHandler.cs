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
    [SerializeField] bool isColllectAll; //���� ���ݾ� �������� �ѹ��� ��������
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

    //�� �ѹ��� ��������
    IEnumerator collecMoneyAllRoutine()
    {

        if (GM.playerMoneyCount >= currentMoney)
        {
            int count = currentMoney;
            for (int i = 0; i < count; i++)
            {
                GameObject money = ObjectPool.Instance.GetObject(DataManager.Instance.money_Prefab, Player.Instance.transform.position, Quaternion.identity);
                yield return (Utils.MoveBezier(money.transform, Player.Instance.transform.position, transform.position + Vector3.up * 4f, transform.position, 0.02f));

                //�� ���
                currentMoney--;
                GM.playerMoneyCount--;
                moneyText.text = currentMoney.ToString();
                ObjectPool.Instance.ReturnObject(DataManager.Instance.money_Prefab, money);

                //��� ����
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

    //�� �ϳ��� ��������.
    IEnumerator collecMoneyRoutine()
    {
        //�ܿ��� 
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
        //���� �� ��ȯ ó��
        if (activeMoneys != null)
        {
            activeMoneys.SetActive(false);

            ObjectPool.Instance.ReturnObject(DataManager.Instance.money_Prefab, activeMoneys);

        }

        collectMoney = null;
    }
}
