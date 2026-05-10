using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Глобальный менеджер звуков. Синглтон с DontDestroyOnLoad.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Serializable]
    public class SfxEntry
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("SFX")]
    [Tooltip("Имена: CmdOn, CmdOff, Switch, TurnOn, TurnOff, Jump, PickUp.")]
    [SerializeField] private SfxEntry[] sfxEntries;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] steps3D;
    [SerializeField] private AudioClip[] steps2D;
    [Range(0f, 1f)]
    [SerializeField] private float stepsVolume = 0.7f;

    [Header("Background Music")]
    [SerializeField] private AudioClip music3D;
    [SerializeField] private AudioClip music2D;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float musicCrossfadeDuration = 0.5f;

    [Header("Master Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private Dictionary<string, SfxEntry> sfxMap;
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private AudioSource activeMusicSource;
    private AudioSource sfxOneShotSource;

    private Coroutine crossfadeRoutine;

    public enum MusicTrack { None, Track3D, Track2D }
    public MusicTrack CurrentTrack { get; private set; } = MusicTrack.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxMap = new Dictionary<string, SfxEntry>(StringComparer.OrdinalIgnoreCase);
        if (sfxEntries != null)
        {
            foreach (var e in sfxEntries)
            {
                if (e == null || string.IsNullOrEmpty(e.name) || e.clip == null) continue;
                sfxMap[e.name] = e;
            }
        }

        sfxOneShotSource = gameObject.AddComponent<AudioSource>();
        sfxOneShotSource.playOnAwake = false;

        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceA.loop = true;
        musicSourceA.playOnAwake = false;
        musicSourceA.volume = 0f;

        musicSourceB = gameObject.AddComponent<AudioSource>();
        musicSourceB.loop = true;
        musicSourceB.playOnAwake = false;
        musicSourceB.volume = 0f;

        activeMusicSource = musicSourceA;
    }

    private void Start()
    {
        PlayMusic3D();
    }

    /// <summary>Играет короткий звуковой эффект по имени.</summary>
    public void PlaySFX(string name)
    {
        if (string.IsNullOrEmpty(name) || sfxMap == null) return;

        if (!sfxMap.TryGetValue(name, out SfxEntry e))
        {
            Debug.LogWarning($"[SoundManager] SFX '{name}' не найден.");
            return;
        }

        sfxOneShotSource.PlayOneShot(e.clip, e.volume * sfxVolume);
    }

    public void PlayFootstep3D() => PlayRandomFromArray(steps3D, 1f);
    public void PlayFootstep3D(float volumeMul) => PlayRandomFromArray(steps3D, volumeMul);
    public void PlayFootstep2D() => PlayRandomFromArray(steps2D, 1f);
    public void PlayFootstep2D(float volumeMul) => PlayRandomFromArray(steps2D, volumeMul);

    private void PlayRandomFromArray(AudioClip[] arr, float volumeMul)
    {
        if (arr == null || arr.Length == 0) return;
        AudioClip clip = arr[UnityEngine.Random.Range(0, arr.Length)];
        if (clip == null) return;
        sfxOneShotSource.PlayOneShot(clip, stepsVolume * sfxVolume * Mathf.Clamp01(volumeMul));
    }

    public void PlayMusic3D() => PlayMusicTrack(MusicTrack.Track3D, music3D);
    public void PlayMusic2D() => PlayMusicTrack(MusicTrack.Track2D, music2D);

    private void PlayMusicTrack(MusicTrack track, AudioClip clip)
    {
        if (CurrentTrack == track) return;
        CurrentTrack = track;

        if (clip == null) return;

        if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
        crossfadeRoutine = StartCoroutine(CrossfadeRoutine(clip));
    }

    private IEnumerator CrossfadeRoutine(AudioClip newClip)
    {
        AudioSource fadingOut = activeMusicSource;
        AudioSource fadingIn = (activeMusicSource == musicSourceA) ? musicSourceB : musicSourceA;

        fadingIn.clip = newClip;
        fadingIn.volume = 0f;
        fadingIn.Play();

        float elapsed = 0f;
        float fromOut = fadingOut.volume;
        while (elapsed < musicCrossfadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / musicCrossfadeDuration);
            fadingOut.volume = Mathf.Lerp(fromOut, 0f, t);
            fadingIn.volume = Mathf.Lerp(0f, musicVolume, t);
            yield return null;
        }

        fadingOut.volume = 0f;
        fadingOut.Stop();
        fadingIn.volume = musicVolume;

        activeMusicSource = fadingIn;
        crossfadeRoutine = null;
    }

    public void SetSfxVolume(float v) => sfxVolume = Mathf.Clamp01(v);

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (activeMusicSource != null && crossfadeRoutine == null)
            activeMusicSource.volume = musicVolume;
    }
}
