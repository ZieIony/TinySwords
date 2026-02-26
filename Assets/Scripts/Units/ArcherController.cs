using UnityEngine;

public class ArcherController : UnitController {
    public GameObject arrow;
    public AudioClip arrowSound;
    public AudioClip arrowHitSound;

    protected override void Awake() {
        base.Awake();
        className = "Archer";
    }

    protected override void Start() {
        base.Start();
        audioSource = GetComponent<AudioSource>();
    }

    public override async Awaitable PerformAction(UnitController target) {
        animator.Play("attack");
        float EPSILON = 0.01f;
        var currentArrow = Instantiate(arrow, transform);
        var offset = new Vector3(0, 0.5f, 0);
        currentArrow.transform.position = transform.position + offset;
        currentArrow.transform.right = (target.transform.position - transform.position) * transform.localScale.x;
        audioSource.PlayOneShot(arrowSound);
        while (Vector2.Distance(currentArrow.transform.position, target.transform.position + offset) > EPSILON) {
            currentArrow.transform.position = Vector2.MoveTowards(currentArrow.transform.position, target.transform.position + offset, 5 * Time.deltaTime);
            await Awaitable.NextFrameAsync();
        }
        audioSource.PlayOneShot(arrowHitSound);
        currentArrow.transform.position = new Vector3(15.0f, 0.0f, 0.0f);
        Destroy(currentArrow);
        target.TakeDamage(damage);
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
