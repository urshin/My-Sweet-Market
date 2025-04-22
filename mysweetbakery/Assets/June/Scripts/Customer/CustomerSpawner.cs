using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomerSpawner : MonoBehaviour
{

    GameManager GM;
    [SerializeField] Customer customer;


    [SerializeField] private Transform spawn_Position;
    [SerializeField] private float spawn_Duration;

    private Coroutine spawnCoroutine;
    private WaitForSeconds delay_Wait;



    void Start()
    {
        GM = GameManager.Instance;
        delay_Wait = new WaitForSeconds(spawn_Duration);
        spawnCoroutine = StartCoroutine(SpawnCustomerRoutine());

    }


    //�� ��ȯ �ڷ�ƾ
    private IEnumerator SpawnCustomerRoutine()
    {
        while (true)
        {
            if (GM.CanSpawnCustomer())
            {
                ObjectPool.Instance.GetObject(DataManager.Instance.customer_Prefab, spawn_Position.position, Quaternion.identity).GetComponent<Customer>();

                //�� �� ī����
                GM.currentCustomerCount++;
                GM.customercount++;
            }
            yield return delay_Wait;
        }
    }

    //�� �ʱ�ȭ
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Customer"))
        {
            customer = other.GetComponent<Customer>();


            if (customer.isBillFinish && customer.isToGo)
            {
                //�����ִ� ���� �����
                for (int i = customer.croassantBox.childCount - 1; i >= 0; i--)
                {
                    GameObject child = customer.croassantBox.GetChild(i).gameObject;
                    if (child.tag == "Apple")
                    {

                        ObjectPool.Instance.ReturnObject(DataManager.Instance.croassant_Prefab, child);
                    }
                    else if (child.tag == "Bag")
                    {
                        ObjectPool.Instance.ReturnObject(DataManager.Instance.bag_Prefab, child);

                    }
                }

                ObjectPool.Instance.ReturnObject(DataManager.Instance.customer_Prefab, customer.gameObject);

            }
            else if (customer.isBillFinish && !customer.isToGo)
            {
                ObjectPool.Instance.ReturnObject(DataManager.Instance.customer_Prefab, customer.gameObject);

            }
        }
    }
}