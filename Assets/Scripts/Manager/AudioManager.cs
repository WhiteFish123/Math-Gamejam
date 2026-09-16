using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour//音频管理器
{
    public static AudioManager instance;

    [SerializeField] private AudioDataBaseSO audioDB;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float crossfadeWait = 0.3f;

    private bool bgmShouldPlay;
    private string currentBgGroupName;
    private AudioClip lastMusicPlayed;
    private Coroutine currentBgmCo;
    private int bgmRetryCount;
    private const int BGM_MAX_RETRY = 3;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        AudioManager.instance.StartBGM("Bgm_MainMenu");   
    }

    private void Update()
    {
        if (!bgmShouldPlay) return;
        if (bgmSource.isPlaying) return;
        if (currentBgmCo != null) return;


        NextBGM(currentBgGroupName);
    }

    // ==================== BGM ====================

    public void StartBGM(string musicGroup)
    {
        bgmShouldPlay = true;
        bgmRetryCount = 0;

        if (musicGroup == currentBgGroupName && bgmSource.isPlaying)
            return;

        NextBGM(musicGroup);
    }

    public void NextBGM(string musicGroup)
    {
        if (!bgmShouldPlay) return;

        currentBgGroupName = musicGroup;

        var bgmData = audioDB.Get(musicGroup);
        if (bgmData == null || bgmData.clips == null || bgmData.clips.Count == 0)
        {
            if (bgmRetryCount >= BGM_MAX_RETRY) return;
            bgmRetryCount++;
            this.Invoke(nameof(RetryNextBGM), 0.5f);
            return;
        }

        AudioClip nextClip;
        if (bgmData.clips.Count == 1)
        {
            nextClip = bgmData.clips[0];
        }
        else
        {
            nextClip = bgmData.clips[Random.Range(0, bgmData.clips.Count)];
            int safety = 0;
            while (nextClip == lastMusicPlayed && bgmData.clips.Count > 1 && safety < 100)
            {
                nextClip = bgmData.clips[Random.Range(0, bgmData.clips.Count)];
                safety++;
            }
        }

        lastMusicPlayed = nextClip;

        if (currentBgmCo != null)
            StopCoroutine(currentBgmCo);

        currentBgmCo = StartCoroutine(SwitchMusicCo(nextClip, bgmData.maxVolume));
    }

    private void RetryNextBGM()
    {
        NextBGM(currentBgGroupName);
    }

    private IEnumerator SwitchMusicCo(AudioClip nextClip, float maxVolume)
    {
        if (bgmSource.isPlaying)
        {
            yield return StartCoroutine(FadeVolumeCo(bgmSource, 0f, fadeOutDuration));
        }

        bgmSource.clip = nextClip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        yield return new WaitForSeconds(crossfadeWait);

        yield return StartCoroutine(FadeVolumeCo(bgmSource, maxVolume, fadeInDuration));

        currentBgmCo = null;
        bgmRetryCount = 0;
    }

    public void StopMusic()
    {
        bgmShouldPlay = false;
        lastMusicPlayed = null;

        if (currentBgmCo != null)
        {
            StopCoroutine(currentBgmCo);
            currentBgmCo = null;
        }

        StartCoroutine(StopMusicCo());
    }

    private IEnumerator StopMusicCo()
    {
        if (bgmSource.isPlaying)
        {
            yield return StartCoroutine(FadeVolumeCo(bgmSource, 0f, fadeOutDuration));
            bgmSource.Stop();
        }
    }

    // ==================== SFX ====================

    public void PlayGlobalSFX(string soundName)
    {
        var data = audioDB.Get(soundName);
        if (data == null || data.clips == null || data.clips.Count == 0) return;

        var clip = data.GetRandomClip();
        if (clip == null) return;

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.volume = data.maxVolume;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayGlobalSFX(string soundName, float volumeMultiplier)
    {
        var data = audioDB.Get(soundName);
        if (data == null || data.clips == null || data.clips.Count == 0) return;

        var clip = data.GetRandomClip();
        if (clip == null) return;

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.volume = data.maxVolume * volumeMultiplier;
        sfxSource.PlayOneShot(clip);
    }

    // ==================== Volume Control ====================

    public void SetBGMVolume(float volume, float fadeTime = 0.5f)
    {
        if (currentBgmCo != null)
            StopCoroutine(currentBgmCo);
        currentBgmCo = StartCoroutine(FadeVolumeCo(bgmSource, volume, fadeTime));
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    public float GetBGMVolume()
    {
        return bgmSource.volume;
    }

    public float GetSFXVolume()
    {
        return sfxSource.volume;
    }

    // ==================== Utility ====================

    private IEnumerator FadeVolumeCo(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }
}