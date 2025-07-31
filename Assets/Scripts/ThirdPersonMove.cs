using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMove : MonoBehaviour
{
    // CharacterController 组件
    private CharacterController _controller;
    // 主相机
    private GameObject _mainCamera;
    void Start()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        _controller = GetComponent<CharacterController>();
    }
    // 移动速度
    public float speed = 6.0f;
    // 目标旋转角度
    float _targetRot = 0.0f;
    // 平滑旋转时间
    public float RotationSmoothTime = 0.1f;
    float _rotationVelocity;
    void Update()
    {
        Vector3 velocity = new Vector3(0, -1, 0);
        if (_move != Vector2.zero)
        {
            // 计算输入方向，将其转换为世界空间
            Vector3 inputDir = new Vector3(_move.x, 0.0f, _move.y).normalized;
            // 计算目标旋转角度
            _targetRot = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
            // 平稳地旋转玩家，使其面向他们移动的方向。
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRot,
                                ref _rotationVelocity, RotationSmoothTime);
            // 旋转玩家
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            // 计算移动方向
            Vector3 targetDir = Quaternion.Euler(0.0f, _targetRot, 0.0f) * Vector3.forward;
            velocity += targetDir.normalized * (speed * Time.deltaTime);

            
        }
        // 移动玩家
        _controller.Move(velocity);
    }
    Vector2 _move;
    void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
    }
}
