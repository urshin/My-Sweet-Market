using Cinemachine;
using System.Collections;
using UnityEngine;

public enum CamEnum
{
    player,
    table,
    wall,
}

public class CameraManager : MonoBehaviour
{
    private static CameraManager instance = null;


    private CinemachineMixingCamera mixCam;
    public CamEnum currentCam;
    private WaitForSeconds waitForOneSec;

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
    private void Start()
    {
        mixCam = GetComponent<CinemachineMixingCamera>();
        currentCam = CamEnum.player;
        waitForOneSec = new WaitForSeconds(1);
    }

    public void Show(params CamEnum[] cams)
    {
        StopAllCoroutines();  // 기존 전환 중단
        StartCoroutine(SwitchCamerasSequence(cams));
    }

    //카메라 시점 전환
    private IEnumerator SwitchCamerasSequence(CamEnum[] cams)
    {
        CamEnum fromCam = CamEnum.player;
        yield return new WaitForSeconds(0.5f);  // 대기


        for (int i = 0; i < cams.Length; i++)
        {
            CamEnum toCam = cams[i];

            //배열의 순서대로 이동
            yield return StartCoroutine(SwitchCamera(fromCam, toCam));
            yield return waitForOneSec;  // 1초 대기

            // 현재 위치를 출발점으로 설정
            fromCam = toCam;
        }

        //플레이어 원래 카메라로 돌아오기
        if (fromCam != CamEnum.player)
        {
            yield return StartCoroutine(SwitchCamera(fromCam, CamEnum.player));
        }
    }


    // 카메라 전환
    private IEnumerator SwitchCamera(CamEnum from, CamEnum to)
    {
        int fromIndex = (int)from;
        int toIndex = (int)to;

        float t = 0f;
        float duration = 0.8f;

        float startFromWeight = mixCam.GetWeight(fromIndex);
        float startToWeight = mixCam.GetWeight(toIndex);

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / duration);

            mixCam.SetWeight(fromIndex, Mathf.Lerp(startFromWeight, 0f, lerp));
            mixCam.SetWeight(toIndex, Mathf.Lerp(startToWeight, 1f, lerp));

            yield return null;
        }

        mixCam.SetWeight(fromIndex, 0f);
        mixCam.SetWeight(toIndex, 1f);

        currentCam = to;
    }
}
