using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlightControlerBus.Plane 
{
    
        [RequireComponent(typeof(Rigidbody))]
        public class PlaneControler : MonoBehaviour
        {
            public FlightControler controller = null;

            public float thrust = 100f;
            public Vector3 turnTorque = new Vector3(90f, 25f, 45f);
            public float forceMult = 1000f;

            public float sensitivity = 5f;
            public float aggressiveTurnAngle = 10f;


            [Range(-1f, 1f)] public float pitch = 0f;
            [Range(-1f, 1f)] public float yaw = 0f;
            [Range(-1f, 1f)] public float roll = 0f;

            public float Pitch { set { pitch = Mathf.Clamp(value, -1f, 1f); } get { return pitch; } }
            public float Yaw { set { yaw = Mathf.Clamp(value, -1f, 1f); } get { return yaw; } }
            public float Roll { set { roll = Mathf.Clamp(value, -1f, 1f); } get { return roll; } }

            private Rigidbody rigid;

            private bool rollOverride = false;
            private bool pitchOverride = false;


        public ParticleSystem ParticleSystem1;
        public ParticleSystem ParticleSystem2;
        ParticleSystem.EmissionModule emissionModule1;
        ParticleSystem.EmissionModule emissionModule2;

        private void Awake()
            {
                rigid = GetComponent<Rigidbody>();
            emissionModule1 = ParticleSystem1.emission;
            emissionModule2 = ParticleSystem2.emission;
            if (controller == null)
                    Debug.LogError(name + ": Plane - Missing reference to MouseFlightController!");
            }

            private void Update()
            {

                rollOverride = false;
                pitchOverride = false;

                float keyboardRoll = Input.GetAxis("Horizontal");
                if (Mathf.Abs(keyboardRoll) > .25f)
                {
                    rollOverride = true;
                }

                float keyboardPitch = Input.GetAxis("Vertical");
                if (Mathf.Abs(keyboardPitch) > .25f)
                {
                    pitchOverride = true;
                    rollOverride = true;
                }

                // Calculate the autopilot stick inputs.
                float autoYaw = 0f;
                float autoPitch = 0f;
                float autoRoll = 0f;
                if (controller != null)
                    RunAutopilot(controller.MouseAimPos, out autoYaw, out autoPitch, out autoRoll);

                // Use either keyboard or autopilot input.
                yaw = autoYaw;
                pitch = (pitchOverride) ? keyboardPitch : autoPitch;
                roll = (rollOverride) ? keyboardRoll : autoRoll;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                thrust = 500f;
                emissionModule1.rateOverTime = 12;
                emissionModule2.rateOverTime = 12;
            }
            else
            if (Input.GetKeyUp(KeyCode.Space))
            {
                thrust = 100f;
                emissionModule1.rateOverTime = 6;
                emissionModule2.rateOverTime = 6;
            }


            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                thrust = 0;
                emissionModule1.rateOverTime = 3;
                emissionModule2.rateOverTime = 3;
              
            }
            else
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                thrust = 100f;
                emissionModule1.rateOverTime = 6;
                emissionModule2.rateOverTime = 6;
      
            }

        }

            private void RunAutopilot(Vector3 flyTarget, out float yaw, out float pitch, out float roll)
            {
                // This is my usual trick of converting the fly to position to local space.
                // You can derive a lot of information from where the target is relative to self.
                var localFlyTarget = transform.InverseTransformPoint(flyTarget).normalized * sensitivity;
                var angleOffTarget = Vector3.Angle(transform.forward, flyTarget - transform.position);

                // IMPORTANT!
                // These inputs are created proportionally. This means it can be prone to
                // overshooting. The physics in this example are tweaked so that it's not a big
                // issue, but in something with different or more realistic physics this might
                // not be the case. Use of a PID controller for each axis is highly recommended.

                // ====================
                // PITCH AND YAW
                // ====================

                // Yaw/Pitch into the target so as to put it directly in front of the aircraft.
                // A target is directly in front the aircraft if the relative X and Y are both
                // zero. Note this does not handle for the case where the target is directly behind.
                yaw = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
                pitch = -Mathf.Clamp(localFlyTarget.y, -1f, 1f);

                // ====================
                // ROLL
                // ====================

                // Roll is a little special because there are two different roll commands depending
                // on the situation. When the target is off axis, then the plane should roll into it.
                // When the target is directly in front, the plane should fly wings level.

                // An "aggressive roll" is input such that the aircraft rolls into the target so
                // that pitching up (handled above) will put the nose onto the target. This is
                // done by rolling such that the X component of the target's position is zeroed.
                var agressiveRoll = Mathf.Clamp(localFlyTarget.x, -1f, 1f);

                // A "wings level roll" is a roll commands the aircraft to fly wings level.
                // This can be done by zeroing out the Y component of the aircraft's right.
                var wingsLevelRoll = transform.right.y;

                // Blend between auto level and banking into the target.
                var wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);
                roll = Mathf.Lerp(wingsLevelRoll, agressiveRoll, wingsLevelInfluence);
            }

            private void FixedUpdate()
            {
                // Ultra simple flight where the plane just gets pushed forward and manipulated
                // with torques to turn.
                rigid.AddRelativeForce(Vector3.forward * thrust * forceMult, ForceMode.Force);
                rigid.AddRelativeTorque(new Vector3(turnTorque.x * pitch,
                                                    turnTorque.y * yaw,
                                                    -turnTorque.z * roll) * forceMult,
                                        ForceMode.Force);
            }
        }
    

}
