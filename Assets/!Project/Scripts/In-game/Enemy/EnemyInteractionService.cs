using UnityEngine;

public class EnemyInteractionService : MonoBehaviour
{
    [SerializeField] float _interactionRange = 1f;
    [SerializeField] LayerMask _interactionLayers;
    public bool IsInitialized { get; private set; } = false;
    public void Init()
    {

        IsInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!IsInitialized) return;

        DetectInteractablesInFront();
    }

    void DetectInteractablesInFront()
    {
        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _interactionRange, _interactionLayers))
        {
            Debug.DrawRay(transform.position, transform.forward, Color.blue);
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) Interact(interactable);
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward, Color.purple);
        }
    }

    void Interact(IInteractable interactable)
    {
        switch (interactable.Config.Type)
        {
            case InteractableType.Door:
                if (!interactable.InteractionState)
                {
                    interactable.Interact(null);
                }
                break;
        }
    }
}
