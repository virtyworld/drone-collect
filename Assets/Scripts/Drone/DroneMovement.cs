using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float moveSpeed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize target position if needed
        if (targetPosition == Vector3.zero)
        {
            targetPosition = transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Move towards target position
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    // Method to set new target position
    public void SetTargetPosition(Vector3 newTarget)
    {
        targetPosition = newTarget;
    }
}
