using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CubeController : MonoBehaviour
{
    [Tooltip("플레이어의 'PlayerInput' 컴포넌트 입니다.")]
    [SerializeField] private PlayerInput playerInput = null;

    [Tooltip("설정이 완료 된 'RebindingUIController' 입니다.")]
    [SerializeField] private TB_Tool.RebindingUIController rebindingUIController = null;
    
    //키 리바인딩 UI의 상테체크 변수.
    private bool isKeyConfigUI;

    [Tooltip("이동속도 입니다.")]
    public float moveSpeed = 4f;

    //현재 선택된 버튼의 List 번호 변수.
    private int selectButtonNumber = 0;

    //실 이동용 Vector3 변수.
    private Vector3 moveDirection;

    //외부 참조용 PlayerInput 인스턴스.
    public PlayerInput PlayerInput => playerInput;

    //실저 이동처리.
    void Update()
    { 
        bool hasControl = (moveDirection != Vector3.zero);
        if(hasControl)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

    //액션 "Move" 콜백, Vector2
    //"isKeyConfigUI"가 비활성화일 때만 인풋받음.
    void OnMove(InputValue value)
    {
        if(!isKeyConfigUI)
        {
            Vector2 input = value.Get<Vector2>();
            if(input != null)
            {
                moveDirection = new Vector3(input.x, input.y, 0f);
                Debug.Log($"SEND_MESSAGE : {input.magnitude}");
            }
        }
    }

    //액션 "Attack" 콜백, 버튼.
    void OnAttack()
    {
        Debug.Log("Attack!!");
    }

    //액션 "Menu" 콜백, 버튼.
    //"isKeyConfigUI"의 On/Off를 컨트롤 합니다.
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

    //액션 "Confirm" 콜백, 버튼.
    //"rebindingUIController.keyBindingButtons" 리스트에서 "selectButtonNumber"에 해당하는 버튼의 onClick()를 호출.
    void OnConfirm()
    {
        Button selectdButton = rebindingUIController.keyBindingButtons[selectButtonNumber];
        bool isActive = selectdButton.gameObject.activeSelf;
        if(isActive) selectdButton.onClick.Invoke();
    }

     //액션 "UI_control" 콜백, Vector2.
    void OnUI_control(InputValue value)
    {
         Vector2 input = value.Get<Vector2>();

        //"isKeyConfigUI"가 On 되었을 때는 UI를 컨트롤하고, 반대일 때는 캐릭터를 컨트롤한다.
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
