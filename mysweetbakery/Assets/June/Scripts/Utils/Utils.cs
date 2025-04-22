using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class Utils
{
    /// <summary>
    /// 베지어 곡선으로 움직이기
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="start"></param>
    /// <param name="control"></param>
    /// <param name="end"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public static IEnumerator MoveBezier(Transform obj, Vector3 start, Vector3 control, Vector3 end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 pos = Mathf.Pow(1 - t, 2) * start + 2 * (1 - t) * t * control + Mathf.Pow(t, 2) * end;

            obj.position = pos;
            yield return null;
        }

        obj.position = end;
    }

    //가방 넘겨주기
    public static void GetTheBag(GameObject bag, Transform bag_Position, Quaternion quaternion)
    {
        bag.transform.SetParent(bag_Position, true);

        bag.transform.localPosition = new Vector3(0, 0, 0);
        bag.transform.localRotation = quaternion;
    }

    // Stack에 크로아상을 추가하고 위치 세팅
    public static void AddCroassant(Stack<GameObject> stack, GameObject croassant, Transform parent, Quaternion quaternion, float height = 0.5f)
    {
        stack.Push(croassant);
        croassant.transform.SetParent(parent, true);

        int index = stack.Count - 1;
        croassant.transform.localPosition = new Vector3(0, index * height, 0);
        croassant.transform.localRotation = quaternion;
    }

    /// <summary>
    /// 돈계산
    /// </summary>
    /// <param name="croassantCount"></param>
    /// <param name="moneyPosition"></param>
    /// <param name="stack"></param>
    public static void CalculateMoney(int croassantCount, Transform moneyPosition, Stack<GameObject> stack)
    {
        // 돈 생성
        int row = 3;  // X 방향 (가로)
        int col = 3;  // Z 방향 (세로)

        Vector3 moneySize = DataManager.Instance.money_Prefab.GetComponent<Renderer>().bounds.size;
        Vector3 padding = moneySize * 1.1f;

        int currnetMoneyCount = stack.Count;
        for (int i = 0; i < croassantCount * 10; i++)
        {
            int moneyIndex = currnetMoneyCount + i;

            int x = moneyIndex % row;
            int z = (moneyIndex / row) % col;
            int y = moneyIndex / (row * col);

            int reversedZ = (col - 1) - z;

            Vector3 offset = new Vector3(
                x * padding.z,
                y * padding.y,
                reversedZ * padding.x
            );

            stack.Push(ObjectPool.Instance.GetObject(DataManager.Instance.money_Prefab, moneyPosition.position + offset, Quaternion.Euler(0, 90, 0)));
        }
        SoundManager.instance.PlayAudio(audioSource.cash);

    }
    /// <summary>
    /// 돈 하나씩 플레이어가 받기
    /// </summary>
    /// <param name="hasMoney"></param>
    /// <param name="getMoney"></param>
    /// <param name="onCollect"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public static IEnumerator PlayerTakeMoneyRoutine(
     Func<bool> hasMoney,
     Func<GameObject> getMoney,
     Action<GameObject> onCollect,
     Action onComplete)
    {
        while (hasMoney())
        {
            GameObject money = getMoney();

            yield return MoveBezier(money.transform, money.transform.position, Player.Instance.transform.position + Vector3.up * 4f, Player.Instance.transform.position, 0.05f);
            GameManager.Instance.playerMoneyCount++;
            SoundManager.instance.PlayAudio(audioSource.Get_Money);


            onCollect?.Invoke(money);
        }

        onComplete?.Invoke();
    }
    /// <summary>
    /// 팝업 효과
    /// </summary>
    /// <param name="popUpObjects"></param>
    /// <returns></returns>
    public static IEnumerator PopUp(GameObject[] popUpObjects)
    {
        float popUpDuration = GameManager.Instance.popUpDuration;
        AnimationCurve scaleXCurve = GameManager.Instance.scaleXCurve;
        AnimationCurve scaleYCurve = GameManager.Instance.scaleYCurve;

        // 시작 스케일을 0으로 초기화
        foreach (GameObject obj in popUpObjects)
        {
            if (obj != null)
                obj.transform.localScale = Vector3.zero;
        }

        float time = 0f;

        while (time < popUpDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / popUpDuration);

            float scaleX = scaleXCurve.Evaluate(t);
            float scaleY = scaleYCurve.Evaluate(t);

            Vector3 scale = new Vector3(scaleX, scaleY, 1f);

            foreach (GameObject obj in popUpObjects)
            {
                if (obj != null)
                    obj.transform.localScale = scale;
            }

            yield return null;
        }

        // 마지막 보정
        foreach (GameObject obj in popUpObjects)
        {
            if (obj != null)
                obj.transform.localScale = Vector3.one;
        }

    }

}
