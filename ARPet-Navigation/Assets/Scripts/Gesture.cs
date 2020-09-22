using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Gesture : MonoBehaviour
{

    float xSpeed = 100f;
    float ySpeed = 100f;
    float x = 0f;
    float y = 0f;
    float Speed = 0.01f;


    /// <summary>  
    /// 第一次按下的位置  
    /// </summary>  
    private Vector3 first = Vector3.zero;
    /// <summary>  
    /// 鼠标的拖拽位置（第二次的位置）  
    /// </summary>  
    private Vector3 second = Vector3.zero;
    /// <summary>  
    /// 旋转的角度  
    /// </summary>  
    private float angle = 3f;
    /// <summary>  
    /// 记录当前的方向
    /// </summary>
    public GameObject model;

    Vector2 oldPosition1;
    Vector2 oldPosition2;

    void Update()
    {
        if (Input.anyKey)
        {
            //当只有一次触摸
            if (Input.touchCount == 1)
            {
                //触摸类型，滑动
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    //移动拖拽
                    //获取x轴
                    x = Input.GetAxis("Mouse X") * Speed;
                    //获取y轴
                    y = Input.GetAxis("Mouse Y") * Speed;
                    model.transform.Translate(-x, y, 0);//*Time.deltaTime


                    //旋转
                    ////获取x轴
                    x = Input.GetAxis("Mouse X") * xSpeed;
                    ////获取y轴
                    y = Input.GetAxis("Mouse Y") * ySpeed;
                    ////对模型进行上下左右旋转
                    model.transform.Rotate(Vector3.up * x * Time.deltaTime, Space.World);
                    //model.transform.Rotate(Vector3.left * -y * Time.deltaTime, Space.World);
                }

            }
            //当两次触摸
            if (Input.touchCount > 1)
            {
                //两次触摸都有滑动
                if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
                {
                    //获取第一、二次两次触摸的位置
                    Vector2 tempPosition1 = Input.GetTouch(0).position;
                    Vector2 tempPosition2 = Input.GetTouch(1).position;
                    //放大
                    if (isEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2))
                    {
                        float oldScale = model.transform.localScale.x;
                        float newScale = oldScale * 1.025f;
                        model.transform.localScale = new Vector3(newScale, newScale, newScale);
                    }
                    else//缩小
                    {
                        float oldScale = model.transform.localScale.x;
                        float newScale = oldScale / 1.025f;
                        model.transform.localScale = new Vector3(newScale, newScale, newScale);

                    }
                    //备份上一次触摸点的位置，用于对比   
                    oldPosition1 = tempPosition1;
                    oldPosition2 = tempPosition2;
                }
            }
        }

    }
    /// <summary>
    /// 比较两次的位置，大小，来进行放大还是缩小
    /// </summary>
    /// <param name="oP1"></param>
    /// <param name="oP2"></param>
    /// <param name="nP1"></param>
    /// <param name="nP2"></param>
    /// <returns></returns>
    bool isEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        //函数传入上一次触摸两点的位置与本次触摸两点的位置计算出用户的手势   
        var leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        var leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
        if (leng1 < leng2)
        {
            //放大手势   
            return true;
        }
        else
        {
            //缩小手势   
            return false;
        }
    }
}

