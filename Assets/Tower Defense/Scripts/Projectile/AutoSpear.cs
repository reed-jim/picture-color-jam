using System.Collections;
using UnityEngine;

public class AutoSpear : MonoBehaviour
{
    public Vector3 rotateSpeed;

    void Update()
    {
        transform.eulerAngles += rotateSpeed;
    }

    private IEnumerator AttackPeriodically()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        while (true)
        {
            yield return waitForSeconds;
        }
    }
}
