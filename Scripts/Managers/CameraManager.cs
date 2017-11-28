using System;
using System.Collections.Generic;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    internal class CameraManager : MonoBehaviour
    {
        private float MovementSpeed = 400;
        private Vector3 MoveToPos;

        private void Start()
        {
            MoveToPos = transform.position;
        }

        void FixedUpdate()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");

            Boundary yBound = new Boundary(40, 51200);
            Boundary zBound = new Boundary(-102400, 51200);
            Boundary xBound = new Boundary(-51200, 51200);

            if (Math.Abs(scroll) > 0.001)
            {
                var scrollSpeed = (MoveToPos.y / yBound.Max) * 5120;

                if (scroll > 0)
                {
                    MoveToPos.y -= scrollSpeed * 2;
                    MoveToPos.z += scrollSpeed * 1.5f;
                }
                else
                {
                    MoveToPos.y += scrollSpeed * 2;
                    MoveToPos.z -= scrollSpeed * 1.5f;
                }
            }

            float vertical = Input.GetAxis("Vertical") * MovementSpeed;
            float horizontal = Input.GetAxis("Horizontal") * MovementSpeed;

            MoveToPos = new Vector3(MoveToPos.x + horizontal, MoveToPos.y, MoveToPos.z + vertical);

            // Clamp the camera
            MoveToPos = new Vector3(xBound.Clamp(MoveToPos.x), yBound.Clamp(MoveToPos.y), zBound.Clamp(MoveToPos.z));

            // Lerp it to new position
            transform.localPosition = Vector3.Lerp(transform.localPosition, MoveToPos, Time.deltaTime*20);
        }
    }
}
