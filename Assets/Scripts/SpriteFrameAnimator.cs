using UnityEngine;

public class SpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] Sprite[] frames;
    [SerializeField] float fps = 12f;
    [SerializeField] bool loop = true;

    SpriteRenderer sr;
    float timer;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && frames != null && frames.Length > 0) sr.sprite = frames[0];
    }

    void Update()
    {
        if (sr == null || frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        int i = (int)(timer * fps);
        i = loop ? i % frames.Length : Mathf.Min(i, frames.Length - 1);
        sr.sprite = frames[i];
    }
}