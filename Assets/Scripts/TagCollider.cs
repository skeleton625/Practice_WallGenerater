using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TagCollider : MonoBehaviour
{
    [Header("Conflict Setting")]
    [SerializeField] private string TagName = "";
    [SerializeField] private string ExceptName = "";
    [SerializeField] private UnityEvent StayEvent = null;
    [SerializeField] private UnityEvent ExitEvent = null;

    private Collider tagCollider = null;

    public bool IsConflict { get; private set; }

    private void Awake()
    {
        tagCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        Debug.Log("Enable");
        tagCollider.enabled = true;
    }

    private void OnDisable()
    {
        Debug.Log("Disable");
        IsConflict = false;
        tagCollider.enabled = false;

        ExitEvent.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(TagName) && !(IsConflict || other.CompareTag(ExceptName)))
        {
            IsConflict = true;
            StayEvent.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(TagName) && IsConflict)
        {
            IsConflict = false;
            ExitEvent.Invoke();
        }
    }
}
