using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 3f;
    public float rotationOffset = 0f; // ajuste se o sprite não aponta para a direita

    Vector2 direction = Vector2.right;

    // Chamado pelo PlayerShooter logo após criar o projétil
    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);

        // Se o prefab tiver Rigidbody2D, evita que caia por gravidade
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.gravityScale = 0f;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
}