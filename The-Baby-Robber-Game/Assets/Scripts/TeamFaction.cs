using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamFaction : MonoBehaviour
{
    public Team team;
    [HideInInspector]
    public Character humanoidCharacter;

    [SerializeField]
    public Faction faction;

    [System.Serializable]
    public enum Faction
    {
        Blue, Yellow, Red, Green, Black
    }

    [SerializeField]
    public UnitType unitType;

    [System.Serializable]
    public enum UnitType
    {
        Humanoid, AirCraft
    }

    private void Awake()
    {
        humanoidCharacter = GetComponent<Character>();
    }
}
