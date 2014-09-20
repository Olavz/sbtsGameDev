/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;
using TETCSharpClient;
using TETCSharpClient.Data;
using Assets.Scripts;

/// <summary>
/// Component attached to 'Main Camera' of '/Scenes/std_scene.unity'.
/// This script handles the navigation of the 'Main Camera' according to 
/// the GazeData stream recieved by the EyeTribe Server.
/// </summary>
public class GazeCamera : MonoBehaviour, IGazeListener
{
    private Camera cam;
    public GameObject go;

    private double eyesDistance;
    private double depthMod;

    private Component gazeIndicator;

    private Collider currentHit;

    private GazeDataValidator gazeUtils;

    void Start()
    {
        //Stay in landscape
        Screen.autorotateToPortrait = false;

        cam = GetComponent<Camera>();
        //gazeIndicator = cam.transform.GetChild(0);

        //initialising GazeData stabilizer
        gazeUtils = new GazeDataValidator(30);

        GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push);

        //register for gaze updates
        GazeManager.Instance.AddGazeListener(this);
        
    }

    

    public void OnGazeUpdate(GazeData gazeData)
    {
        //Add frame to GazeData cache handler
        gazeUtils.Update(gazeData);
    }

    public void Update()
    {
        bool useMouseAsInput = true;
        Point2D userPos = gazeUtils.GetLastValidUserPosition();

        if (null != userPos)
        {
            //mapping cam panning to 3:2 aspect ratio
            double tx = (userPos.X * 5) - 2.5f;
            double ty = (userPos.Y * 3) - 1.5f;

            

            //camera 'look at' origo
            cam.transform.LookAt(Vector3.zero);
        }

        if (!useMouseAsInput)
        {
            Point2D gazeCoords = gazeUtils.GetLastValidSmoothedGazeCoordinates();

            if (null != gazeCoords)
            {
                //map gaze indicator
                Point2D gp = UnityGazeUtils.getGazeCoordsToUnityWindowCoords(gazeCoords);

                Vector3 screenPoint = new Vector3((float)gp.X, (float)gp.Y, cam.nearClipPlane + .1f);

                Vector3 planeCoord = cam.ScreenToWorldPoint(screenPoint);
                //gazeIndicator.transform.position = planeCoord;

                //handle collision detection
                checkGazeCollision(screenPoint); // eye tracker
            }
        }

        if (useMouseAsInput)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 PositionToMoveTo = ray.GetPoint(5);
            checkGazeCollision(PositionToMoveTo); // mouse
            //go.transform.position = PositionToMoveTo;
        }


        //handle keypress
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            Application.LoadLevel(0);
        }

    }

    
    private void checkGazeCollision(Vector3 screenPoint)
    {

        RaycastHit2D hit = Physics2D.Raycast(screenPoint, -Vector2.up);
        if (hit.collider != null)
        {
            Debug.Log("WOAH!");
            //float distance = Mathf.Abs(hit.point.y - transform.position.y);
        }
    }
    

    void OnApplicationQuit()
    {
        GazeManager.Instance.RemoveGazeListener(this);

        // Disconnect client
        GazeManager.Instance.Deactivate();
    }
}
