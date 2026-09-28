using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] Transform target;          // arraste o Player aqui
    [SerializeField] string targetTag = "Player"; // usado se o alvo não for arrastado

    [Header("Ajustes")]
    [SerializeField] Vector2 offset = Vector2.zero;
    [SerializeField] float smoothTime = 0.15f;  // 0 = segue instantaneamente

    Vector3 velocity;
    float fixedZ;

    void Awake()
    {
        fixedZ = transform.position.z; // mantém o Z da câmera (normalmente -10)
    }

    void Start()
    {
        if (target == null)
        {
            var obj = GameObject.FindGameObjectWithTag(targetTag);
            if (obj != null) target = obj.transform;
            else Debug.LogWarning("CameraFollow: nenhum alvo definido e nenhum objeto com a tag '" + targetTag + "'.", this);
        }
    }

    // LateUpdate: roda depois do movimento do jogador, evitando tremidas
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            fixedZ);

        transform.position = smoothTime > 0f
            ? Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime)
            : desired;
    }
}