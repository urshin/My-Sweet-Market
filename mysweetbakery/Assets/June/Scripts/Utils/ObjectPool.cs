using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    //������Ʈ Ǯ ��ųʸ�
    private readonly Dictionary<GameObject, Queue<GameObject>> pool_Dic = new Dictionary<GameObject, Queue<GameObject>>();
    private Transform poolRoot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            poolRoot = new GameObject("PooledObjects").transform;
            poolRoot.SetParent(this.transform);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //������Ʈ ���
    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        //üũ
        if (!pool_Dic.ContainsKey(prefab))
        {
            pool_Dic[prefab] = new Queue<GameObject>();
        }

        Queue<GameObject> pool = pool_Dic[prefab];
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.transform.position = position;  // ��ġ �缳��
            obj.transform.rotation = rotation;  // ȸ�� �缳��
        }
        else
        {
            obj = Instantiate(prefab, position, rotation, poolRoot);
        }

        obj.SetActive(true);
        return obj;
    }


    //��ȯ 
    public void ReturnObject(GameObject prefab, GameObject obj)
    {
        if (!pool_Dic.ContainsKey(prefab))
        {
            pool_Dic[prefab] = new Queue<GameObject>();
        }

        // �ߺ� ��ȯ ����
        if (!pool_Dic[prefab].Contains(obj))
        {
            obj.SetActive(false);
            obj.transform.SetParent(poolRoot);  // ��ȯ �� ��ġ �ʱ�ȭ
            pool_Dic[prefab].Enqueue(obj);
        }
    }
}

