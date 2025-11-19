using UnityEngine;

public static class AudioSourceExtensions
{
    public static void PlayRandomPitch(this AudioSource source, float min = 0.97f, float max = 1.03f)
    {
        source.pitch = Random.Range(min, max);
        source.Play();
    }

    public static void PlayOneShotRandomPitch(this AudioSource source, AudioClip clip, float min = 0.92f, float max = 1.08f)
    {
        source.pitch = Random.Range(min, max);
        source.PlayOneShot(clip);
    }

    public static void PlayClipAtPointRandomPitch(AudioClip clip, Vector3 position, float volume = 1f, float minPitch = 0.92f, float maxPitch = 1.08f)
    {
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.Play();
        Object.Destroy(tempGO, clip.length);
    }

    public static void PlayRandomClipWithPitch(this AudioSource source, AudioClip[] clips, float min = 0.97f, float max = 1.03f)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip randomClip = clips[Random.Range(0, clips.Length)];
        if (randomClip == null) return;

        source.clip = randomClip;
        source.pitch = Random.Range(min, max);
        source.Play();
    }

    public static void PlayOneShotRandomClipWithPitch(this AudioSource source, AudioClip[] clips, float min = 0.92f, float max = 1.08f)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip randomClip = clips[Random.Range(0, clips.Length)];
        if (randomClip == null) return;

        source.pitch = Random.Range(min, max);
        source.PlayOneShot(randomClip);
    }

    public static void PlayRandomClipAtPointWithPitch(AudioClip[] clips, Vector3 position, float volume = 1f, float minPitch = 0.92f, float maxPitch = 1.08f)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip randomClip = clips[Random.Range(0, clips.Length)];
        if (randomClip == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = randomClip;
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.Play();
        Object.Destroy(tempGO, randomClip.length);
    }
}
