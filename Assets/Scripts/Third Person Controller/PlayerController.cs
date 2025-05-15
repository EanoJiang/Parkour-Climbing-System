using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("玩家属性")]
    [SerializeField]float moveSpeed = 5f;
    [SerializeField]float rotationSpeed = 500f;

    [Header("Ground Check")]
    [SerializeField]float groundCheckRadius = 0.5f;
    //检测射线偏移量
    [SerializeField]Vector3 groundCheckOffset;
    [SerializeField]LayerMask groundLayer;

    //是否在地面
    bool isGrounded;
    //是否拥有控制权：默认拥有控制权，否则角色初始就不受控
    bool hasControl = true;
    //是否在悬崖边沿上
    public bool IsOnLedge { get; set; }
    //悬崖边沿击中相关数据
    public LedgeHitData LedgeHitData { get; set; }

    float ySpeed;

    Quaternion targetRotation;

    CameraController cameraController;
    Animator animator;
    CharacterController charactercontroller;
    EnvironmentScanner environmentScanner;

    private void Awake()
    {
        //相机控制器设置为main camera
        cameraController = Camera.main.GetComponent<CameraController>();
        //角色动画
        animator = GetComponent<Animator>();
        //角色控制器
        charactercontroller = GetComponent<CharacterController>();
        //环境扫描器
        environmentScanner = GetComponent<EnvironmentScanner>();
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

        //让人物移动方向关联相机的水平旋转朝向
        var moveDir = cameraController.PlanarRotation * moveInput;

        //如果没有控制权，后面的就不执行了
        if(!hasControl){
            return;
        }

        var velocity = Vector3.zero;

        #region 地面检测
        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);
        if (isGrounded)
        {
            //设置一个较小的负值，让角色在地上的时候被地面吸住
            ySpeed = -0.5f;
            velocity = moveDir * moveSpeed;
            #region 悬崖检测
            //在地上的时候进行悬崖检测,传给isOnLedge变量
            IsOnLedge = environmentScanner.LedgeCheck(moveDir,out LedgeHitData ledgeHitData);
            //如果在悬崖边沿，就把击中数据传给LedgeHitData变量，用来在ParkourController里面调用
            if (IsOnLedge)
            {
                LedgeHitData = ledgeHitData;
                Debug.Log("On Ledge");
            }
            #endregion
        }
        else
        {
            //在空中时，ySpeed受重力控制
            ySpeed += Physics.gravity.y * Time.deltaTime;
            //简单模拟有空气阻力的平抛运动：空中时的速度设置为角色朝向速度的一半
            velocity = transform.forward * moveSpeed / 2;
        }
        #endregion
        //更新y轴方向的速度
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
        //设置人物动画参数moveAmount
        animator.SetFloat("moveAmount", moveAmount,0.2f,Time.deltaTime);

        #endregion


    }

    //地面检测
    private void GroundCheck()
    {
        // Physics.CheckSphere()方法会向场景中的所有碰撞体投射一个胶囊体（capsule），有相交就返回true
        // 位置偏移用来在unity控制台里面调整
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    //角色控制
    public void SetControl(bool hasControl){
        //传参给 hasControl 私有变量
        this.hasControl = hasControl;
        //根据 hasControl 变量的值来启用或禁用 charactercontroller 组件
        //如果角色没有控制权，则禁用角色控制器，hasControl = false，让角色静止不动
        charactercontroller.enabled = hasControl;

        //如果角色控制权被禁用，则更新动画参数和朝向
        if (!hasControl)
        {
            //更新动画参数
            animator.SetFloat("moveAmount", 0f);
            //更新朝向
            targetRotation = transform.rotation;

        }
    }

    //画检测射线
    private void OnDrawGizmosSelected()
    {
        //射线颜色，最后一个参数是透明度
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    //让rotationSpeed可以被外部访问
    public float RotationSpeed => rotationSpeed;

}
