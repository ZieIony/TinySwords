using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitController : MonoBehaviour {
    public Sprite avatarBlue, avatarRed;

    protected Animator animator;
    public RuntimeAnimatorController blueAnimationController, redAnimationController;
    public ArmyColor armyColor;

    public Vector2Int tilePosition {
        get;
        private set;
    }

    public int attackRange;

    private VisualElement healthBarBackground, healthBar;

    public int maxHp;
    public int currentHp;

    public bool isAlive {
        get => currentHp > 0;
    }

    public uint moveSpeed = 3;
    protected AudioSource audioSource;
    public AudioClip runSound;

    protected virtual void Awake() {
        currentHp = maxHp;
    }

    protected virtual void Start() {
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = armyColor == ArmyColor.Red ? redAnimationController : blueAnimationController;

        tilePosition = new Vector2Int(Mathf.RoundToInt(gameObject.transform.position.x - 0.5f), Mathf.RoundToInt(gameObject.transform.position.y - 0.5f));
        transform.position = new Vector3(tilePosition.x + 0.5f, tilePosition.y + 0.5f, 0);

        healthBarBackground = GetComponent<UIDocument>().rootVisualElement.Q("healthBarBackground");
        healthBar = GetComponent<UIDocument>().rootVisualElement.Q("healthBar");
    }

    private void Update() {
        healthBarBackground.style.translate = new Vector2(0, 50f);
        healthBar.style.scale = new Vector2((float)currentHp / maxHp, 1);
    }

    public void TakeDamage(int damage) {
        animator.Play("hit");
        currentHp = Math.Max(0, currentHp - damage);
        if (currentHp == 0)
            transform.position = new Vector3(15.0f, 0.0f, 0.0f);
    }

    public bool canBeHealed {
        get => isAlive&& currentHp < maxHp;
    }

    public String className { get; protected set; }

    public int damage;

    public void GetHealed(int hp) {
        currentHp = Math.Min(maxHp, currentHp + hp);
    }

    public async Awaitable Attack(UnitController target, int damage) {
        animator.Play("attack");
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("attack"))
            await Awaitable.NextFrameAsync();
        target.TakeDamage(damage);
    }

    public virtual async Awaitable PerformAction(UnitController target) {
        await Awaitable.NextFrameAsync();
    }

    public async Awaitable Move(List<Vector2Int> path) {
        animator.Play("run");
        audioSource.PlayOneShot(runSound);
        float EPSILON = 0.01f;
        foreach (Vector2Int nextPos in path) {
            var targetSpritePos = new Vector2(nextPos.x + 0.5f, nextPos.y + 0.5f);
            while (Vector2.Distance(transform.position, targetSpritePos) > EPSILON) {
                transform.position = Vector2.MoveTowards(transform.position, targetSpritePos, moveSpeed * Time.deltaTime);
                await Awaitable.NextFrameAsync();
            }
            tilePosition = nextPos;
        }
        audioSource.Stop();
        animator.Play("idle");
    }

    public bool CanReach(UnitController target) {
        return attackRange + 0.1f >= Vector3.Distance(transform.position, target.transform.position);
    }

    public virtual bool CanPerformAction(UnitController target) {
        return false;
    }

    public virtual ActionType GetActionType(UnitController target) {
        return ActionType.None;
    }
}
