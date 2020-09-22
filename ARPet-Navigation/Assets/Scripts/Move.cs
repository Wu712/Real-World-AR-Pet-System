using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using System;
#if UNITY_EDITOR
// NOTE:
// - InstantPreviewInput does not support `deltaPosition`.
// - InstantPreviewInput does not support input from
//   multiple simultaneous screen touches.
// - InstantPreviewInput might miss frames. A steady stream
//   of touch events across frames while holding your finger
//   on the screen is not guaranteed.
// - InstantPreviewInput does not generate Unity UI event system
//   events from device touches. Use mouse/keyboard in the editor
//   instead.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class Move : MonoBehaviour
{
    public Camera FirstPersonCamera;
    //public GameObject prefab;
    private const float mModelRotation = 180.0f;
    private bool mIsQuitting = false;

    public GameObject routePoint; //indicator of route point
    public GameObject placementIndicator; //indicator of camera raycast
    
    private Pose placementPose;
    private bool placementPoseIsValid = false;
    private bool isPlacing = true; //true if is placing routepoint.
    private int indexOfRoutePoint;
    public List<GameObject> wayPoints;
    public int num = 0;
    public float minDist = 0.1f;
    public float speed = 0.5f;
    public bool rand = false;
    public bool circle = true;
    public bool go = true;

    public void updateWaypoints()
    {
        if (wayPoints.Count != 0)//(wayPoints != null)
        {
            float dist = Vector3.Distance(gameObject.transform.position, wayPoints[num].transform.position);
            if (go)
            {
                if (dist > minDist)
                {
                    MoveToNextPoint();
                }
                else
                {
                    if (!rand)
                    {
                        if ((num + 1 == wayPoints.Count) && (circle))
                        {
                            num = 0;
                        }
                        else if ((num + 1 == wayPoints.Count) && (!circle))
                        {
                            return;
                        }
                        else
                        {
                            num++;
                        }
                    }
                    else
                    {
                        num = UnityEngine.Random.Range(0, wayPoints.Count);
                    }
                }
            }
        }
    }

    public void MoveToNextPoint()
    {
        gameObject.transform.LookAt(wayPoints[num].transform.position);
        gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;
    }

    private void PlaceRoutePoint()
    {
        GameObject a = Instantiate(routePoint, placementPose.position, placementPose.rotation);
        wayPoints.Add(a);
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        //var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        ///
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }
        placementPoseIsValid = true;

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
                placementPose = hit.Pose;
                var FoxObject = Instantiate(routePoint, hit.Pose.position, hit.Pose.rotation);
                FoxObject.transform.Rotate(0, mModelRotation, 0, Space.Self);
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                FoxObject.transform.parent = anchor.transform;
                wayPoints.Add(FoxObject);
                Debug.Log(wayPoints.Count);

            }
        }
        ///
        //arOrigin.GetComponent<ARRaycastManager>().Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

        //placementPoseIsValid = hits.Count > 0;
        /*if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;
            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }*/
    }
    // Start is called before the first frame update
    void Start()
    {
        OnCheckDevice();
    }

    private void OnCheckDevice()
    {
        throw new NotImplementedException();
    }

    // Update is called once per frame
    /*void Update()
    {
        UpdatePlacementPose();
        //UpdatePlacementIndicator();
        updateWaypoints();

        /*if (placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && isPlacing)
        {
            PlaceRoutePoint();
        }*/
    //}

    void Update()
    {
        UpdateApplicationLifecycle();

        updateWaypoints();


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
                var FoxObject = Instantiate(routePoint, hit.Pose.position, hit.Pose.rotation);
                FoxObject.transform.Rotate(0, mModelRotation, 0, Space.Self);
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                FoxObject.transform.parent = anchor.transform;
                wayPoints.Add(FoxObject);
                Debug.Log(wayPoints.Count);
            }
        }
    }

    private void UpdateApplicationLifecycle() { if (Session.Status != SessionStatus.Tracking) { const int lostTrackingSleepTimeout = 15; Screen.sleepTimeout = lostTrackingSleepTimeout; } else { Screen.sleepTimeout = SleepTimeout.NeverSleep; } if (mIsQuitting) { return; } }
}
