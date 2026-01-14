using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityStandardAssets.Vehicles.Ball
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] private float m_MovePower = 5; // The force added to the ball to move it.
        [SerializeField] private bool m_UseTorque = true; // Whether or not to use torque to move the ball.
        [SerializeField] private float m_MaxAngularVelocity = 25; // The maximum velocity the ball can rotate at.
        [SerializeField] public float m_JumpPower = 2; // The force added to the ball when it jumps.
        [SerializeField] private float reverseForceMultiplier = 2.0f; // Multiplies force when moving against current velocity


        private const float k_GroundRayLength = 1f; // The length of the ray to check if the ball is grounded.
        private Rigidbody m_Rigidbody;
        

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            // Set the maximum angular velocity.
            GetComponent<Rigidbody>().maxAngularVelocity = m_MaxAngularVelocity;
        }


        public void Move(Vector3 moveDirection, bool jump)
        {
            Vector3 currentVelocity = m_Rigidbody.velocity;
            Vector3 desiredForceDir = m_UseTorque 
                ? new Vector3(moveDirection.z, 0, -moveDirection.x) 
                : moveDirection;

            // Normalize direction for comparison
            Vector3 velocityDir = currentVelocity.normalized;
            Vector3 forceDir = desiredForceDir.normalized;

            float alignment = Vector3.Dot(forceDir, velocityDir);

            // If applying force opposite to current movement
            if (alignment < 0)
            {
                desiredForceDir *= reverseForceMultiplier;
            }

            if (m_UseTorque)
            {
                m_Rigidbody.AddTorque(desiredForceDir * m_MovePower);
            }
            else
            {
                m_Rigidbody.AddForce(desiredForceDir * m_MovePower);
            }

            // Jump
            if (Physics.Raycast(transform.position, -Vector3.up, k_GroundRayLength) && jump)
            {
                m_Rigidbody.AddForce(Vector3.up * m_JumpPower, ForceMode.Impulse);
            }
        }


        public void Update() {
            // if player falls below y = -5 reload the scene
            if(transform.position.y < -5.0) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
