using UnityEngine;
using UnityEngine.InputSystem;

public class CubeController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput = null;
    public float moveSpeed = 4f;
    private Vector3 moveDirection;

    public PlayerInput PlayerInput => playerInput;

    void Update()
    { 
        bool hasControl = (moveDirection != Vector3.zero);
        if(hasControl)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        if(input != null)
        {
            moveDirection = new Vector3(input.x, input.y, 0f);
            Debug.Log($"SEND_MESSAGE : {input.magnitude}");
        }
    }

    void OnAttack()
    {
        //var input = value;
        //if(input != null)
            Debug.Log("Attack!!");
    }
}
