using UnityEngine;

public class SheepController : MonoBehaviour {
    protected Animator animator;

    internal Vector2Int tilePosition;

    public float moveSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake() {
        animator = GetComponent<Animator>();

        tilePosition = new Vector2Int(Mathf.RoundToInt(gameObject.transform.position.x - 0.5f), Mathf.RoundToInt(gameObject.transform.position.y - 0.5f));
        transform.position = new Vector3(tilePosition.x + 0.5f, tilePosition.y + 0.5f, 0);
    }

    public void Eat() {
        animator.Play("eat");
    }

    public async Awaitable Move(Vector2Int nextPos) {
        animator.Play("move");
        float EPSILON = 0.01f;
        var targetSpritePos = new Vector2(nextPos.x + 0.5f, nextPos.y + 0.5f);
        while (Vector2.Distance(transform.position, targetSpritePos) > EPSILON) {
            transform.position = Vector2.MoveTowards(transform.position, targetSpritePos, moveSpeed * Time.deltaTime);
            await Awaitable.NextFrameAsync();
        }
        tilePosition = nextPos;
        animator.Play("idle");
    }
}
