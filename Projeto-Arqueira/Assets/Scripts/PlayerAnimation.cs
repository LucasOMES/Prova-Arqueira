using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Velocidades (batem com os thresholds do Animator)")]
    [SerializeField] float walkSpeed = 2f;    // entre 0.1 e 2.5 -> Walk
    [SerializeField] float runSpeed = 5f;     // acima de 3 -> Run
    [SerializeField] float crouchSpeed = 1.5f;

    [Header("Combo")]
    [SerializeField] float comboResetTime = 1f; // tempo sem atacar para voltar ao Attack1

    [Header("Teclas")]
    [SerializeField] Key runKey = Key.LeftShift;
    [SerializeField] Key crouchKey = Key.C;
    [SerializeField] Key quickShotKey = Key.Q;
    [SerializeField] Key kickKey = Key.E;
    [SerializeField] Key castSpellKey = Key.R;
    [SerializeField] Key ability1Key = Key.Digit1;
    [SerializeField] Key ability2Key = Key.Digit2;
    [SerializeField] Key rollKey = Key.LeftCtrl;
    [SerializeField] Key frontFlipKey = Key.Space;
    [SerializeField] Key hitKey = Key.H;   // teste: levar dano
    [SerializeField] Key dieKey = Key.K;   // teste: morrer
    // Ataque = botão esquerdo do mouse

    Animator anim;
    int attackIndex;
    float lastAttackTime;
    bool crouching;
    bool dead;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int DirXHash = Animator.StringToHash("DirX");
    static readonly int DirYHash = Animator.StringToHash("DirY");
    static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    static readonly int IsDeadHash = Animator.StringToHash("IsDead");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");
    static readonly int AttackRunHash = Animator.StringToHash("AttackRun");
    static readonly int QuickShotHash = Animator.StringToHash("QuickShot");
    static readonly int KickHash = Animator.StringToHash("Kick");
    static readonly int CastSpellHash = Animator.StringToHash("CastSpell");
    static readonly int Ability1Hash = Animator.StringToHash("Ability1");
    static readonly int Ability2Hash = Animator.StringToHash("Ability2");
    static readonly int RollHash = Animator.StringToHash("Roll");
    static readonly int FrontFlipHash = Animator.StringToHash("FrontFlip");
    static readonly int HitHash = Animator.StringToHash("Hit");

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogError("PlayerAnimation: nenhum Animator encontrado neste objeto ou nos filhos.", this);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (dead || anim == null || kb == null) return;

        HandleMovement(kb);
        HandleCrouch(kb);
        HandleActions(kb);
        HandleComboReset();
    }

    void HandleMovement(Keyboard kb)
    {
        float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f)
                - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);

        Vector2 move = Vector2.ClampMagnitude(new Vector2(x, y), 1f);

        float targetSpeed;
        if (crouching) targetSpeed = crouchSpeed;
        else if (kb[runKey].isPressed) targetSpeed = runSpeed;
        else targetSpeed = walkSpeed;

        float speed = move.magnitude * targetSpeed;

        // Negativo ao andar só para trás (transição Walk <-> Run Backwards)
        if (move.y < -0.1f && Mathf.Abs(move.x) < 0.5f) speed = -speed;

        anim.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        anim.SetFloat(DirXHash, move.x);
        anim.SetFloat(DirYHash, move.y);
    }

    void HandleCrouch(Keyboard kb)
    {
        if (!kb[crouchKey].wasPressedThisFrame) return;
        crouching = !crouching;
        anim.SetBool(IsCrouchingHash, crouching);
    }

    void HandleActions(Keyboard kb)
    {
        var mouse = Mouse.current;

        // Correndo dispara AttackRun; parado dispara o combo 1-2-3
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (anim.GetFloat(SpeedHash) > 3f)
            {
                anim.SetTrigger(AttackRunHash);
            }
            else
            {
                attackIndex = (attackIndex % 3) + 1;
                anim.SetInteger(AttackIndexHash, attackIndex);
                anim.SetTrigger(AttackHash);
                lastAttackTime = Time.time;
            }
        }

        if (kb[quickShotKey].wasPressedThisFrame) anim.SetTrigger(QuickShotHash);
        if (kb[kickKey].wasPressedThisFrame) anim.SetTrigger(KickHash);
        if (kb[castSpellKey].wasPressedThisFrame) anim.SetTrigger(CastSpellHash);
        if (kb[ability1Key].wasPressedThisFrame) anim.SetTrigger(Ability1Hash);
        if (kb[ability2Key].wasPressedThisFrame) anim.SetTrigger(Ability2Hash);
        if (kb[rollKey].wasPressedThisFrame) anim.SetTrigger(RollHash);

        // Front Flip só vale durante o Rolling (evita o trigger ficar guardado)
        if (kb[frontFlipKey].wasPressedThisFrame &&
            anim.GetCurrentAnimatorStateInfo(0).IsName("Rolling"))
            anim.SetTrigger(FrontFlipHash);

        // Teclas de teste para dano e morte
        if (kb[hitKey].wasPressedThisFrame) TakeHit();
        if (kb[dieKey].wasPressedThisFrame) Die();
    }

    void HandleComboReset()
    {
        if (attackIndex != 0 && Time.time - lastAttackTime > comboResetTime)
        {
            attackIndex = 0;
            anim.SetInteger(AttackIndexHash, 0);
        }
    }

    // Chame pelo seu sistema de vida/dano
    public void TakeHit()
    {
        if (dead || anim == null) return;
        anim.SetTrigger(HitHash);
    }

    public void Die()
    {
        if (dead || anim == null) return;
        dead = true;
        anim.SetBool(IsDeadHash, true);
        StartCoroutine(FreezeAfterDeath());
    }

    System.Collections.IEnumerator FreezeAfterDeath()
    {
        // 1) espera o Animator realmente entrar no estado "Die" (fim do blend)
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Die") || anim.IsInTransition(0))
            yield return null;

        // 2) espera a animação tocar até o fim uma vez
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.98f)
            yield return null;

        // 3) congela no último frame (personagem no chão)
        anim.speed = 0f;
    }
}