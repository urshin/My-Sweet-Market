using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance = null;



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
    public static DataManager Instance
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


    [Space(10)]
    [Header("GameObjects")]
    public GameObject croassant_Prefab;
    public GameObject customer_Prefab;
    public GameObject money_Prefab;
    public GameObject bag_Prefab;
    public GameObject wayPointPrefab;







}
