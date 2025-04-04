using UnityEngine;

public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// Terminar esta clase con el video de chadrtronic
    /// </summary>
    public static SoundManager instance { get; private set; }

    private AudioSource[] sources;
    // 0: music
    // 1: ambient
    // 2: sound effects

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;

        sources = this.GetComponents<AudioSource>();
    }

    public void PlayMusic(AudioClip clip)
    {
        this.sources[0].clip = clip;
        if (!this.sources[0].isPlaying)
            this.sources[0].Play();
    }
    public void PlayAmbient(AudioClip clip)
    {
        this.sources[1].PlayOneShot(clip);
    }
    public void PlayEffect(AudioClip clip)
    {
        float lastCliplen = 0;
        if (this.sources[2].clip != null)
            lastCliplen = this.sources[2].clip.length;
        this.sources[2].clip = clip;
        this.sources[2].Play(System.Convert.ToUInt64(lastCliplen));
    }
}
