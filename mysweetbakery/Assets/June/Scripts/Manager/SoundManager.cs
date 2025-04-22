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

    //�����, �̳� ¦ �����ֱ�
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

    //������ҽ� ������Ʈ Ǯ ���� ����
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

    //��ųʸ� �ʱ�ȭ
    private void InitializeAudioDictionary()
    {
        foreach (var data in audioDataList)
        {
            if (!audioClipDict.ContainsKey(data.key))
                audioClipDict.Add(data.key, data.clip);
        }
    }

    //����� Ǯ �����
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

    //����� ��������
    private AudioSource GetAvailableAudioSource()
    {
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying)
                return source;
        }

        // ���� ���� ��� ���̸�, Ǯ�� �÷��� ����
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
        //���� ����
        if (!audioClipDict.ContainsKey(type))
        {
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        source.clip = audioClipDict[type];
        source.Play();
    }
}
