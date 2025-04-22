using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    //오브젝트 풀 딕셔너리
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

    //오브젝트 사용
    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        //체크
        if (!pool_Dic.ContainsKey(prefab))
        {
            pool_Dic[prefab] = new Queue<GameObject>();
        }

        Queue<GameObject> pool = pool_Dic[prefab];
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.transform.position = position;  // 위치 재설정
            obj.transform.rotation = rotation;  // 회전 재설정
        }
        else
        {
            obj = Instantiate(prefab, position, rotation, poolRoot);
        }

        obj.SetActive(true);
        return obj;
    }


    //반환 
    public void ReturnObject(GameObject prefab, GameObject obj)
    {
        if (!pool_Dic.ContainsKey(prefab))
        {
            pool_Dic[prefab] = new Queue<GameObject>();
        }

        // 중복 반환 방지
        if (!pool_Dic[prefab].Contains(obj))
        {
            obj.SetActive(false);
            obj.transform.SetParent(poolRoot);  // 반환 시 위치 초기화
            pool_Dic[prefab].Enqueue(obj);
        }
    }
}

