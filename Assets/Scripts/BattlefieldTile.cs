public class BattlefieldTile {
    public bool walkable;
    public UnitController unit;

    public BattlefieldTile(bool walkable, UnitController unit) {
        this.walkable = walkable;
        this.unit = unit;
    }
}
