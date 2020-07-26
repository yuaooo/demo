using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyFlow : MonoBehaviour
{
    [Header("=====控制======")]
    public bool start = true;           //是否开启滚动
    public bool horizontal = true;     //是否横向滚动
    public float speed;
    public float endPos = -56.44f;      //一个组件长度


    void Update()
    {
        if (!start)
        {
            return;
        }
        Vector2 pos = transform.localPosition;

        if (horizontal)
        {
            pos.x -= speed * Time.deltaTime;
            if (pos.x < endPos)
            {
                pos.x = 48.06f;
            }
        }
        else
        {
            pos.y -= speed * Time.deltaTime;
            if (pos.y < -endPos)
            {
                pos.y = 0;
            }
        }
        transform.localPosition = pos;
    }
}
