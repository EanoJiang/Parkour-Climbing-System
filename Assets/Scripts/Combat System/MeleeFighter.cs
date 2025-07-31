using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeleeFighter : MonoBehaviour
{
    Animator animator;
    AnimationClip attackClip;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        // 获取 AnimatorController 中名为 "Attack002" 的动画片段
        var clips = animator.runtimeAnimatorController.animationClips;
        attackClip = clips.FirstOrDefault(c => c.name == "Attack002");
    }

    public bool InAtkAction { get; private set; } = false;

    public void TryToAttack(){
        if(!InAtkAction)
        {
            StartCoroutine(Attack());

        }


    }

    /// <summary>
    /// Attack animation
    /// </summary>
    /// <returns></returns>
    IEnumerator Attack()
    {
        InAtkAction = true;

        animator.CrossFade("Attack002", 0.2f);
        yield return null;

        float attackAnimTime = attackClip.length;
        yield return new WaitForSeconds(attackAnimTime);

        InAtkAction = false;
        
    }

}
