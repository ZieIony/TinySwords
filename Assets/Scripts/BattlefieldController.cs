using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public partial class BattlefieldController : MonoBehaviour {
    private class AcceptableMovement {
        public List<Vector2Int> tilesInRange;
        public List<Vector2Int> path;
    }

    private class TilePositionWithCost {
        public Vector2Int tilePos;
        public int cost;

        public TilePositionWithCost(Vector2Int tilePos, int cost) {
            this.tilePos = tilePos;
            this.cost = cost;
        }
    }

    private class MapTileComparer : IComparer<TilePositionWithCost> {
        int IComparer<TilePositionWithCost>.Compare(TilePositionWithCost x, TilePositionWithCost y) {
            return x.cost - y.cost;
        }
    }

    public Tilemap[] tileMaps;
    private Vector3Int size;
    private Vector3Int origin;
    private BattlefieldTile[] navMap;

    public SheepController[] sheep;
    private System.Random random = new();

    public GameObject warriorPrefab, archerPrefab, monkPrefab;
    public GameObject[] spawnPoints;

    private Army blueArmy, redArmy, currentArmy;
    private ArmyTurn turn = ArmyTurn.Attacking;
    private uint round = 1;

    public CurrentUnitHighlight currentUnitHighlight;
    public TileSelector actionSelector;

    private List<TileSelector> actionSelectors = new();
    private List<TileSelector> movementSelectors = new();

    public GameUI gameUI;

    private UnitController currentUnit {
        get => currentArmy.currentUnit;
    }

    private bool animationInProgress = false;

    void Start() {
        BuildNavMap();
        foreach (SheepController sheep in sheep) {
            var sheepTile = GetTileAt(sheep.tilePosition);
            sheepTile.walkable = false;
        }

        blueArmy = new Army(ArmyColor.Blue);
        redArmy = new Army(ArmyColor.Red);
        currentArmy = blueArmy;
        List<GameObject> unitsToSpawn = new List<GameObject> {
            warriorPrefab,
            archerPrefab,
            monkPrefab,
            warriorPrefab,
            archerPrefab
        };
        foreach (GameObject spawnPoint in spawnPoints) {
            if (spawnPoint.transform.position.x >= 0) {
                SpawnUnit(redArmy, unitsToSpawn[redArmy.units.Count], spawnPoint.transform.position, UnitDirection.FacingLeft);
            } else {
                SpawnUnit(blueArmy, unitsToSpawn[blueArmy.units.Count], spawnPoint.transform.position, UnitDirection.FacingRight);
            }
        }
        currentUnitHighlight.currentUnit = currentArmy.currentUnit;
        gameUI.redUnit = null;
        gameUI.blueUnit = currentUnit;
        gameUI.skipButtonSide = ArmyColor.Blue;
        gameUI.round = round;
    }

    private void SpawnUnit(Army army, GameObject prefab, Vector3 position, UnitDirection direction) {
        var tilePos = GetTilePosAt(position);
        var unit = Instantiate(prefab, position, Quaternion.identity);
        if (direction == UnitDirection.FacingLeft) {
            unit.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }
        var unitController = unit.GetComponent<UnitController>();
        GetTileAt(tilePos).unit = unitController;
        unitController.armyColor = army.armyColor;
        army.units.Add(unit.GetComponent<UnitController>());
    }

    private void BuildNavMap() {
        size = tileMaps[0].size;
        origin = tileMaps[0].origin;

        foreach (Tilemap tileMap in tileMaps) {
            size = Vector3Int.Max(size, tileMap.size);
            origin = Vector3Int.Min(origin, tileMap.origin);
        }

        navMap = new BattlefieldTile[size.x * size.y];
        for (int y = origin.y; y < origin.y + size.y; y++) {
            for (int x = origin.x; x < origin.x + size.x; x++) {
                bool walkable = true;
                foreach (Tilemap tileMap in tileMaps) {
                    var pos = new Vector3Int(x, y, 0);
                    if (!tileMap.HasTile(pos))
                        continue;
                    Tile tile = tileMap.GetTile<Tile>(pos);
                    walkable = tile.colliderType == Tile.ColliderType.None;
                }
                SetTileAt(new Vector2Int(x, y), new BattlefieldTile(walkable, null));
            }
        }
    }

    void Update() {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);

        bool hasTile = HasTileAt(worldPoint);
        if (hasTile&&!animationInProgress) {
            var tilePosition = GetTilePosAt(currentUnit.transform.position);
            Vector2Int goalTilePos = GetTilePosAt(worldPoint);
            var goalTile = GetTileAt(goalTilePos);
            var movement = GetMovement(tilePosition, goalTilePos, (uint)currentUnit.moveSpeed);

            for(int i=0;i<movement.tilesInRange.Count;i++) {
                var tilePos = movement.tilesInRange[i];
                if (movementSelectors.Count <= i) {
                    movementSelectors.Add(Instantiate(actionSelector));
                }
                movementSelectors[i].SetAction(ActionType.Move);
                movementSelectors[i].position = tilePos;
            }
            for (int i = movement.tilesInRange.Count; i < movementSelectors.Count; i++)
                movementSelectors[i].SetAction(ActionType.None);
            int u = 0;
            foreach (UnitController unit in blueArmy.units) {
                if (currentUnit.CanPerformAction(unit)) {
                    if (actionSelectors.Count <= u) {
                        actionSelectors.Add(Instantiate(actionSelector));
                    }
                    actionSelectors[u].position = unit.tilePosition;
                    actionSelectors[u].SetAction(currentUnit.GetActionType(unit));
                    u++;
                }
            }
            foreach (UnitController unit in redArmy.units) {
                if (currentUnit.CanPerformAction(unit)) {
                    if (actionSelectors.Count <= u) {
                        actionSelectors.Add(Instantiate(actionSelector));
                    }
                    actionSelectors[u].position = unit.tilePosition;
                    actionSelectors[u].SetAction(currentUnit.GetActionType(unit));
                    u++;
                }
            }
            for (int i = u; i < actionSelectors.Count; i++)
                actionSelectors[i].SetAction(ActionType.None);
            if (goalTile.unit != null) {
                if (goalTile.unit.armyColor != currentArmy.armyColor) {
                    if (currentArmy == blueArmy) {
                        gameUI.redUnit = goalTile.unit;
                    } else {
                        gameUI.blueUnit = goalTile.unit;
                    }
                } else {
                    if (currentArmy == blueArmy) {
                        gameUI.redUnit = null;
                    } else {
                        gameUI.blueUnit = null;
                    }
                }
            } else {
                if (currentArmy == blueArmy) {
                    gameUI.redUnit = null;
                } else {
                    gameUI.blueUnit = null;
                }
            }

            if (Mouse.current.leftButton.wasPressedThisFrame) {
                if (goalTile.unit != null) {
                    if (currentUnit.CanPerformAction(goalTile.unit)) {
                        animationInProgress = true;
                        Task.Run(async () => {
                            await Awaitable.MainThreadAsync();
                            currentUnitHighlight.currentUnit = null;
                            await currentUnit.PerformAction(goalTile.unit);
                            if (!goalTile.unit.isAlive)
                                goalTile.unit = null;
                            currentArmy.NextUnit();
                            await NextTurn();
                            currentUnitHighlight.currentUnit = currentUnit;
                            animationInProgress = false;
                        });
                        return;
                    }
                }
                if (IsTileWalkable(goalTilePos.x, goalTilePos.y) && movement.path != null) {
                    animationInProgress = true;
                    var tile = GetTileAt(tilePosition);
                    tile.unit = null;
                    Task.Run(async () => {
                        await Awaitable.MainThreadAsync();
                        currentUnitHighlight.currentUnit = null;
                        await currentUnit.Move(movement.path);
                        goalTile.unit = currentUnit;
                        currentArmy.NextUnit();
                        await NextTurn();
                        currentUnitHighlight.currentUnit = currentUnit;
                        animationInProgress = false;
                    });
                }
            }
        } else {
            for (int i = 0; i < movementSelectors.Count; i++)
                movementSelectors[i].SetAction(ActionType.None);
            for (int i = 0; i < actionSelectors.Count; i++)
                actionSelectors[i].SetAction(ActionType.None);
            if (currentArmy == blueArmy) {
                gameUI.redUnit = null;
            } else {
                gameUI.blueUnit = null;
            }
        }
    }

    private async Awaitable NextTurn() {
        if (turn == ArmyTurn.Attacking) {
            turn = ArmyTurn.Defending;
            currentArmy = redArmy;
            gameUI.skipButtonSide = ArmyColor.Red;
            gameUI.redUnit = currentUnit;
        } else {
            turn = ArmyTurn.Attacking;
            currentArmy = blueArmy;
            gameUI.skipButtonSide = ArmyColor.Blue;
            gameUI.blueUnit = currentUnit;
        }
        if (blueArmy.isFirstUnit && redArmy.isFirstUnit) {
            round++;
            gameUI.round = round;
        }
        if (!currentUnit.isAlive)
            currentArmy.NextUnit();

        var sheep = GetRandomSheep();
        var sheepTile = GetTileAt(sheep.tilePosition);
        var neighbors = GetWalkableNeighbors(sheep.tilePosition);
        if (neighbors.Count > 0) {
            var nextPos = neighbors[random.Next(neighbors.Count - 1)];
            var actions = Enum.GetValues(typeof(SheepActionType));
            SheepActionType action = (SheepActionType)random.Next(actions.Length);
            if (action == SheepActionType.Eat) {
                sheep.Eat();
            } else if (action == SheepActionType.Move) {
                if (nextPos != null) {
                    sheepTile.walkable = true;
                    await sheep.Move(nextPos);
                    GetTileAt(nextPos).walkable = false;
                }
            }

        } else {
            SheepActionType action = (SheepActionType)random.Next(1);
            if (action == SheepActionType.Eat)
                sheep.Eat();
        }
    }

    private SheepController GetRandomSheep() {
        return sheep[random.Next(sheep.Length)];
    }

    public bool IsTileWalkable(int x, int y) {
        BattlefieldTile tile = navMap[(y - origin.y) * size.x + x - origin.x];
        return tile.walkable && tile.unit == null;
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current) {
        List<Vector2Int> total_path = new List<Vector2Int> { current };
        while (cameFrom.Keys.Contains(current)) {
            current = cameFrom[current];
            total_path.Insert(0, current);
        }
        return total_path;
    }

    int GetEstimatedCost(Vector2Int start, Vector2Int goal) {
        return Math.Abs(goal.x - start.x) + Math.Abs(goal.y - start.y);
    }

    List<Vector2Int> GetWalkableNeighbors(Vector2Int current) {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (current.x > origin.x) {
            if (IsTileWalkable(current.x - 1, current.y))
                neighbors.Add(new Vector2Int(current.x - 1, current.y));
            if (current.y > origin.y && IsTileWalkable(current.x - 1, current.y - 1))
            {
                neighbors.Add(new Vector2Int(current.x - 1, current.y - 1));
            }
            if (current.y < origin.y + size.y - 1 && IsTileWalkable(current.x - 1, current.y + 1))
            {
                neighbors.Add(new Vector2Int(current.x - 1, current.y + 1));
            }
        }
        if (current.y > origin.y && IsTileWalkable(current.x, current.y - 1)) {
            neighbors.Add(new Vector2Int(current.x, current.y - 1));
        }
        if (current.y < origin.y + size.y - 1 && IsTileWalkable(current.x, current.y + 1)) {
            neighbors.Add(new Vector2Int(current.x, current.y + 1));
        }
        if (current.x < origin.x + size.x - 1) {
            if (IsTileWalkable(current.x + 1, current.y))
                neighbors.Add(new Vector2Int(current.x + 1, current.y));
            if (current.y > origin.y && IsTileWalkable(current.x + 1, current.y - 1))
            {
                neighbors.Add(new Vector2Int(current.x + 1, current.y - 1));
            }
            if (current.y < origin.y + size.y - 1 && IsTileWalkable(current.x + 1, current.y + 1))
            {
                neighbors.Add(new Vector2Int(current.x + 1, current.y + 1));
            }
        }
        return neighbors;
    }

    private AcceptableMovement GetMovement(Vector2Int start, Vector2Int goal, uint maxCost) {
        AcceptableMovement movement = new();

        List<Vector2Int> tilesAlreadyChecked = new List<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        Dictionary<Vector2Int, int> costFromStart = new Dictionary<Vector2Int, int>();
        costFromStart[start] = 0;

        var tilesToCheck = new List<Vector2Int>();
        tilesToCheck.Add(start);

        Vector2Int current;
        while (tilesToCheck.Count != 0) {
            current = tilesToCheck.First();

            tilesToCheck.Remove(tilesToCheck.First());
            tilesAlreadyChecked.Add(current);

            List<Vector2Int> neighbors = GetWalkableNeighbors(current);
            foreach (Vector2Int neighbor in neighbors) {
                if (tilesAlreadyChecked.Contains(neighbor))
                    continue;
                var costFromStartToNeighbor = costFromStart[current] + 1;
                if (costFromStart.Keys.Contains(neighbor) && costFromStartToNeighbor >= costFromStart[neighbor])
                    continue;
                cameFrom[neighbor] = current;
                costFromStart[neighbor] = costFromStartToNeighbor;
                tilesToCheck.Add(neighbor);
            }
        }

        movement.tilesInRange = tilesAlreadyChecked.Where((Vector2Int tilePos) => { return costFromStart[tilePos] <= maxCost; }).ToList();
        if (costFromStart.ContainsKey(goal) && costFromStart[goal] <= maxCost)
            movement.path = ReconstructPath(cameFrom, goal);
        return movement;
    }

    public List<Vector2Int> GetPath(Vector2Int start, Vector2Int goal) {
        List<Vector2Int> tilesAlreadyChecked = new List<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        Dictionary<Vector2Int, int> costFromStart = new Dictionary<Vector2Int, int>();
        costFromStart[start] = 0;

        var tilesToCheck = new List<TilePositionWithCost>();
        tilesToCheck.Add(new TilePositionWithCost(start, GetEstimatedCost(start, goal)));

        Vector2Int current;
        while (tilesToCheck.Count != 0) {
            tilesToCheck.Sort(new MapTileComparer());
            current = tilesToCheck.First().tilePos;
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            tilesToCheck.Remove(tilesToCheck.First());
            tilesAlreadyChecked.Add(current);

            List<Vector2Int> neighbors = GetWalkableNeighbors(current);
            foreach (Vector2Int neighbor in neighbors) {
                if (tilesAlreadyChecked.Contains(neighbor))
                    continue;
                var costFromStartToNeighbor = costFromStart[current] + 1;
                if (costFromStart.Keys.Contains(neighbor) && costFromStartToNeighbor >= costFromStart[neighbor])
                    continue;
                cameFrom[neighbor] = current;
                costFromStart[neighbor] = costFromStartToNeighbor;
                tilesToCheck.Add(new TilePositionWithCost(neighbor, costFromStartToNeighbor + GetEstimatedCost(neighbor, goal)));
            }
        }
        return null;
    }

    internal bool HasTileAt(Vector2 worldPoint) {
        var pos = tileMaps[0].WorldToCell(worldPoint);
        return pos.x >= origin.x && pos.y >= origin.y && pos.x < origin.x + size.x && pos.y < origin.y + size.y;
    }

    internal Vector2Int GetTilePosAt(Vector2 worldPoint) {
        return (Vector2Int)tileMaps[0].WorldToCell(worldPoint);
    }

    private BattlefieldTile GetTileAt(Vector2Int tilePos) {
        return navMap[(tilePos.y - origin.y) * size.x + tilePos.x - origin.x];
    }

    private void SetTileAt(Vector2Int tilePos, BattlefieldTile tile) {
        navMap[(tilePos.y - origin.y) * size.x + tilePos.x - origin.x] = tile;
    }

    internal void SkipUnit() {
        if (animationInProgress)
            return;
        animationInProgress = true;
        Task.Run(async () => {
            await Awaitable.MainThreadAsync();
            currentUnitHighlight.currentUnit = null;
            currentArmy.NextUnit();
            await NextTurn();
            currentUnitHighlight.currentUnit = currentUnit;
            animationInProgress = false;
        });
    }
}
