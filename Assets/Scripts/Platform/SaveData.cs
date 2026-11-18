using System;

// The cloud save: the highest level won (1 = Level01). JsonUtility ignores fields it does not know, so older saves still load.
[Serializable]
public class SaveData
{
    public int highestLevelWon;
    public int credits;
}
