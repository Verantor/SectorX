//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;
using UnityEngine.UI;

namespace FlightControlerBus.HUD
{
    public class Hud : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private FlightControler mouseFlight = null;

        [Header("HUD Elements")]
        [SerializeField] private RectTransform boresight = null;
        [SerializeField] private RectTransform mousePos = null;
        [SerializeField] private Text TextKMH = null;

        private Camera playerCam = null;

        private void Awake()
        {
            if (mouseFlight == null)
                Debug.LogError(name + ": Hud - Mouse Flight Controller not assigned!");

            playerCam = mouseFlight.GetComponentInChildren<Camera>();

            if (playerCam == null)
                Debug.LogError(name + ": Hud - No camera found on assigned Mouse Flight Controller!");
        }

        private void Update()
        {
            if (mouseFlight == null || playerCam == null)
                return;

            UpdateGraphics(mouseFlight);
        }

        private void UpdateGraphics(FlightControler controller)
        {
            if (boresight != null)
            {
                boresight.position = playerCam.WorldToScreenPoint(controller.BoresightPos);
                boresight.gameObject.SetActive(boresight.position.z > 1f);
            }

            if (mousePos != null)
            {
                mousePos.position = playerCam.WorldToScreenPoint(controller.MouseAimPos);
                mousePos.gameObject.SetActive(mousePos.position.z > 1f);
            }
            if (TextKMH!= null)
            {
                float x = controller.aircraft.gameObject.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
                
                TextKMH.text = x.ToString();
            }
        }

        public void SetReferenceMouseFlight(FlightControler controller)
        {
            mouseFlight = controller;
        }
    }
}
