using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseInteractableLogic : MonoBehaviour
{
    public abstract string interactiontName { get; }
    public abstract bool isInteractable { get; }

    public abstract void onHoverStart();

    public abstract void onHoverEnd();

    public abstract void onInteraction();
}
