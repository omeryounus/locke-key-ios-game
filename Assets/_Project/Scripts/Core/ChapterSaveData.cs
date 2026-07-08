using System;
using System.Collections.Generic;

[Serializable]
public class ChapterSaveData
{
    public int version = 2;
    public float playerX;
    public float playerY;
    public bool hasSavedPosition;
    public float checkpointX;
    public float checkpointY;
    public bool hasHouseKey;
    public bool hasGhostKey;
    public bool hasHeadKey;
    public bool ghostKeyRevealed;
    public string activeKeyAbility = string.Empty;
    public int currentBeat;
    public int currentRoom;
    public bool chapterComplete;
    public bool echoEncounterCleared;
    public bool echoEncounterActive;
    public List<string> solvedPuzzleIds = new();
    public List<string> collectedPickupIds = new();
}