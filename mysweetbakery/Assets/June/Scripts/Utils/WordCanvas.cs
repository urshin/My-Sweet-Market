using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordCanvas : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        // ó�� ȸ���� ����
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // �� �����Ӹ��� ������ ȸ���� ����
        transform.rotation = initialRotation;
    }
}
