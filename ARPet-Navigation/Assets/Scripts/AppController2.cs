using GoogleARCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class AppController2 : MonoBehaviour

{

    public Camera FirstPersonCamera;

    public GameObject prefab;

    private bool mIsQuitting = false;

    private const float mModelRotation = 180.0f;

    // Use this for initialization

    void Start()

    {

        OnCheckDevice();

    }

    private void OnCheckDevice()

    {

        throw new NotImplementedException();

    }

    // Update is called once per frame

    void Update()

    {

        UpdateApplicationLifecycle();

        Touch touch;

        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)

        {

            return;

        }

        TrackableHit hit;

        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.PlaneWithinBounds;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))

        {

            if ((hit.Trackable is DetectedPlane) && Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) < 0)

            {

                Debug.Log("射线击中了DetectedPlane的背面！");

            }

            else

            {

                var FoxObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                FoxObject.transform.Rotate(0, mModelRotation, 0, Space.Self);

                var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                FoxObject.transform.parent = anchor.transform;

            }

        }

    }

    private void UpdateApplicationLifecycle() {
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        if (mIsQuitting) {
            return;
        }
    }
 }

