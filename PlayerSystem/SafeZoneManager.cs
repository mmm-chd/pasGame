using UnityEngine;
using System.Collections;

public class SafeZoneManager : MonoBehaviour
{
    public static SafeZoneManager Instance;

    [Header("Safe Zone Duration")]
    public float safeZoneDuration = 2f;

    private bool isSafe = false;

    void Awake()
    {
        Instance = this;
    }

    public bool IsPlayerSafe()
    {
        return isSafe;
    }

    public void ActivateSafeZone()
    {
        StopAllCoroutines();
        StartCoroutine(SafeZoneRoutine());
    }

    private IEnumerator SafeZoneRoutine()
    {
        isSafe = true;
        Debug.Log("🛡️ Player is SAFE (cannot be attacked)");

        yield return new WaitForSeconds(safeZoneDuration);

        isSafe = false;
        Debug.Log("⚠️ SafeZone ended — player can be attacked again");
    }
}
