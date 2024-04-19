using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class EnemySettings : ScriptableObject
{
    public String enemyName;
    public int enemyMaxHp;
    public float secondsBetweenAttacks;
    public BulletPattern[] bulletPatterns;

}
