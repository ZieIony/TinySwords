using UnityEngine;

public class CurrentUnitHighlight : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public UnitController currentUnit {
        set {
            if (value == null) {
                spriteRenderer.enabled = false;
            } else {
                spriteRenderer.enabled = true;
                gameObject.transform.position = value.transform.position;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        float scale = Mathf.Sin(Time.timeSinceLevelLoad * 5) * 0.1f + 0.8f;
        transform.localScale = new Vector3(scale, scale * 0.6f, 1);
    }
}
