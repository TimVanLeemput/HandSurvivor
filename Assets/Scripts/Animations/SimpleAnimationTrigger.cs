using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.Events;

public class SimpleAnimationTrigger : MonoBehaviour
{
    [SerializeField] private string animationParameterName = "canMove";
    [SerializeField] private bool resetAnimationClipAfterTrigger = false;
    private Animator animator;
    private Coroutine resetRoutine;
    
    #region Events
    [SerializeField] private UnityEvent EndOfAnimationEvent = null;
    #endregion

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    [ButtonMethod]
    public void TriggerAnimation()
    {
        animator.SetBool(animationParameterName, true);

        if (resetRoutine != null)
            StopCoroutine(resetRoutine);
        if (!resetAnimationClipAfterTrigger) return;
        resetRoutine = StartCoroutine(ResetAfterAnimation());
    }

    private IEnumerator ResetAfterAnimation()
    {
        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float duration = stateInfo.length;

        yield return new WaitForSeconds(duration);

        animator.SetBool(animationParameterName, false);
        resetRoutine = null;
        EndOfAnimationEvent?.Invoke();
    }

    [ButtonMethod]
    public void StopAnimation()
    {
        animator.SetBool(animationParameterName, false);

        if (resetRoutine != null)
            StopCoroutine(resetRoutine);
        resetRoutine = null;
    }
}