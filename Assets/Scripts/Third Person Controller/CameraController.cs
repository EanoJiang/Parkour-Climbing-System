using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //摄像机跟随的目标
    [SerializeField] Transform followTarget;
    [SerializeField] float rotationSpeed = 1.5f;
    //距离
    [SerializeField] float distance;

    //绕y轴的旋转角度——水平视角旋转
    float rotationY;
    //绕x轴的旋转角度——垂直视角旋转
    float rotationX;
    //限制rotationX幅度
    [SerializeField] float minVerticalAngle = -20;
    [SerializeField] float maxVerticalAngle = 45;
    //框架偏移向量——摄像机位置视差偏移
    [SerializeField] Vector2 frameOffset;

    //视角控制反转
    [Header("视角控制反转:invertX是否反转垂直视角,invertY是否反转水平视角")]
    [SerializeField] bool invertX;
    [SerializeField] bool invertY;

    float invertXValue;
    float invertYValue;

    private void Start()
    {
        //隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    private void Update()
    {
        //视角控制反转参数
        invertXValue = (invertX)? -1 : 1;
        invertYValue = (invertY)? -1 : 1;

        //水平视角控制——鼠标(手柄)x轴控制rotationY
        rotationY += Input.GetAxis("Camera X") * rotationSpeed * invertYValue;
        //垂直视角控制——鼠标(手柄)y轴控制rotationX
        rotationX += Input.GetAxis("Camera Y") * rotationSpeed * invertXValue;
        //限制rotationX幅度
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        //视角旋转参量
        //想要水平旋转视角，所以需要的参量为绕y轴旋转角度
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        //摄像机的焦点位置
        var focusPosition = followTarget.position + new Vector3(frameOffset.x, frameOffset.y, 0);
        //摄像机放在目标后面5个单位的位置
        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        //摄像机始终朝向目标
        transform.rotation = targetRotation;
    }

    //水平方向的旋转，返回摄像机的水平旋转四元数。
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);

}
