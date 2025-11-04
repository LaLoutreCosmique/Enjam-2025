using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickableCharacter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    SpriteRenderer renderer;
    
    [SerializeField] GameManager gm;
    [SerializeField] public int characterIndex { get; private set; }

    private bool isServed;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!gm.hasServiceStarted || isServed) return;
        gm.ToggleCharacter(characterIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
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
    }

    public void Deselect()
    {
        renderer.color = new Color(1f, 1f, 1f, 1f);
        isServed = false;
    }
}
