using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public Sprite volar;
    public Sprite transicion;
    public Sprite bajar;

    public float tiempoLimite = 1.5f;

    private SpriteRenderer sr;
    private PlayerJetpack jetpack;

    private float tiempoSinPresionar = 0f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        jetpack = GetComponent<PlayerJetpack>();
    }

    private void Update()
    {
        if (sr == null || jetpack == null)
        {
            return;
        }

        if (jetpack.IsThrusting)
        {
            SetSpriteIfAvailable(volar);
            tiempoSinPresionar = 0f;
        }
        else
        {
            tiempoSinPresionar += Time.deltaTime;

            if (tiempoSinPresionar < tiempoLimite)
            {
                SetSpriteIfAvailable(transicion);
            }
            else
            {
                SetSpriteIfAvailable(bajar);
            }
        }
    }

    private void SetSpriteIfAvailable(Sprite sprite)
    {
        if (sprite != null)
        {
            sr.sprite = sprite;
        }
    }
}
