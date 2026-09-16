// 文件路径: d:\unity1\Math GameJam\Assets\Scripts\Manager\FadeScreenTransition.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreenTransition : MonoBehaviour, ISceneTransition
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    private void Awake()
    {
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = false;
    }

    public IEnumerator PlayEnter()
    {
        fadeImage.raycastTarget = true;
        yield return FadeCoroutine(0f, 1f, fadeOutDuration);
    }

    public IEnumerator PlayExit()
    {
        yield return FadeCoroutine(1f, 0f, fadeInDuration);
        fadeImage.raycastTarget = false;
    }

    private IEnumerator FadeCoroutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(from, to, t);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, to);
    }
}