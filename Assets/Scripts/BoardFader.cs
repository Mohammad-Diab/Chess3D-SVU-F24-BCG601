using System.Collections;
using UnityEngine;

public class BoardFader : MonoBehaviour
{
    public float duration = 0.5f;
    // public GameObject board;
    public Renderer table;

    private Renderer[] _renderers;

    void Awake()
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            _renderers = new Renderer[] { table };
        }
    }

    void Start()
    {
        foreach (var rend in _renderers)
        {
            Color c = rend.material.color;
            c.a = 0f;
            rend.material.color = c;
        }
    }


    public void FadeIn() => StartCoroutine(Fade(0f, 1f));
    public void FadeOut() => StartCoroutine(Fade(1f, 0f));

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            foreach (var rend in _renderers)
            {
                Color c = rend.material.color;
                c.a = Mathf.Lerp(from, to, elapsed / duration);
                rend.material.color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        foreach (var rend in _renderers)
        {
            Color c = rend.material.color;
            c.a = to;
            rend.material.color = c;
        }
    }
}