using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    public AnimationCurve recoilCurve;

    private WeaponManager weaponManager;
    private Transform mainCam;
    public Vector3 basePos = Vector3.zero;
    public Vector3 baseRot = Vector3.zero;

    private Vector3 offsetRot;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    public void SetUp(WeaponManager weapMan)
    {
        weaponManager = weapMan;
    }

    public Transform GetCameraTransform
    {
        set
        {
            mainCam = value;
        }
    }


    public void RecoilShake(float time)
    {
        offsetRot = Vector3.right * weaponManager.currentWepPro.weapSetting.recoilAmount * recoilCurve.Evaluate(time);
        mainCam.localEulerAngles = baseRot + offsetRot;
    }

    public void HitShake(float duration, float magnitude)
    {
        StartCoroutine(DamageShake(duration, magnitude));
    }

    private IEnumerator DamageShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.6f, 0.6f) * magnitude;
            float y = Random.Range(-0.5f, 0.5f) * magnitude;

            Vector3 newPos = new Vector3(x, y, basePos.z);
            mainCam.localPosition = Vector3.Lerp(mainCam.localPosition, newPos, Time.deltaTime * 3);

            elapsed += Time.fixedDeltaTime;

            yield return null;
        }

        mainCam.localPosition = basePos;
    }

}
