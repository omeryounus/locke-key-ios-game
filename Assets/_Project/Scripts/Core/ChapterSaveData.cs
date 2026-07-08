using System;
using System.Collections.Generic;

[Serializable]
public class ChapterSaveData
{
    public int version = 1;
    public float playerX;
    public float playerY;
    public bool hasHouseKey;
    public bool hasGhostKey;
    public bool hasHeadKey;
    public string activeKeyAbility = string.Empty;
    public int currentBeat;
    public bool chapterComplete;
    public bool echoEncounterCleared;
    public List<string> solvedPuzzleIds = new();
    public List<string> collectedPickupIds = new();
}