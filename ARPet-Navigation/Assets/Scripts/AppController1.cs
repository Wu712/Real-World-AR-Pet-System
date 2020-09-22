using UnityEngine;
using GoogleARCore;
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

public class AppController1: MonoBehaviour
{

    public Camera FirstPersonCamera;    //ARCore的FirstPersonCamera摄像机
    public GameObject prefab;           //点击生成的对象预制体

    private const float mModelRotation = 180.0f;

    void Start()
    {
        //检查设备
        OnCheckDevice();
    }

    void Update()
    {
        //管理应用的生命周期
        UpdateApplicationLifecycle();

        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        //TrackableHit：保存的是发生碰撞检测时的检测到相关信息
        TrackableHit hit;
        //TrackableHitFlags：用来过滤需要进行碰撞检测的对象类型,其值可以一个，也可以是几个
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.PlaneWithinBounds;

        //ARCore在Frame中为我们准备了四种发射射线检测物体的方法，具体参考文章
        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            if ((hit.Trackable is DetectedPlane) && Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("射线击中了DetectedPlane的背面！");
            }
            else
            {
                var prefabIns = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);
                prefabIns.transform.Rotate(0, mModelRotation, 0, Space.Self);
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                prefabIns.transform.parent = anchor.transform;
            }
        }
    }

    /// <summary>
    /// 检查设备
    /// </summary>
    private void OnCheckDevice()
    {
        if (Session.Status == SessionStatus.ErrorSessionConfigurationNotSupported)
        {
            ShowAndroidToastMessage("ARCore在本机上不受支持或配置错误！");
            Invoke("DoQuit", 0.5f);
        }
        else if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            ShowAndroidToastMessage("AR应用的运行需要使用摄像头，现无法获取到摄像头授权信息，请允许使用摄像头！");
            Invoke("DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            ShowAndroidToastMessage("ARCore运行时出现错误，请重新启动本程序！");
            Invoke("DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// 管理应用的生命周期
    /// </summary>
    private void UpdateApplicationLifecycle()
    {
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            //Screen.sleepTimeout：一种节能设置，允许屏幕在上次激活用户交互后一段时间变暗
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            //SleepTimeout.NeverSleep：防止屏幕变暗
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }

    /// <summary>
    /// 弹出信息提示
    /// </summary>
    /// <param name="message">要弹出的信息</param>
    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}