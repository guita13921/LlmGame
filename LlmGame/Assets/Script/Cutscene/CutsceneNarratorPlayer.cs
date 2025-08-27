using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutsceneNarratorPlayer : MonoBehaviour
{
    [System.Serializable]
    public class Slide
    {
        [Tooltip("Sprite file base in Resources/Cutscenes/ (without extension). Example: I1_TenSuns_HouYi")]
        public string spriteBaseName;
        [TextArea(6, 16)]
        public string narration;
        [Tooltip("Optional voiceover clip (full line).")]
        public AudioClip voiceOver;
    }

    [Header("Slides (ordered)")]
    public List<Slide> slides = new List<Slide>();

    [Header("UI")]
    public Image imageTarget;              // Fullscreen Image
    public TMP_Text narrationText;         // Narration subtitle text
    public CanvasGroup cgImage;            // CanvasGroup on imageTarget
    public CanvasGroup cgNarration;        // CanvasGroup on narrationText
    public TMP_Text continueHint;          // Optional "Press [Space]" hint (can be null)

    [Header("Typing")]
    [Range(10, 1200)] public float charsPerSecond = 45f;
    [Tooltip("Multiplier delay for , ; :")] public float commaPause = 0.35f;
    [Tooltip("Multiplier delay for . ! ?")] public float periodPause = 0.6f;
    [Tooltip("Multiplier delay for line breaks")] public float lineBreakPause = 0.8f;
    public KeyCode advanceKey = KeyCode.Space;
    public KeyCode fastKey = KeyCode.Space;
    public bool allowMouseClickAdvance = true;

    [Header("Fades")]
    public float fadeInImage = 1.0f;
    public float fadeOutImage = 1.0f;
    public float fadeInText = 0.4f;
    public float fadeOutText = 0.3f;

    [Header("Audio")]
    public AudioSource voiceSource;        // Optional (for voiceOver)
    public AudioSource sfxSource;          // Optional (for typewriter ticks)
    public AudioClip typeTick;             // Optional short tick per few chars

    public System.Action OnCutsceneFinished;

    bool _playing;
    bool _skipTyping;

    void Awake()
    {
        if (cgImage) cgImage.alpha = 0f;
        if (cgNarration) cgNarration.alpha = 0f;
        if (continueHint) continueHint.alpha = 0f;
    }

    public void Play()
    {
        if (!_playing) StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        _playing = true;

        for (int i = 0; i < slides.Count; i++)
        {
            var slide = slides[i];

            // Load sprite
            var sprite = Resources.Load<Sprite>("Cutscenes/" + slide.spriteBaseName);
            if (sprite == null)
                Debug.LogWarning($"Cutscene: Missing sprite Resources/Cutscenes/{slide.spriteBaseName}.png");

            imageTarget.sprite = sprite;
            narrationText.text = "";

            // Fade in image
            yield return Fade(cgImage, cgImage.alpha, 1f, fadeInImage);

            // VoiceOver (if any)
            if (voiceSource && slide.voiceOver)
            {
                voiceSource.clip = slide.voiceOver;
                voiceSource.Play();
            }

            // Show text (typewriter)
            yield return Fade(cgNarration, cgNarration.alpha, 1f, fadeInText);
            yield return TypeText(slide.narration);

            // Wait for advance
            if (continueHint) yield return FadeTMPAlpha(continueHint, continueHint.alpha, 1f, 0.2f);
            yield return WaitForAdvance();
            if (continueHint) yield return FadeTMPAlpha(continueHint, continueHint.alpha, 0f, 0.15f);

            // Fade out text + image
            yield return Fade(cgNarration, cgNarration.alpha, 0f, fadeOutText);
            yield return Fade(cgImage, cgImage.alpha, 0f, fadeOutImage);

            // Stop VO if still playing
            if (voiceSource && voiceSource.isPlaying) voiceSource.Stop();
        }

        _playing = false;
        OnCutsceneFinished?.Invoke();
    }

    IEnumerator TypeText(string fullText)
    {
        narrationText.text = "";
        _skipTyping = false;
        int tickEvery = 3;
        int charCountSinceTick = 0;

        for (int i = 0; i < fullText.Length; i++)
        {
            // fast-forward while holding fastKey
            bool fast = Input.GetKey(fastKey);

            // If player taps advance during typing, reveal instantly
            if (allowMouseClickAdvance && Input.GetMouseButtonDown(0)) fast = true;
            if (Input.GetKeyDown(advanceKey)) fast = true;

            if (fast && !_skipTyping)
            {
                _skipTyping = true;
            }

            if (_skipTyping)
            {
                narrationText.text = fullText;
                break;
            }

            narrationText.text = fullText.Substring(0, i + 1);

            // optional tick
            if (sfxSource && typeTick)
            {
                charCountSinceTick++;
                if (charCountSinceTick >= tickEvery && !char.IsWhiteSpace(fullText[i]))
                {
                    sfxSource.PlayOneShot(typeTick, 0.25f);
                    charCountSinceTick = 0;
                }
            }

            // smart pauses
            float delay = 1f / Mathf.Max(1f, charsPerSecond);
            char c = fullText[i];
            if (c == ',' || c == ';' || c == ':') delay *= (1f + commaPause);
            else if (c == '.' || c == '!' || c == '?') delay *= (1f + periodPause);
            else if (c == '\n')
            {
                // If it's a blank line (double line break), add extra pause
                bool blankLineBreak = (i + 1 < fullText.Length && fullText[i + 1] == '\n');
                delay *= blankLineBreak ? (1f + lineBreakPause * 1.2f) : (1f + lineBreakPause);
            }

            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator WaitForAdvance()
    {
        // Wait for advanceKey or mouse click
        while (true)
        {
            if (Input.GetKeyDown(advanceKey)) yield break;
            if (allowMouseClickAdvance && Input.GetMouseButtonDown(0)) yield break;
            yield return null;
        }
    }

    IEnumerator Fade(CanvasGroup cg, float a, float b, float d)
    {
        if (!cg) yield break;
        float t = 0f;
        while (t < d)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(a, b, t / d);
            yield return null;
        }
        cg.alpha = b;
    }

    IEnumerator FadeTMPAlpha(TMP_Text tmp, float a, float b, float d)
    {
        if (!tmp) yield break;
        float t = 0f;
        while (t < d)
        {
            t += Time.deltaTime;
            var c = tmp.color;
            c.a = Mathf.Lerp(a, b, t / d);
            tmp.color = c;
            yield return null;
        }
        var c2 = tmp.color; c2.a = b; tmp.color = c2;
    }
}
