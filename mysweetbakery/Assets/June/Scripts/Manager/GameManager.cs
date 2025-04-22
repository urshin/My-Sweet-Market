using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

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
    public static GameManager Instance
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

    public CameraManager CM;
    [SerializeField] PosTable postable;

    [Space(10)]
    [Header("Customer")]
    public int maxCustomer;
    public float finishCustomerCount;
    public int currentCustomerCount = 0;


    [Space(10)]
    [Header("Position")]
    public Dictionary<Transform, bool> showBasket_Dic = new Dictionary<Transform, bool>();
    public Transform posPosition;
    public Transform entrancePosition;
    public Transform seatPosition;
    public Transform middlePosition;
    public Transform complainPosition;

    [Space(10)]
    [Header("Customer List")]
    //포장
    public List<Customer> postableCustomerList = new List<Customer>();
    //먹고 가는 사람
    public List<Customer> tableCustomerList = new List<Customer>();
    //민원처리
    public List<Customer> complainCustomerList = new List<Customer>();


    List<GameObject> wastedGameObject = new List<GameObject>();


    [Space(10)]
    [Header("Player")]
    public float playerMoneyCount;


    //UI관련
    [SerializeField] Text MoneyCountText;


    [Space(10)]
    [Header("Checker")]
    public bool isUnlockSeat;

    [Space(10)]
    [Header("animation")]
    public float popUpDuration = 0.9f;
    //popup curve
    public AnimationCurve scaleXCurve;
    public AnimationCurve scaleYCurve;

    [Header("TEST")]
    public bool isTesting;
    public int customercount;
    public List<GameObject> testCroassant;

    private void LateUpdate()
    {

        MoneyCountText.text = playerMoneyCount.ToString();
    }


    /// <summary>
    /// 포스로 가는 손님 추가하기
    /// </summary>
    /// <param name="newCustomer"></param>
    public void AddCustomerPos(Customer newCustomer)
    {
        if (newCustomer != null)
        {
            postableCustomerList.Add(newCustomer);
            newCustomer.AddPosition(customer_Destination.middlePoint);
            SortCustomersByDistance(postableCustomerList, posPosition, orderDiction.up_left);
        }
    }

    /// <summary>
    /// 테이블로 가는 손님 추가하기
    /// </summary>
    /// <param name="newCustomer"></param>
    public void AddCustomeTable(Customer newCustomer)
    {
        if (newCustomer != null)
        {
            //  Debug.Log("테이블 업데이트");
            tableCustomerList.Add(newCustomer);
            SortCustomersByDistance(tableCustomerList, seatPosition, orderDiction.down_left);
        }
    }

    /// <summary>
    /// 불만이나 Unlock상태일때 손님 추가하기
    /// </summary>
    /// <param name="newCustomer"></param>
    public void AddCustomerComplain(Customer newCustomer)
    {
        if (newCustomer != null)
        {
            complainCustomerList.Add(newCustomer);
            newCustomer.AddPosition(customer_Destination.middlePoint);
            UpDateOrder(complainCustomerList, complainPosition, orderDiction.up_right);
        }
    }

    /// <summary>
    /// 리스트에 있는 손님 정렬하기
    /// </summary>
    /// <param name="sortList"></param>
    /// <param name="trans"></param>
    /// <param name="orderDirection"></param>
    public void SortCustomersByDistance(List<Customer> sortList, Transform trans, orderDiction orderDirection)
    {
        sortList.Sort((a, b) =>
        {
            float distanceA = Vector3.Distance(a.transform.position, trans.position);
            float distanceB = Vector3.Distance(b.transform.position, trans.position);
            return distanceA.CompareTo(distanceB);
        });

        UpDateOrder(sortList, trans, orderDirection);
    }

    /// <summary>
    /// 정렬된 손님들 줄 세우기
    /// </summary>
    /// <param name="sortList"></param>
    /// <param name="trans"></param>
    /// <param name="orderDirection"></param>
    public void UpDateOrder(List<Customer> sortList, Transform trans, orderDiction orderDirection)
    {
        //wastedGameObject.Clear();
        foreach (var a in wastedGameObject)
        {
            ObjectPool.Instance.ReturnObject(DataManager.Instance.wayPointPrefab, a);

        }
        foreach (Customer customer in sortList)
        {
            GameObject tempObject = UpdateCustomerPositions(customer, sortList, trans, orderDirection);
            wastedGameObject.Add(tempObject);

            customer.AddFreePosition(tempObject.transform);
        }

    }


    /// <summary>
    /// ~~쪽 으로 서고 ~~쪽으로 서기
    /// </summary>
    /// <param name="customer"></param>
    /// <param name="sortList"></param>
    /// <param name="trans"></param>
    /// <param name="orderDirection"></param>
    /// <returns></returns>
    public GameObject UpdateCustomerPositions(Customer customer, List<Customer> sortList, Transform trans, orderDiction orderDirection)
    {
        int index = sortList.IndexOf(customer);
        if (index == -1) return trans.gameObject;  // 리스트에 없으면 자기 자신 반환

        int rowLength = 5;  // 한 줄에 5명
        int row = index / rowLength;
        int col = index % rowLength;

        // 지그재그 배치
        if (row % 2 == 1)
        {
            col = rowLength - 1 - col;
        }
        Vector3 offset = new Vector3();
        Quaternion rotation = Quaternion.identity;
        switch (orderDirection)
        {
            case orderDiction.left_up:
                offset = new Vector3(col * 1f, 0, row * 1f);
                rotation = Quaternion.Euler(0, 90, 0);
                break;

            case orderDiction.right_up:
                offset = new Vector3(col * 1f, 0, row * 1f);
                rotation = Quaternion.Euler(0, 90, 0);
                break;

            case orderDiction.left_down:
                offset = new Vector3(-col * 1f, 0, -row * 1f);
                rotation = Quaternion.Euler(0, -90, 0);
                break;
            case orderDiction.right_down:
                offset = new Vector3(col * 1f, 0, -row * 1f);
                rotation = Quaternion.Euler(0, -90, 0);
                break;
            case orderDiction.up_left:
                offset = new Vector3(-row * 1f, 0, col * 1f);
                rotation = Quaternion.Euler(0, 180, 0);
                break;

            case orderDiction.up_right:
                offset = new Vector3(row * 1f, 0, col * 1f);
                rotation = Quaternion.Euler(0, 180, 0);
                break;

            case orderDiction.down_left:
                offset = new Vector3(-row * 1f, 0, -col * 1f);
                rotation = Quaternion.Euler(0, 0, 0);
                break;

            case orderDiction.down_right:
                offset = new Vector3(row * 1f, 0, -col * 1f);
                rotation = Quaternion.Euler(0, 0, 0);
                break;

        }

        // 새로운 GameObject 생성 및 위치 설정
        GameObject tempPoint = ObjectPool.Instance.GetObject(DataManager.Instance.wayPointPrefab, transform.position, rotation);
        tempPoint.transform.position = trans.position + offset;

        return tempPoint;
    }


    //고객 소환 체크
    public bool CanSpawnCustomer()
    {
        if (currentCustomerCount < maxCustomer)
        {
            return true;
        }
        return false;
    }

    //이미지 스케일 조정
    public IEnumerator ScaleTriggerImage(GameObject image, Vector3 targetScale)
    {
        Vector3 startScale = image.transform.localScale;
        float time = 0f;

        while (time < 0.2f)
        {
            time += Time.deltaTime;
            image.transform.localScale = Vector3.Lerp(startScale, targetScale, time / 0.2f);
            yield return null;
        }
        image.transform.localScale = targetScale;
    }




}
//줄 서는 위치
public enum orderDiction
{
    left_up,
    right_up,
    left_down,
    right_down,
    up_left,
    up_right,
    down_left,
    down_right,
}