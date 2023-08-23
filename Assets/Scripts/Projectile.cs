using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile
{
    // prefabs du projectile
    private GameObject prefabs;



    // degats du projectile
    private int damage;
    // vitesse du projectile
    private int speed;
    // direction of the projectile
    Vector3 direction;
    // penetration le nombre d'enemi traversable avant destruction
    private int penetration;
    // duree de vie du projectile ( en millisecondes )
    private int timeLeft;



    public Projectile()
    {
    }


    public virtual void test()
    {
        Debug.Log("0");
    }



}
