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

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        jetpack = GetComponent<PlayerJetpack>();
    }

    void Update()
    {
        if (jetpack.IsThrusting)
        {
            sr.sprite = volar;
            tiempoSinPresionar = 0f;
        }
        else
        {
            tiempoSinPresionar += Time.deltaTime;

            if (tiempoSinPresionar < tiempoLimite)
            {
                sr.sprite = transicion;
            }
            else
            {
                sr.sprite = bajar;
            }
        }
    }
}