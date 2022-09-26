using UnityEngine;
using UnityEngine.UI;

namespace TB_Tool
{
//이 클래스는 키 리바인드 시의 정보를 담게되는 클래스로써 키변경 버튼이나 기타 다른 오브젝트에 어태치하여 사용하면 됩니다. 
//"KeyREbindInfo"가 어태치 된 오브젝트는 "RebindingUIController"의 "MoveSections" 혹은 "ButtonSections"에 등록해야 합니다.
    public class KeyRebindInfo : MonoBehaviour
    {
        [Tooltip("해당버튼의 액션 인덱스 입니다.")]
        public int rebindActionIndex;

        [Tooltip("해당버튼의 컨트롤 인덱스 입니다.")]
        public int rebindControlIndex;
    
        [Tooltip("해당버튼의 바인딩 인덱스 입니다. 바인딩 인덱스가 복수일때는 1부터 차례대로 입력하고, 하나일때는 -1을 입력합니다.")]
        public int rebindBindingIndex = -1;

        [Tooltip("바인딩 텍스트 표시용 텍스트 입니다.")]
        public Text bindingDisplayNameText;
        
        [Tooltip("바인딩 시작 오브젝트 입니다. 보통 버튼을 등록합니다.")]
        public GameObject StartRebindObj;

        [Tooltip("리바인딩 키 입력 대기시 표시할 오브젝트입니다.")]
        public GameObject waitingForInputObj;
    }
}
