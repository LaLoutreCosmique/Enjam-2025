using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ClickableCharacter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    SpriteRenderer renderer;
    
    [SerializeField] GameManager gm;
    public int characterIndex;

    private bool isServed;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        StartInteractableAnim();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!gm.hasServiceStarted || isServed) return;
        gm.ToggleCharacter(characterIndex);
        renderer.color = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!gm.hasServiceStarted || isServed) return;
        renderer.color = new Color(1f, 1f, 1f, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!gm.hasServiceStarted || isServed) return;
        
        gm.ServeCharacter(this);
        Select();
    }

    private void Select()
    {
        renderer.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        isServed = true;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void Deselect()
    {
        renderer.color = new Color(1f, 1f, 1f, 1f);
        isServed = false;
    }

    private void StartInteractableAnim()
    {
        StartCoroutine(InteractableAnimationRoutine());
    }

    IEnumerator InteractableAnimationRoutine()
    {
        if (!isServed)
        {
            int degree = Random.Range(10, 30);
            if (transform.rotation.eulerAngles.z is > 0 and < 300) degree = -degree;
            transform.rotation = Quaternion.Euler(0, 0, degree);
        }
        
        yield return new WaitForSeconds(1f);
        StartInteractableAnim();
    }
}
