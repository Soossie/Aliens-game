using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public sealed class LevelConfig : ScriptableObject
{
    [SerializeField] 
    public List<LevelData> levels = new(); 
}

[Serializable]
public class LevelData
{
    public string levelName;
    public bool isCompleted;
    public bool perfectScore;
    public bool firstTimeInLevel;
    
    public Sprite levelSprite;
    public Sprite bitmapSprite;
    public AudioClip levelMusic;
    
    public int requiredScore;
    public Vector3 spawnPoint;
    public Vector3 goalPoint;
    public int lemmingsAmount;
    public string[] unlocks;
    
}