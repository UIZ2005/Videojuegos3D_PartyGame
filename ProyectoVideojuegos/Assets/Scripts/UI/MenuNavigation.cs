using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MenuNavigation : MonoBehaviour
{
    [Header("Boton inicial del panel")]
    public GameObject firstSelectedButton;

    [Header("Seleccion dinamica")]
    public bool IsInteractable;

    private Coroutine selectionCoroutine;

    private void OnEnable()
    {
        selectionCoroutine = StartCoroutine(SelectFirstButton());
    }

    private void OnDisable()
    {
        if (selectionCoroutine != null)
        {
            StopCoroutine(selectionCoroutine);
            selectionCoroutine = null;
        }
    }

    private IEnumerator SelectFirstButton()
    {
        while(true)
        {
            yield return null;

            if (EventSystem.current == null)
            {
                continue;
            }

            if (firstSelectedButton == null)
            {
                yield break;
            }

            if (!firstSelectedButton.activeInHierarchy)
            {
                continue;
            }

            Selectable selectable = firstSelectedButton.GetComponent<Selectable>();

            if (selectable == null)
            {
                yield break;
            }

            if (!selectable.interactable)
            {
                if (IsInteractable)
                {
                    continue;
                }

                yield break;
            }

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            yield break;
        }
    }
}