using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer = 1;

    public bool IsGrounded { get; private set; }

    public event System.Action OnGrounded;
    public event System.Action OnLeftGround;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            if (!IsGrounded)
            {
                IsGrounded = true;
                OnGrounded?.Invoke();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            if (IsGrounded)
            {
                IsGrounded = false;
                OnLeftGround?.Invoke();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            if (!IsGrounded)
            {
                IsGrounded = true;
                OnGrounded?.Invoke();
            }
        }
    }
}