using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("玩家属性")]
    [SerializeField]float moveSpeed = 5f;
    [SerializeField]float rotationSpeed = 10f;

    [Header("Ground Check")]
    [SerializeField]float groundCheckRadius = 0.5f;
    //检测射线偏移量
    [SerializeField]Vector3 groundCheckOffset;
    [SerializeField]LayerMask groundLayer;

    bool isGrounded;

    float ySpeed;

    Quaternion targetRotation;

    CameraController cameraController;
    Animator animator;
    CharacterController charactercontroller;

    private void Awake()
    {
        //相机控制器设置为main camera
        cameraController = Camera.main.GetComponent<CameraController>();
        //角色动画
        animator = GetComponent<Animator>();
        //角色控制器
        charactercontroller = GetComponent<CharacterController>();
    }
    private void Update()
    {
        #region 角色输入控制
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //把moveAmount限制在0-1之间(混合树的区间)
        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        //标准化 moveInput 向量
        var moveInput = new Vector3(h, 0, v).normalized;

        //让人物移动方向关联相机的朝向
        var moveDir = cameraController.PlanarRotation * moveInput;

        #region 碰撞检测
        GroundCheck();

        #endregion

        if (isGrounded)
        {
            //设置一个较小的负值，让角色在地上的时候被地面吸住
            ySpeed = -0.5f;
        }
        else
        {
            //在空中时，角色的速度由ySpeed决定
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;
        //帧同步移动
        //通过CharacterController.Move()来控制角色的移动，通过碰撞限制运动
        charactercontroller.Move(velocity * Time.deltaTime);

        //每次判断moveAmount的时候，确保只有在玩家实际移动时才会更新转向
        //没有输入就不更新转向，也就不会回到初始朝向
        if (moveAmount > 0)
        {
            //人物模型转起来：让人物朝向与移动方向一致
            targetRotation = Quaternion.LookRotation(moveDir);
        }
        //更新transform.rotation：让人物从当前朝向到目标朝向慢慢转向
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                         rotationSpeed * Time.deltaTime);
        #endregion

        #region 角色动画控制
        //角色动画播放
        animator.SetFloat("moveAmount", moveAmount,0.2f,Time.deltaTime);

        #endregion


    }

    private void GroundCheck()
    {
        // Physics.CheckSphere()方法会向场景中的所有碰撞体投射一个胶囊体（capsule），有相交就返回true
        // 位置偏移用来在unity控制台里面调整
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    //画检测射线
    private void OnDrawGizmosSelected()
    {
        //射线颜色，最后一个参数是透明度
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

}
