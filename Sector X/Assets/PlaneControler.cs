using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    
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
        public float HowLongAreStanding = 0;

        public float speed;

        public ParticleSystem ParticleSystem1;
        public ParticleSystem ParticleSystem2;
        public ParticleSystem ParticleSystem3;
        public ParticleSystem ParticleSystem4;


        private void Awake()
            {
                rigid = GetComponent<Rigidbody>();
           
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
            HowLongAreStanding = 0;
            forceMult = 100;
            speed = 100;
                ParticleSystem1.startSpeed = 10f;
                ParticleSystem1.startSpeed = 10f;
                ParticleSystem3.startSpeed = 10f;
                ParticleSystem4.startSpeed = 10f;
            }
            else
            if (Input.GetKeyUp(KeyCode.Space))
            {
                thrust = 100f;
            speed = 50;
            ParticleSystem1.startSpeed = 7f;
                ParticleSystem1.startSpeed = 7f;
                ParticleSystem3.startSpeed = 7f;
                ParticleSystem4.startSpeed = 7f;
            }


            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
            speed = 0;
            thrust = 0;
                ParticleSystem1.startSpeed = 5f;
                ParticleSystem2.startSpeed = 5f;
                ParticleSystem3.startSpeed = 5f;
                ParticleSystem4.startSpeed = 5f;
            StartCoroutine(TurnSpeedOf());
            }
        else
            if (Input.GetKeyUp(KeyCode.LeftShift) && HowLongAreStanding != 4)
        {
            speed = 50;
            thrust = 100f;
            forceMult = 100;
            HowLongAreStanding = 0;
            ParticleSystem1.startSpeed = 7f;
            ParticleSystem2.startSpeed = 7f;
            ParticleSystem3.startSpeed = 7f;
            ParticleSystem4.startSpeed = 7f;

        }
        else
            if (Input.GetKey(KeyCode.LeftShift) && HowLongAreStanding == 4)
        {
            speed = 0;
            thrust = 0;
            forceMult = 0;
        }
      

    }
    IEnumerator TurnSpeedOf()
    {
        Debug.Log("SAS1");
        while (Input.GetKey(KeyCode.LeftShift) && HowLongAreStanding <=3)
        {
            Debug.Log("SAS");
            yield return new WaitForSeconds(1f);
            HowLongAreStanding++;
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
    

