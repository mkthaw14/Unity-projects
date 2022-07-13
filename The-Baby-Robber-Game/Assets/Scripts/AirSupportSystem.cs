using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSupportSystem : MonoBehaviour
{
	public void ComfirmAirStrike(Transform target, Transform StrikePoint)
    {
        Transform airStrike = Instantiate(StrikePoint, target.position, Quaternion.Euler(0, GetRandomRotation(), 0));
        Helicopter[] h = airStrike.GetComponentsInChildren<Helicopter>();

        for (int x = 0; x < h.Length; x++)
            h[x].Striketarget = target;

        Destroy(airStrike.gameObject, 30f);
    }

    float GetRandomRotation()
    {
        float val = 0;
        int randomNum = Random.Range(0, 4);

        switch (randomNum)
        {
            case 0:
                val = 0;
                break;
            case 1:
                val = 90;
                break;
            case 2:
                val = 180;
                break;
            case 3:
                val = 360;
                break;
        }

        return val;
    }
}
