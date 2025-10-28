using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickableCharacter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] GameManager gm;
    [SerializeField] int characterIndex;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!gm.canServe) return;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!gm.canServe) return;
        
        gm.ServeCharacter(characterIndex);
    }
}
