using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalConfigs 
{
    public static int CornerPerLevel = 6;
    // Increase corner count each level finished
    public static int CornerIncreasePerLevel = 1;

    public static float CarStartSpeed = 25f;
    public static float CarSpeedIncreasePerLevel = 5f;

    public static float DriftSpeedAngular = 30f;
    public static float DriftLimitAngle = 30f;

    public static float LevelFinishAnimationTimeInSec = 1.5f;
}
