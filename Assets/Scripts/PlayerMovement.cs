using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Velocidades (unidades por segundo)")]
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float runSpeed = 4.5f;
    [SerializeField] float crouchSpeed = 1.2f;

    [Header("Teclas")]
    [SerializeField] Key runKey = Key.LeftShift;

    [Header("Sprite")]
    [SerializeField] bool flipSpriteWithDirection = true;

    Animator anim;
    SpriteRenderer sprite;

    static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Parado se estiver morto
        if (anim != null && anim.GetBool(IsDeadHash)) return;

        float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f)
                - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);

        Vector2 move = Vector2.ClampMagnitude(new Vector2(x, y), 1f);

        bool crouching = anim != null && anim.GetBool(IsCrouchingHash);
        float speed = crouching ? crouchSpeed : (kb[runKey].isPressed ? runSpeed : walkSpeed);

        transform.position += (Vector3)(move * speed * Time.deltaTime);

        if (flipSpriteWithDirection && sprite != null && Mathf.Abs(move.x) > 0.01f)
            sprite.flipX = move.x < 0f;
    }
}