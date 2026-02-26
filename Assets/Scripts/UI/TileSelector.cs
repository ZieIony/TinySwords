using UnityEngine;

public class TileSelector : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public Color moveColor, attackColor, healColor;

    public Vector2Int position {
        set {
            gameObject.transform.position = new Vector3(value.x + 0.5f, value.y+0.5f, 0);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetAction(ActionType actionType) {
        if (actionType == ActionType.Attack) {
            spriteRenderer.enabled = true;
            spriteRenderer.color = attackColor;
        } else if (actionType == ActionType.Heal) {
            spriteRenderer.enabled = true;
            spriteRenderer.color = healColor;
        } else if (actionType == ActionType.Move) {
            spriteRenderer.enabled = true;
            spriteRenderer.color = moveColor;
        } else {
            spriteRenderer.enabled = false;
        }
    }
}
