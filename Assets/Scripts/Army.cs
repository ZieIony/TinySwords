using System.Collections.Generic;
using UnityEngine;

public class Army
{
    public ArmyColor armyColor;

    public List<UnitController> units = new List<UnitController>();

    private uint _currentUnit = 0;
    public UnitController currentUnit {
        get {
            return units[(int)_currentUnit];
        }
    }

    public void NextUnit() {
        do {
            _currentUnit = (uint)((_currentUnit + 1) % units.Count);
        } while (!currentUnit.isAlive);
    }

    public bool isFirstUnit {
        get => _currentUnit == 0;
    }

    public Army(ArmyColor armyColor) {
        this.armyColor = armyColor;
    }
}
