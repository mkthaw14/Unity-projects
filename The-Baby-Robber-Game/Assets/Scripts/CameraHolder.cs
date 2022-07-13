using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MrThaw
{
    public class CameraHolder : MonoBehaviour
    {
        public Player target;
        public bool lockCursor;

        private InputHandling inp;
        private Transform pivot;
        private Transform camT;
        private PostProcessVolume postProcessVolume;
        [SerializeField]
        private PostProcessProfile[] postProcessProfiles;
        public CameraSetting camsetting;

        public void SetUp(Player player, InputHandling inputH, CameraSetting _camsetting, int sceneIndex)
        {
            target = player;
            inp = inputH;

            pivot = transform.GetChild(0);
            camT = pivot.GetChild(0);
            postProcessVolume = GetComponentInChildren<PostProcessVolume>();
            postProcessVolume.profile = postProcessProfiles[sceneIndex - 1];

            if(QualitySettings.GetQualityLevel() >= 3)
            {
                postProcessVolume.weight = 1;
            }
            else
            {
                postProcessVolume.weight = 0;
            }

            camsetting = _camsetting;
        }

        public void Tick()
        {
            HandlePosition();
        }

        public void LateTick()
        {
            if(target.m_Transform)
                Follow(target.m_Transform.position, camsetting.moveSpeed, Time.deltaTime);

            HandleRotation();
        }

        void HandleRotation()
        {
            float x = inp.inputX * Mathf.Abs(inp.inputX);// Multiply with its absolute value to get smoothing
            float y = inp.inputY * Mathf.Abs(inp.inputY);

            camsetting.smoothX = x;
            camsetting.smoothY = y;

            camsetting.smoothX = Mathf.Clamp(camsetting.smoothX, -1.5f, 1.5f);// Clamping the acceleration to limit
            camsetting.smoothY = Mathf.Clamp(camsetting.smoothY, -1.5f, 1.5f);

            if (target.character.DebugAim)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                pivot.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {

                if (GameManager.instance.isGameFreeze()) 
                { 
                    camsetting.lookAngle += 0.3f * 3; 
                    camsetting.tiltAngle = 0; 
                }
                else
                {
                    camsetting.lookAngle += camsetting.smoothX * camsetting.x_rotateSpeed;
                    camsetting.tiltAngle -= camsetting.smoothY * camsetting.y_rotateSpeed;
                    camsetting.tiltAngle = Mathf.Clamp(camsetting.tiltAngle, camsetting.minAngle, camsetting.maxAngle);
                }

                if (camsetting.invertX)
                    transform.rotation = Quaternion.Euler(0, -camsetting.lookAngle, 0);
                else
                    transform.rotation = Quaternion.Euler(0, camsetting.lookAngle, 0);

                if (camsetting.invertY)
                    pivot.localRotation = Quaternion.Euler(-camsetting.tiltAngle, 0, 0);
                else
                    pivot.localRotation = Quaternion.Euler(camsetting.tiltAngle, 0, 0);
            }
        }

        void HandlePosition()
        {
            float targetX = camsetting.PositionNormalX;
            float targetY = camsetting.PositionNormalY;
            float targetZ = camsetting.PositionNormalZ;

            bool isAiming = target.character.aim;
            bool isCrouching = target.character.isCrouching;

            if (isAiming)
            {
                targetX = camsetting.PositionAimingX;
                targetZ = camsetting.PositionAimingZ;
            }

            else
            {
                targetX = camsetting.PositionNormalX;
                targetZ = camsetting.PositionNormalZ;
            }
				
            if (isCrouching)
                targetY = targetY - 0.45f;

            Vector3 newPivotPos = pivot.localPosition;

            newPivotPos.x = targetX;
            newPivotPos.y = targetY;

            Vector3 newCamPos = camT.localPosition;

            float t = Time.deltaTime * camsetting.adaptSpeed;
            float actualZ = targetZ;

            CheckCollision(ref actualZ, ref newCamPos.z);

            Vector3 lerpPivotPos = Vector3.Lerp(pivot.localPosition, newPivotPos, t);
            Vector3 lerpCamPos = Vector3.Lerp(camT.localPosition, newCamPos, t);

            pivot.localPosition = lerpPivotPos;
            camT.localPosition = lerpCamPos;
        }

        void Follow(Vector3 T, float speed, float d)
        {
            Vector3 Target = Vector3.Lerp(transform.position, T, speed * d);
            transform.position = Target;
        }

        public LayerMask otherLayer;

        void CheckCollision(ref float actualZ, ref float camZ)
        {
            RaycastHit hit;
            Vector3 origin; //target.transform.position; // value changed from pivot position 
            origin = pivot.position;
            Vector3 dir = camT.position - origin;

            float maxDist = Mathf.Abs(actualZ);
            Debug.DrawRay(origin, dir * maxDist, Color.red);

            if (Physics.Raycast(origin, dir, out hit, maxDist, otherLayer))
            {
                float dist = Vector3.Distance(hit.point, origin);
                actualZ = -(dist / 2);
                camZ = actualZ;
            }
            else
            {
                camZ = actualZ;
            }
        }

    }

  
}
