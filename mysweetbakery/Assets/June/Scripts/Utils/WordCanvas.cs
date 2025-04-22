using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordCanvas : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        // 처음 회전값 저장
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // 매 프레임마다 고정된 회전값 유지
        transform.rotation = initialRotation;
    }
}
