using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ConditionalBGM : MonoBehaviour
{
    private AudioSource audioSource;
    public float fadeDuration = 2f; // time in seconds for fade out/in

    private static ConditionalBGM instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // avoid duplicate
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.StartsWith("FieldScene"))
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            StartCoroutine(FadeIn());
        }
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Pause();
    }

    IEnumerator FadeIn()
    {
        float targetVolume = 0.6f; // your preferred volume
        float startVolume = audioSource.volume;

        // ✅ 不再重播，只在未播放时才播放
        if (!audioSource.isPlaying)
            audioSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }


    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
