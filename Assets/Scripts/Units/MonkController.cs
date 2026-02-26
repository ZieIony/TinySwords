using UnityEngine;

public class MonkController : UnitController {
    public AudioClip healSound;
    public GameObject healEffect;

    protected override void Awake() {
        base.Awake();
        className = "Monk";
    }

    protected override void Start() {
        base.Start();
        audioSource = GetComponent<AudioSource>();
    }

    public async Awaitable Heal(UnitController target, int hp) {
        animator.Play("heal");
        var currentEffect = Instantiate(healEffect, target.transform);
        await Awaitable.NextFrameAsync();
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("heal"))
            await Awaitable.NextFrameAsync();
        //currentEffect.transform.position = new Vector3(15.0f, 0.0f, 0.0f);
        //Destroy(currentEffect);
        target.GetHealed(hp);
    }

    public override async Awaitable PerformAction(UnitController target) {
        audioSource.PlayOneShot(healSound);
        await Heal(target, damage);
    }

    public override bool CanPerformAction(UnitController target) {
        if (target == this && canBeHealed)
            return true;
        if (target.armyColor == armyColor && target.canBeHealed && CanReach(target))
            return true;
        return base.CanPerformAction(target);
    }

    public override ActionType GetActionType(UnitController target) {
        if (CanPerformAction(target))
            return ActionType.Heal;
        return base.GetActionType(target);
    }
}
