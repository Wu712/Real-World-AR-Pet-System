using GoogleARCore;
using System;
using System.Collections.Generic;
using UnityEngine;
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

public class APPController : MonoBehaviour
{

    public List<GameObject> routeArrow;
    public Camera FirstPersonCamera;
    public static int MAX_LEN = 1000;       // Maximum length of the pathArray

    public GameObject arrowGameObject;      // Original instance of the arrow figure

    private Vector3[] hitArr;               // store the points of the current path
    private Vector3[] path1Array,           // store source to destination path of the location A
                      path2Array;           // store source to destination path of the location B

    private GameObject[] gameArray;         // store the drawable figure of the arrow on the flat plane

    private int i;                          // Number of vector points in the current pathArray
    private int len, k;                     // totalLength of the pathArray to be iterated or stored in location X
    
    private int path1Len, path2Len;         // length of the path of location A and B, respectively

    public float speed;                     // manage the speed of the helper rabit

    public GameObject followPrefab;         // the original figure of the rabit
    private GameObject travellingSalesman;  // replicate the figure of the rabit for the application gaile
                                            //  public GameObject travellingSalesman;

    private int followPathBool;             // whether the path is to followed or not

    public static bool rabitVisible;		// whether the rabit is visible on the flat plane or not
    private readonly bool mIsQuitting = false;
    private const float mModelRotation = 180.0f;
    // Use this for initialization
    void Start()
    {
        OnCheckDevice();

        Initialize();

        path1Array = new Vector3[MAX_LEN];      // create an array for the path1Array
        path2Array = new Vector3[MAX_LEN];      // create an array for the path2Array

        gameArray = new GameObject[MAX_LEN];    // create an array of the arrow figure

        path1Len = 0; path2Len = 0;             // initialize the pathLen to 0

        rabitVisible = false; 					// initial not visible state
    }

