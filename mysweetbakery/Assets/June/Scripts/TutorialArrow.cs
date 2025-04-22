using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialArrow : MonoBehaviour
{
    [SerializeField] Transform arrowPrefab;

    private Vector3 startPosition;


    void Start()
    {
        if (arrowPrefab != null)
            startPosition = arrowPrefab.localPosition;  // 시작 위치 저장
    }

    void Update()
    {
        if (arrowPrefab == null) return;

     
        float y = Mathf.Sin(Time.time * 5) * 0.6f;

        
        arrowPrefab.localPosition = startPosition + new Vector3(0, y, 0);
    }
}
