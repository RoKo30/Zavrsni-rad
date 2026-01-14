using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class SmoothFollowWithATwist : MonoBehaviour
    {
        public float delayTime = 0.0f;
        private float targetTime;
        [SerializeField]
        private Transform target;
        [SerializeField]
        private float distance = 10.0f;
        [SerializeField]
        private float height = 5.0f;
        [SerializeField]
        private float rotationDamping = 3.0f;
        [SerializeField]
        private float heightDamping = 2.0f;
        [SerializeField]
        private float mouseSensitivity = 5.0f;

        private float currentRotationAngle;
        private float mouseX;
        
        void Start()
        {
            targetTime = Time.time + delayTime;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            StartCoroutine(getPlayer());
        }
        IEnumerator getPlayer()
        {
            yield return new WaitForSeconds(0.2f);
            target = GameObject.FindWithTag("Player").transform;
        }
        void LateUpdate()
        {
            if (Time.time < targetTime || !target)
                return;

            float mouseDeltaX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseX += mouseDeltaX;

            currentRotationAngle = Mathf.LerpAngle(transform.eulerAngles.y, mouseX, rotationDamping * Time.deltaTime);
            float currentHeight = target.position.y + height; // ← always fixed height above target

            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
            Vector3 newPosition = target.position - currentRotation * Vector3.forward * distance;
            newPosition.y = currentHeight;

            transform.position = newPosition;
            transform.LookAt(target);
        }


    }
}
