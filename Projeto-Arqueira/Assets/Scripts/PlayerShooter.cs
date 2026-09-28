using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject arrowPrefab;      // ArrowPrefab
    [SerializeField] GameObject castSpellPrefab;  // ex.: FireSpellPrefab
    [SerializeField] GameObject ability1Prefab;   // ex.: IceSpellPrefab
    [SerializeField] GameObject ability2Prefab;   // ex.: PoisonSpellPrefab

    [Header("Ajustes")]
    [SerializeField] float spawnDistance = 0.8f;  // distância do centro do jogador
    [SerializeField] float arrowSpeed = 10f;
    [SerializeField] float spellSpeed = 6f;
    [SerializeField] float arrowDelay = 0.25f;    // espera até o ponto da animação em que solta
    [SerializeField] float spellDelay = 0.3f;

    [Header("Teclas (as mesmas do PlayerAnimation)")]
    [SerializeField] Key castSpellKey = Key.R;
    [SerializeField] Key ability1Key = Key.Digit1;
    [SerializeField] Key ability2Key = Key.Digit2;

    Animator anim;
    Collider2D playerCollider;
    Vector2 facing = Vector2.right; // direção padrão (a mesma do DirX/DirY = 0 no Blend Tree)

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || anim == null) return;
        if (anim.GetBool(IsDeadHash)) return;

        UpdateFacing(kb);

        // Ataque com o mouse: parado = combo (Attack1-2-3) lança flecha.
        // Correndo (Speed > 3) é o AttackRun, que não lança nada.
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && anim.GetFloat(SpeedHash) <= 3f)
            StartCoroutine(Shoot(arrowPrefab, arrowSpeed, arrowDelay));

        if (kb[castSpellKey].wasPressedThisFrame) StartCoroutine(Shoot(castSpellPrefab, spellSpeed, spellDelay));
        if (kb[ability1Key].wasPressedThisFrame) StartCoroutine(Shoot(ability1Prefab, spellSpeed, spellDelay));
        if (kb[ability2Key].wasPressedThisFrame) StartCoroutine(Shoot(ability2Prefab, spellSpeed, spellDelay));
    }

    // Guarda a última direção em que o jogador andou
    void UpdateFacing(Keyboard kb)
    {
        float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f)
                - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);

        var dir = new Vector2(x, y);
        if (dir.sqrMagnitude > 0.01f) facing = dir.normalized;
    }

    IEnumerator Shoot(GameObject prefab, float speed, float delay)
    {
        if (prefab == null) yield break;

        // espera a animação chegar no momento certo do lançamento
        yield return new WaitForSeconds(delay);

        if (anim.GetBool(IsDeadHash)) yield break; // morreu no meio do caminho

        Vector2 dir = facing;
        Vector3 pos = transform.position + (Vector3)(dir * spawnDistance);
        var obj = Instantiate(prefab, pos, Quaternion.identity);

        // evita que o projétil bata no próprio jogador
        if (playerCollider != null)
            foreach (var c in obj.GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(c, playerCollider);

        var proj = obj.GetComponent<Projectile>();
        if (proj == null) proj = obj.AddComponent<Projectile>();
        proj.speed = speed;
        proj.Launch(dir);
    }
}