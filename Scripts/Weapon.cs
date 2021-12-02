using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //All data needed to make a new weapon
    //  -1 ammo = infinite
    // Weapon Types:
    //      0 = Basic GUN
    //      1 = Basic MELEE
    //      2 = GUN but Box "shot" (Axe)
    public Sprite weaponSprite;
    public string weaponName;
    public int weaponCost, weaponType;
    public int damage;
    public float range, rangeY, reloadTime, spread;
    public bool fullAuto;
    public GameObject weaponEffect;
    public AudioClip weaponSound;
    public bool refundMiss;

    //IGNORE THESE
    
    
}
