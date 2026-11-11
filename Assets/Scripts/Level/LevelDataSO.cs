using System.Collections.Generic;
using UnityEngine;
using Game.Level;

[CreateAssetMenu(menuName = "KeroKero/LevelDataSO", fileName = "LevelDataSO")]
public class LevelDataSO : ScriptableObject
{
    [System.Serializable]
    public class LilyEntry
    {
        public Vector2 position;
        public LilyPadType lilyType;
        public LilyPadDirection lilyDirection;
        public float breakTime;
    }

    public List<LilyEntry> lilies = new List<LilyEntry>();
}