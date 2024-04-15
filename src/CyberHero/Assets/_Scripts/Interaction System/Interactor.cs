using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionPointRadius;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private InteractionPromptUI interactionPromptUI;

    private readonly Collider2D[] colliders = new Collider2D[3];
    [SerializeField] private int numFound;

    private IInteractable interactable;

    private void Update()
    {
        numFound = Physics2D.OverlapCircleNonAlloc(interactionPoint.position, interactionPointRadius, colliders, interactableMask);

        if (numFound > 0 )
        {
            interactable = colliders[0].GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (!interactionPromptUI.IsDisplayed)
                {
                    interactionPromptUI.SetUp(interactable.InteractionPrompt);
                }

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    interactable.Interact(this);
                }
            }

        }
        else
        {
            if (interactable != null)
            {
                interactable = null;
            }

            if (interactionPromptUI.IsDisplayed)
            {
                interactionPromptUI.Close();
            }
        }
    }

}
