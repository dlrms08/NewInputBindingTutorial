using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CubeController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput = null;
    [SerializeField] private TB_Tool.RebindingUIController rebindingUIController = null;
    private bool isKeyConfigUI;
    public float moveSpeed = 4f;
    private int selectButtonNumber = 0;
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
        Debug.Log("Attack!!");
    }

    void OnMenu()
    {
        Debug.Log("Menu!!");
        isKeyConfigUI = rebindingUIController.keyBindingUI.activeSelf;
        isKeyConfigUI = !isKeyConfigUI;
        rebindingUIController.keyBindingUI.SetActive(isKeyConfigUI);
        if(isKeyConfigUI)
        {
            selectButtonNumber = 0;
            rebindingUIController.buttonUIControl(selectButtonNumber);
        }
    }

    void OnConfirm()
    {
        Button selectdButton = rebindingUIController.keyBindingButtons[selectButtonNumber];
        bool isActive = selectdButton.gameObject.activeSelf;
        if(isActive) selectdButton.onClick.Invoke();
    }

    void OnUI_control(InputValue value)
    {
         Vector2 input = value.Get<Vector2>();
        if(isKeyConfigUI)
        {
            selectButtonNumber -= (int)input.y;
            if(selectButtonNumber < 0)
                selectButtonNumber = 0;

            if(selectButtonNumber > rebindingUIController.selectObjects.Count - 1)
                selectButtonNumber = rebindingUIController.selectObjects.Count - 1;

            rebindingUIController.buttonUIControl(selectButtonNumber);
        }
        else
        {
            if(input != null)
            {
                moveDirection = new Vector3(input.x, input.y, 0f);
                Debug.Log($"SEND_MESSAGE : {input.magnitude}");
            }
        }

    }
}
