using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool isMouseOver = false;
    public Animator animator;

    void Update()
    {
        if (isMouseOver == true) return;

        animator.SetBool("isSelected", EventSystem.current.currentSelectedGameObject == this.gameObject ? true : false);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        animator.SetBool("isSelected", true);
        isMouseOver = true;
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        animator.SetBool("isSelected", false);
        isMouseOver = false;
    }
}
