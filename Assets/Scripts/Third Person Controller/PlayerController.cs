using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("玩家属性")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;

    [Header("Ground Check")]
    [SerializeField] float groundCheckRadius = 0.5f;
    //检测射线偏移量
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    //是否在地面
    bool isGrounded;
    //是否拥有控制权：默认拥有控制权，否则角色初始就不受控
    bool hasControl = true;

    //moveDir、velocity改成全局变量
    //当前角色的移动方向，这是实时移动方向，只要输入方向键就会更新
    Vector3 moveDir;
    //角色期望的移动方向，这个期望方向是和相机水平转动方向挂钩的，与鼠标或者手柄右摇杆一致
    Vector3 desireMoveDir;
    Vector3 velocity;

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

        //让人物期望移动方向关联相机的水平旋转朝向
        //  这样角色就只能在水平方向移动，而不是相机在竖直方向的旋转量也会改变角色的移动方向
        desireMoveDir = cameraController.PlanarRotation * moveInput;
        //让当前角色的移动方向等于期望方向
        moveDir = desireMoveDir;

        //如果没有控制权，后面的就不执行了
        if (!hasControl)
        {
            return;
        }

        velocity = Vector3.zero;

        #region 地面检测
        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);
        if (isGrounded)
        {
            //设置一个较小的负值，让角色在地上的时候被地面吸住
            ySpeed = -0.5f;
            //在地上的速度只需要初始化角色期望方向的速度就行，只有水平分量
            velocity = desireMoveDir * moveSpeed;
            #region 悬崖检测
            //在地上的时候进行悬崖检测,传给isOnLedge变量
            IsOnLedge = environmentScanner.LedgeCheck(desireMoveDir, out LedgeHitData ledgeHitData);
            //如果在悬崖边沿，就把击中数据传给LedgeHitData变量，用来在ParkourController里面调用
            if (IsOnLedge)
            {
                LedgeHitData = ledgeHitData;
                //调用悬崖边沿移动限制
                LedgeMovement();
                //  Debug.Log("On Ledge");
            }
            #endregion

            //在地面上，速度只有水平分量
            #region 角色动画控制
            //  dampTime是阻尼系数，用来平滑动画
            //这里不应该根据输入值赋值给BlendTree动画用的moveAmount参数
            //因为动画用的moveAmount参数只需要水平方向的移动量就行了，不需要考虑y轴
            //那么也就不需要方向，只需要值
            //所以传入归一化的 velocity.magnitude / moveSpeed就行了
            animator.SetFloat("moveAmount", velocity.magnitude / moveSpeed, 0.2f, Time.deltaTime);
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
        //没有输入并且移动方向角度小于0.2度就不更新转向，也就不会回到初始朝向
        //moveDir.magnitude > 0.2f 避免了太小的旋转角度也会更新
        if (moveAmount > 0 && moveDir.magnitude > 0.2f)
        {
            //人物模型转起来：让目标朝向与当前移动方向一致
            targetRotation = Quaternion.LookRotation(moveDir);
        }
        //更新transform.rotation：让人物从当前朝向到目标朝向慢慢转向
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                         rotationSpeed * Time.deltaTime);
        #endregion
    }

    //地面检测
    private void GroundCheck()
    {
        // Physics.CheckSphere()方法会向场景中的所有碰撞体投射一个胶囊体（capsule），有相交就返回true
        // 位置偏移用来在unity控制台里面调整
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    //悬崖边沿移动限制机制 
    private void LedgeMovement()
    {
        //计算玩家期望移动方向与悬崖边沿法线的有向夹角
        //所以这里的方向是左前是正，右前是负
        float signedAngle = Vector3.SignedAngle(LedgeHitData.hitSurface.normal, desireMoveDir, Vector3.up);
        //无向夹角
        float angle = Math.Abs(signedAngle);
        //这个夹角是锐角说明玩家将要走过悬崖边沿，限制不让走
        Debug.Log("angle: " + angle);
        if(Vector3.Angle(transform.forward, desireMoveDir) >80){
            //当前朝向与期望移动方向的夹角超过80度
            //转向悬崖边沿也就是期望方向，但是不移动
            velocity = Vector3.zero;
            //这里不能写moveDir = desireMoveDir;直接return就很好
            //这样直接返回就不会执行后面的代码了，人物转向直接由前面Update()里的代码控制
            return;
        }
        if(angle < 60){
            //速度设置为0，让玩家停止移动
            velocity = Vector3.zero;
            //让当前方向为0，也就是不让玩家旋转方向，但是期望方向还是与相机转动方向一致，仍然可以转回去
            moveDir = Vector3.zero;
        }
        else if (angle < 90)
        {
            //60度到90度，玩家直接90度转向与悬崖边沿平行的方向
            //只保留与 悬崖法线和竖直方向构成平面 的垂直方向速度
            //叉乘遵循右手法则：a x b = c，手指从a弯曲向b，拇指方向是c，所以这里是left方向
            var parallerDir_left = Vector3.Cross(Vector3.up, LedgeHitData.hitSurface.normal);
            //具体的左还是右，取决于玩家期望输入方向与悬崖边沿法线的有向夹角signedAngle的正负
            // (刚好也是左正右负，逻辑不变，直接乘就行)
            var dir = parallerDir_left * Math.Sign(signedAngle);
            //只保留与悬崖边沿平行的方向的速度
            velocity = velocity.magnitude * dir;
            //更新角色当前方向
            moveDir = dir;
        }
    }

    //角色控制
    public void SetControl(bool hasControl)
    {
        //传参给 hasControl 私有变量
        this.hasControl = hasControl;
        //根据 hasControl 变量的值来启用或禁用 charactercontroller 组件
        //如果角色没有控制权，则禁用角色控制器，hasControl = false，让角色静止不动
        charactercontroller.enabled = hasControl;

        //如果角色控制权被禁用，moveAmount也应该设置为0，目标朝向设置为当前朝向也就是不允许通过输入转动方向
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
