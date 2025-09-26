using UnityEngine;
using System.Collections;

public class UIFader : MonoBehaviour
{
    public CanvasGroup target;
    public float duration = 0.5f;

    public void FadeIn()
    {
        StartCoroutine(Fade(0f, 1f));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            target.alpha = Mathf.Lerp(from, to, elapsed / duration);
            target.interactable = to > 0.5f;
            target.blocksRaycasts = to > 0.5f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.alpha = to;
        target.interactable = to > 0.5f;
        target.blocksRaycasts = to > 0.5f;
    }
}