    public void Initialize()
    {
        hitArr = new Vector3[MAX_LEN];  // reset the current pathArray for the current path
        i = 0;                          // set pathArray length to 0
        len = -1;                       // set final pathArray length to -1 (for safety purposes)
        followPathBool = 0;             // set followPath Boolean to false, initially
        speed = 5.0f;                  // set the speed of the helper rabit
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


        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.PlaneWithinBounds;


        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out TrackableHit hit))
        {
            if ((hit.Trackable is DetectedPlane) && Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current detected plane!");
            }
            else
            {
                var FoxObject = Instantiate(arrowGameObject, hit.Pose.position, hit.Pose.rotation);
                FoxObject.transform.Rotate(0, mModelRotation, 0, Space.Self);
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                FoxObject.transform.parent = anchor.transform;

             
            }
        }
        if (Input.GetMouseButtonDown(0) && followPathBool == 0)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                var hits = new List<TrackableHit>();
                hitArr[i] = hits[i].Pose.position;
                i++;
            
                //var go = GameObject.Instantiate(arrowGameObject, hitInfo.point, Quaternion.identity);
                var go = GameObject.Instantiate(arrowGameObject, hit.Pose.position, Quaternion.identity);
                go.GetComponent<MeshRenderer>().material.color = UnityEngine.Random.ColorHSV();

                gameArray[i] = go;
                i++;

            }
        }
      //  Debug.Log(hit.Pose.position);


        if (followPathBool == 1)
        {
            float step = speed * Time.deltaTime;
            //hitArr[k] = hit.Pose.position;
           
            if (k < len)
            {
                travellingSalesman.transform.position = Vector3.MoveTowards(travellingSalesman.transform.position, hitArr[k], step);
               // travellingSalesman.transform.position += transform.forward * step;
                travellingSalesman.transform.LookAt(hitArr[k]);
                Vector3 relativePos = hitArr[k] - travellingSalesman.transform.position;

                Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                travellingSalesman.transform.rotation = rotation;


                if (travellingSalesman.transform.position == hitArr[k])
                {
                    k++;
                }
               // Debug.Log(hitArr[k].ToString());
            }

            if (k == len)
            {
                followPathBool = 0;
                Destroy(travellingSalesman); // delete the instance gaile

                Reset();

                rabitVisible = false; // reset visibility to initial state	
            }
        }
    }

    //public void Follow() {
    //    travellingSalesman = GameObject.Instantiate(followPrefab,hitArr[0],Quaternion.identity);
    //}
    public void Follow()
    {
        if (rabitVisible)
        { // already following

            Destroy(travellingSalesman);
        }

        followPathBool = 1;
        travellingSalesman = GameObject.Instantiate(followPrefab, hitArr[0], Quaternion.identity);
        //travellingSalesman = GameObject.Instantiate(followPrefab,gameArray[0].GetComponent<Transform>().position , Quaternion.identity);

        if (followPathBool == 1)
        {
            float step = speed * Time.deltaTime;
            //hitArr[k] = hit.Pose.position;

            if (k < len)
            {
                travellingSalesman.transform.position = Vector3.MoveTowards(travellingSalesman.transform.position, hitArr[k], step);
                // travellingSalesman.transform.position += transform.forward * step;
                travellingSalesman.transform.LookAt(hitArr[k]);
                Vector3 relativePos = hitArr[k] - travellingSalesman.transform.position;

                Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                travellingSalesman.transform.rotation = rotation;


                if (travellingSalesman.transform.position == hitArr[k])
                {
                    k++;
                }
              //  Debug.Log(hitArr[k].ToString());
            }

            if (k == len)
            {
                followPathBool = 0;
                Destroy(travellingSalesman); // delete the instance gaile

                Reset();

                rabitVisible = false; // reset visibility to initial state	
            }
        }

        len = i;
        k = 1;

        rabitVisible = true;

       // Debug.Log(hitArr[0].ToString());
    }
   
    public void DrawPath(Vector3[] arr, int limit)
    {

        for (int x = 0; x < limit; x++)
        {
            var go = GameObject.Instantiate(arrowGameObject, arr[x], Quaternion.identity);
            go.GetComponent<MeshRenderer>().material.color = UnityEngine.Random.ColorHSV();

            gameArray[x] = go;
     
        }

    }
    

    public void Reset()
    {
        for (int x = 0; x < i; x++)
        {
            Destroy(gameArray[x]);
        }
        Initialize(); // i = 0, len = 0

        if (rabitVisible)
        { // if rabit moving
            followPathBool = 0;
            rabitVisible = false;
            Destroy(travellingSalesman);

            rabitVisible = false;
        }
    }

    public void Path1Save()
    {

        if (rabitVisible) return; // if rabit moving
       
        rabitVisible = true;
        path1Len = i; // n
        for (int x = 0; x < path1Len; x++)
        {
            path1Array[x] = hitArr[x];
        }
        Reset();
        Initialize();
    }

    public void FollowPath1()
    {

        if (rabitVisible == true)
        {
            Destroy(travellingSalesman); // current rabit Destroy  
        }
        rabitVisible = true;

        for (int x = 0; x < path1Len; x++)
        {
            hitArr[x] = path1Array[x];
        }
        i = path1Len; // i = n

        DrawPath(path1Array, path1Len); // redraw, gameArray store
        Follow();
    }

    public void Path2Save()
    {

        if (rabitVisible) return; // if rabit moving

        rabitVisible = true;
        path2Len = i; // n
        for (int x = 0; x < path2Len; x++)
        {
            path2Array[x] = hitArr[x];
        }
        Reset();
        Initialize();
    }

    public void FollowPath2()
    {

        if (rabitVisible == true)
        {
            Destroy(travellingSalesman); // current rabit Destroy  gaile
        }
        rabitVisible = true;

        for (int x = 0; x < path2Len; x++)
        {
            hitArr[x] = path2Array[x];
        }
        i = path2Len; // i = n

        DrawPath(path2Array, path2Len); // redraw, gameArray store
        Follow();


    }

    private void UpdateApplicationLifecycle() { if (Session.Status != SessionStatus.Tracking) { const int lostTrackingSleepTimeout = 15; Screen.sleepTimeout = lostTrackingSleepTimeout; } else { Screen.sleepTimeout = SleepTimeout.NeverSleep; } if (mIsQuitting) { return; } }

}

