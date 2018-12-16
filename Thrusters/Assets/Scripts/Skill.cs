using UnityEngine;

[System.Serializable]
public class Skill
{
    public string Name = "Orb";

    public int Damage = 40;

    //public float Range = 200f;

    public float Velocity = 200f;

    public float Force = 200f;

    public float FireRate = 0f;

    public float DestroyAfterSeconds = 3f;
    
    public int Charges = 3;

    public float Slowdown = 0f;

    public float CastTime = 0;

    public float CooldownTime = 1f;
    public Color Color;

    [HideInInspector] public bool IsOnCooldown;

    [HideInInspector] public float CurrentCooldownTime;

    public GameObject Prefab;

    public GameObject Gui;

}
