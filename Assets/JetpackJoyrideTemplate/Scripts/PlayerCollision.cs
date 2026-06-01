using UnityEngine;
using System.Collections;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private string obstacleTag = "Obstacle";
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 1.1f;

    private SpriteRenderer[] spriteRenderers;
    private float invulnerableUntil;
    private Coroutine feedbackRoutine;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleCollision(other.gameObject);
    }

    private void TryHandleCollision(GameObject other)
    {
        if (!other.CompareTag(obstacleTag) || GameManager.Instance == null)
        {
            return;
        }

        if (Time.time < invulnerableUntil)
        {
            return;
        }

        bool ended = GameManager.Instance.RegisterMistake();

        if (ended)
        {
            return;
        }

        invulnerableUntil = Time.time + invulnerabilityDuration;
        PlayHitFeedback();
    }

    private void PlayHitFeedback()
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine = StartCoroutine(HitFeedbackRoutine());
    }

    private IEnumerator HitFeedbackRoutine()
    {
        float endTime = Time.time + invulnerabilityDuration;

        while (Time.time < endTime)
        {
            SetRenderersAlpha(0.35f);
            yield return new WaitForSeconds(0.08f);
            SetRenderersAlpha(1f);
            yield return new WaitForSeconds(0.08f);
        }

        SetRenderersAlpha(1f);
        feedbackRoutine = null;
    }

    private void SetRenderersAlpha(float alpha)
    {
        if (spriteRenderers == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
            {
                continue;
            }

            Color color = spriteRenderers[i].color;
            color.a = alpha;
            spriteRenderers[i].color = color;
        }
    }
}
