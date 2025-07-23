// IInteractable.cs (Revised)
using UnityEngine;

public interface IInteractable
{
    InteractionType InteractionType { get; }
    void Interact(GameObject initiatorGameObject); // Changed parameter
}