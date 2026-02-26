using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour {
    private UIDocument document;

    private VisualElement bannerBlue, bannerRed;
    private Image avatarBlue, avatarRed;
    private Button skipBlueButton, skipRedButton;
    private Label classBlue, classRed, hpBlue, hpRed, damageBlue, damageRed, rangeBlue, rangeRed, speedBlue, speedRed;

    private Label roundLabel;

    public UnitController blueUnit {
        set {
            if (value) {
                bannerBlue.visible = true;
                avatarBlue.sprite = value.avatarBlue;
                classBlue.text = value.className;
                hpBlue.text = "" + value.currentHp + "/" + value.maxHp;
                rangeBlue.text = "" + value.attackRange;
                speedBlue.text = "" + value.moveSpeed;
                damageBlue.text = "" + value.damage;
            } else {
                bannerBlue.visible = false;
            }
        }
    }

    public UnitController redUnit {
        set {
            if (value) {
                bannerRed.visible = true;
                avatarRed.sprite = value.avatarRed;
                classRed.text = value.className;
                hpRed.text = "" + value.currentHp + "/" + value.maxHp;
                rangeRed.text = "" + value.attackRange;
                speedRed.text = "" + value.moveSpeed;
                damageRed.text = "" + value.damage;
            } else {
                bannerRed.visible = false;
            }
        }
    }

    public uint round {
        set {
            roundLabel.text = "" + value;
        }
    }

    public ArmyColor skipButtonSide {
        set {
            if (value == ArmyColor.Red) {
                skipRedButton.visible = true;
                skipBlueButton.visible = false;
            } else {
                skipRedButton.visible = false;
                skipBlueButton.visible = true;
            }
        }
    }

    public BattlefieldController mapController;

    private void Awake() {
        document = GetComponent<UIDocument>();

        bannerBlue = document.rootVisualElement.Q("bannerBlue");
        avatarBlue = document.rootVisualElement.Q<Image>("avatarBlue");
        classBlue = document.rootVisualElement.Q<Label>("classBlue");
        hpBlue = document.rootVisualElement.Q<Label>("hpBlue");
        damageBlue = document.rootVisualElement.Q<Label>("damageBlue");
        rangeBlue = document.rootVisualElement.Q<Label>("rangeBlue");
        speedBlue = document.rootVisualElement.Q<Label>("speedBlue");

        skipBlueButton = document.rootVisualElement.Q<Button>("skipBlue");
        skipBlueButton.RegisterCallback<ClickEvent>(OnSkipClicked);

        bannerRed = document.rootVisualElement.Q("bannerRed");
        avatarRed = document.rootVisualElement.Q<Image>("avatarRed");
        classRed = document.rootVisualElement.Q<Label>("classRed");
        hpRed = document.rootVisualElement.Q<Label>("hpRed");
        damageRed = document.rootVisualElement.Q<Label>("damageRed");
        rangeRed = document.rootVisualElement.Q<Label>("rangeRed");
        speedRed = document.rootVisualElement.Q<Label>("speedRed");

        skipRedButton = document.rootVisualElement.Q<Button>("skipRed");
        skipRedButton.RegisterCallback<ClickEvent>(OnSkipClicked);

        roundLabel = document.rootVisualElement.Q<Label>("round");
    }

    private void OnDisable() {
        skipBlueButton.UnregisterCallback<ClickEvent>(OnSkipClicked);
        skipRedButton.UnregisterCallback<ClickEvent>(OnSkipClicked);
    }

    private void OnSkipClicked(ClickEvent eventArgs) {
        mapController.SkipUnit();
    }
}