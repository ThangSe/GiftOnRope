using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PendulumGiftRefsSO: ScriptableObject
{
    public Vector3 _defaultGiftPos = new Vector3(0, 3f, 0);
    public Vector3[] _defaultCirclePos;

    public float _maxAngleDeflection = 46f;
    public float _pendulumSpeedDefault = 3f;
    public float _noGravity = 0;
    public float _hasGravity = 1;
    public float _defaultForce = 4f;

    public float _countdownToStartTimerMax;

    public int _maxHeart = 3;

    public int _defaultPerfectCount = 0;
    public int _defaultCurrentScore;
    public int _baseIncScore = 5;
}
