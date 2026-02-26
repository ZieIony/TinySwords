using UnityEngine;

public class WarriorController : UnitController {
    public AudioClip swordSound;

    protected override void Awake() {
        base.Awake();
        className = "Warrior";
    }

    protected override void Start() {
        base.Start();
        audioSource = GetComponent<AudioSource>();
    }

    public override async Awaitable PerformAction(UnitController target) {
        audioSource.PlayOneShot(swordSound);
        await Attack(target, damage);
    }

    public override bool CanPerformAction(UnitController target) {
        if (target.armyColor != armyColor && CanReach(target))
            return true;
        return base.CanPerformAction(target);
    }

    public override ActionType GetActionType(UnitController target) {
        if (CanPerformAction(target))
            return ActionType.Attack;
        return base.GetActionType(target);
    }
}
