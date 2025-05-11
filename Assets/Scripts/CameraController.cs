using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //����������Ŀ��
    [SerializeField] Transform followTarget;
    [SerializeField] float rotationSpeed = 1.5f;
    //����
    [SerializeField] float distance;

    //��y�����ת�Ƕȡ���ˮƽ�ӽ���ת
    float rotationY;
    //��x�����ת�Ƕȡ�����ֱ�ӽ���ת
    float rotationX;
    //����rotationX����
    [SerializeField] float minVerticalAngle = -20;
    [SerializeField] float maxVerticalAngle = 45;
    //���ƫ���������������λ���Ӳ�ƫ��
    [SerializeField] Vector2 frameOffset;

    //�ӽǿ��Ʒ�ת
    [Header("�ӽǿ��Ʒ�ת:invertX�Ƿ�ת��ֱ�ӽ�,invertY�Ƿ�תˮƽ�ӽ�")]
    [SerializeField] bool invertX;
    [SerializeField] bool invertY;

    float invertXValue;
    float invertYValue;

    private void Start()
    {
        //���ع��
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    private void Update()
    {
        //�ӽǿ��Ʒ�ת����
        invertXValue = (invertX)? -1 : 1;
        invertYValue = (invertY)? -1 : 1;

        //ˮƽ�ӽǿ��ơ������(�ֱ�)x�����rotationY
        rotationY += Input.GetAxis("Camera X") * rotationSpeed * invertYValue;
        //��ֱ�ӽǿ��ơ������(�ֱ�)y�����rotationX
        rotationX += Input.GetAxis("Camera Y") * rotationSpeed * invertXValue;
        //����rotationX����
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        //�ӽ���ת����
        //��Ҫˮƽ��ת�ӽǣ�������Ҫ�Ĳ���Ϊ��y����ת�Ƕ�
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        //������Ľ���λ��
        var focusPosition = followTarget.position + new Vector3(frameOffset.x, frameOffset.y, 0);
        //���������Ŀ�����5����λ��λ��
        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        //�����ʼ�ճ���Ŀ��
        transform.rotation = targetRotation;
    }

    //ˮƽ�������ת�������������ˮƽ��ת��Ԫ����
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);

}
