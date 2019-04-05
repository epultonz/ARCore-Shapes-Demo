using System;
using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Controls the HelloAR example.
/// </summary>
public class MainController : MonoBehaviour
{
	public GameObject cubePrefab;
	public GameObject cylinderPrefab;
	public GameObject spherePrefab;
	public GameObject tetraPrefab;

	public GameObject UICanvas;
	private Toggle ftg;		// the toggle for the force button
	private Dropdown dpd;	// the list dropdown to choose the shape objects

	//private float currTime = 0.0f;
	//private const float waitTime = 10.0f;
	//private bool isMoving = false;

	/// <summary>
	/// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
	/// </summary>
	private bool m_IsQuitting = false;

	// At the start of the application, we init the Toggle and Dropdown
	// component from the UI gameobject
	public void Start() {
		ftg = UICanvas.GetComponentInChildren<Toggle>();
		dpd = UICanvas.GetComponentInChildren<Dropdown>();
	}

    /// <summary>
    /// The Unity Update() method. Runs every frame
    /// </summary>
    public void Update()
    {
        _UpdateApplicationLifecycle();

		// If the player has not touched the screen, we are done with this update.
		Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

		// Cast a ray from the touch point of a user towards the game world space
		var ray = Camera.main.ScreenPointToRay(touch.position);
		var hitInfo = new RaycastHit();
		// set the distance the ray can go to 5 meters, if the ray hits a UI object, we're done 
		// with this update
		if (Physics.Raycast(ray, out hitInfo, 5.0f) && hitInfo.transform.tag == "UIObj")
		{
			return;
		}
		else if (hitInfo.transform.tag == "ShapeObject")	// else, if the ray hits a shapeObject
		{
			//Transform anc = null;

			// If the force toggle is active, add force to the object hit by raycast
			if (ftg.isOn) { // push force to the objshape
				hitInfo.rigidbody.AddForceAtPosition(ray.direction, hitInfo.point);
				return;

			} /* else if (currTime > waitTime && !isMoving) {

				anc = hitInfo.rigidbody.transform.GetComponentInParent<Anchor>().transform;
				moveShapeObject(hitInfo);       //move the object shape

			} else if (currTime > waitTime && touch.phase == TouchPhase.Ended && isMoving) {
				hitInfo.rigidbody.transform.SetParent(anc);
				isMoving = false;
			}

			if (touch.phase == TouchPhase.Ended)
			{
				currTime = 0.0f;
			}
			else {
				currTime += (1.0f * Time.deltaTime);
			}
			*/
		}

		// Raycast against the location the player touched to search for planes.
		TrackableHit hit;
		// Put a filter so the raycast only picks plane with a convex shape and
		// approximately 45 degree plane 
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

		if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit) && !ftg.isOn)
		{
			// Use hit pose and camera pose to check if hittest is from the
			// back of the plane, if it is, no need to create the anchor.
			if ((hit.Trackable is DetectedPlane) &&
				Vector3.Dot(Camera.main.transform.position - hit.Pose.position,
					hit.Pose.rotation * Vector3.up) < 0)
			{
				Debug.Log("Hit at back of the current DetectedPlane");
			}
			else
			{
				// if the raycast hits a plane, create a shapeobject there
				createPrefab(hit);
			}
		}		
	}

	/*
	private void moveShapeObject(RaycastHit hit)
	{
		hit.transform.parent = Camera.main.transform;
		isMoving = true;
	}*/

	// A method to change the "force" toggle red and green
	public void changeToggleColor()
	{
		if (ftg.isOn)
		{
			Color cl = Color.green;
			ftg.GetComponent<Image>().color = cl;
		}
		else
		{
			Color cl = Color.red;
			ftg.GetComponent<Image>().color = cl;
		}
	}

	// Method to create the shapeObject
	private void createPrefab(TrackableHit hit) {

		GameObject shapePrefab;
		// check which prefab to use by looking at the dropdown index value
		// at the current runtime
		if (dpd.value == 0)
		{
			shapePrefab = cubePrefab;
		}
		else if (dpd.value == 1)
		{
			shapePrefab = cylinderPrefab;
		}
		else if (dpd.value == 2)
		{
			shapePrefab = spherePrefab;
		}
		else {
			shapePrefab = tetraPrefab;
		}

		// Instantiate cube model at the hit pose with half a meter up
		var cubeObject = Instantiate(shapePrefab, hit.Pose.position + new Vector3(0.0f, 0.5f, 0.0f), hit.Pose.rotation);

		// Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
		// Also make the render color of the objects random
		cubeObject.transform.Rotate(0, 0, 0, Space.Self);
		cubeObject.GetComponent<MeshRenderer>().material.color = UnityEngine.Random.ColorHSV();

		// Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
		// world evolves.
		var anchor = hit.Trackable.CreateAnchor(hit.Pose);

		// Make cube model a child of the anchor.
		cubeObject.transform.parent = anchor.transform;
	}


	/// <summary>
	/// Check and update the application lifecycle.
	/// </summary>
	private void _UpdateApplicationLifecycle()
    {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }
}