using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerWeapon
{
    public string name = "Glock";

    public int damage = 40;

    public float range = 200f;

    public float fireRate = 0f;

    public int maxBullets = 3;
    [HideInInspector] public int bullets;

    public float reloadTime = 1f;

    public GameObject graphics;

    [HideInInspector] public Transform firePoint;

    public PlayerWeapon()
    {
        bullets = maxBullets;
    }
}
