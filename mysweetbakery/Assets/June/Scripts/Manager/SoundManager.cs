using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum audioSource
{
    cash,
    Cost_Money,
    Get_Money,
    Get_Object,
    Put_Object,
    Success,
    trash,
    clean,
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    //오디오, 이넘 짝 지어주기
    [System.Serializable]
    public struct AudioData
    {
        public audioSource key;
        public AudioClip clip;
    }

    [Header("Audio Clips")]
    public List<AudioData> audioDataList = new List<AudioData>();

    [Header("Audio Mixer")]
    public AudioMixerGroup outputMixerGroup;

    //오디오소스 오브젝트 풀 따로 관리
    [Header("AudioSource Pool")]
    public int poolSize = 10;
    private List<AudioSource> audioSourcePool = new List<AudioSource>();

    private Dictionary<audioSource, AudioClip> audioClipDict = new Dictionary<audioSource, AudioClip>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioDictionary();
        CreateAudioSourcePool();
    }

    //딕셔너리 초기화
    private void InitializeAudioDictionary()
    {
        foreach (var data in audioDataList)
        {
            if (!audioClipDict.ContainsKey(data.key))
                audioClipDict.Add(data.key, data.clip);
        }
    }

    //오디오 풀 만들기
    private void CreateAudioSourcePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject($"PooledAudioSource_{i}");
            go.transform.SetParent(this.transform);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = outputMixerGroup;
            audioSourcePool.Add(source);
        }
    }

    //오디오 가져오기
    private AudioSource GetAvailableAudioSource()
    {
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying)
                return source;
        }

        // 만약 전부 재생 중이면, 풀을 늘려서 제공
        GameObject go = new GameObject($"PooledAudioSource_Extra");
        go.transform.SetParent(this.transform);
        AudioSource extraSource = go.AddComponent<AudioSource>();
        extraSource.playOnAwake = false;
        extraSource.outputAudioMixerGroup = outputMixerGroup;
        audioSourcePool.Add(extraSource);
        return extraSource;
    }

    public void PlayAudio(audioSource type)
    {
        //사운드 예외
        if (!audioClipDict.ContainsKey(type))
        {
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        source.clip = audioClipDict[type];
        source.Play();
    }
}
