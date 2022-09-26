using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace TB_Tool
{
//이 클래스는 키 리바인딩UI의 실제 컨트롤과 저장을 담당합니다. 
//주된 동작은 키 리바인딩 대상 오브젝트의 "PlayInput" 컴포넌트 정보를 토대로 생성해둔 액션과 리바인딩 된 키 입력을 매치시키는 것을 담당합니다.

    public class RebindingUIController : MonoBehaviour
    {
        [Tooltip("생성한 액션의 리스트 입니다.")]
        public List<InputActionReference> Actions;

        [Tooltip("'KeyRebindInfo' 클래스를 어태치한 게임 오브젝트 중, 이동관련 오브젝트(상/하/좌/우)의 리스트 입니다.")]
        //컨트롤러용 바인딩을 생성한 경우에는 "MoveSections_controller" 같은 변수명으로 리스트를 추가해서 사용해 주세요.
        //편의에 따라서 "MoveSections"과 "ButtonSections"을 구분했지만, 통합한 리스트를 생성하셔도 상관없습니다.
        public List<TB_Tool.KeyRebindInfo> MoveSections;

        [Tooltip("'KeyRebindInfo' 클래스를 어태치한 게임 오브젝트 중, 일반 버튼(키)의 리스트 입니다.")]
        //컨트롤러용 바인딩을 생성한 경우에는 "ButtonSections_controller" 같은 변수명으로 리스트를 추가해서 사용해 주세요.
        //편의에 따라서 "MoveSections"과 "ButtonSections"을 구분했지만, 통합한 리스트를 생성하셔도 상관없습니다.
        public List<TB_Tool.KeyRebindInfo> ButtonSections;

        [Tooltip("세이브 버튼입니다.")]
        public Button saveButton;

        //키 리바인딩과 관련된 모든 버튼의 리스트 입니다.
        //"MoveSections", "ButtonSections" 및 추가 오브젝트에서 "Button" 컴포넌트를 참조시킵니다.
        [SerializeField]private List<Button> rebindButtons;
        
        [Tooltip("리바인딩 된 키를 반영할 대상이될 타겟 오브젝트입니다.")]
        // 보통 주인공 캐릭터를 등록하시면 됩니다. 
        //단, 반드시 주인공 캐릭터에게 'PlayerInput' 컴포넌트가 어태치 되어 있어야 합니다."
        [SerializeField] private CubeController cubeController = null;
        //해당 샘플에는 플레이어 컨트롤러 클래스 상의 "PlayerInput"을 참조하도록 했지만, 
        //그냥 아래코드처럼 "PlayerInput"을 다이렉트로 참조해도 됩니다.
        //[SerializeField] private PlayerInput targetPlayerInput = null;

        //리바인딩 오퍼레이션을 실행할 변수입니다.
        private InputActionRebindingExtensions.RebindingOperation rebindingOperation = null;

        //저장 시 세이브 파일 이름입니다.
        private const string RebindsKey = "rebinds";

        //저장된 바인딩 데이터를 받아와서 적용하고 UI정보를 갱신합니다.
        private void Start()
        {   
            string rebinds = PlayerPrefs.GetString(RebindsKey, string.Empty);

            if (string.IsNullOrEmpty(rebinds)) { return; }

            cubeController.PlayerInput.actions.LoadBindingOverridesFromJson(rebinds);

            foreach(var binding in Actions)
            {
                if(binding.action.name == "Move")
                {
                    for (int i = 0; i < binding.action.controls.Count; i++)
                    {
                        int bindingIndex = binding.action.GetBindingIndexForControl(binding.action.controls[i]);
                        MoveSections[i].bindingDisplayNameText.text = InputControlPath.ToHumanReadableString(
                        binding.action.bindings[bindingIndex].effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice);
                    }
                }
                
                else if(binding.action.name == "Attack")
                {
                    for (int i = 0; i < binding.action.controls.Count; i++)
                    {
                        int bindingIndex = binding.action.GetBindingIndexForControl(binding.action.controls[i]);
                        ButtonSections[i].bindingDisplayNameText.text = InputControlPath.ToHumanReadableString(
                        binding.action.bindings[bindingIndex].effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice);
                    }
                }
            }

            //"MoveSections", "ButtonSections"오브 젝트에서 "Button" 컴포넌트를 리스트화 합니다.
            foreach(var b in MoveSections)
            {
                rebindButtons.Add(b.StartRebindObj.GetComponent<Button>());
            }
            foreach(var b in ButtonSections)
            {
                rebindButtons.Add(b.StartRebindObj.GetComponent<Button>());
            }

            //세이브 버튼을 리스트에 추가합니다.
            rebindButtons.Add(saveButton);

            //rebindButtons List를 기준으로 가장 위의 버튼(0)을 제외한 나머지 버튼을 비활성화 합니다.
            for(int i = 0; i < rebindButtons.Count; i++)
            {
                if(i == 0)
                {
                    rebindButtons[i].interactable = true;
                }
                else
                {
                    rebindButtons[i].interactable = false;
                }
            }
        }

        
        //현재 바인딩 상태를 저장합니다.
        public void Save()
        {
            string rebinds = cubeController.PlayerInput.actions.SaveBindingOverridesAsJson();

            PlayerPrefs.SetString(RebindsKey, rebinds);
        }

        //리바인딩을 시작합니다.
        //리바인딩 시 UI를 처리하고, 리바인딩 키 입력 대기 상태로 변경합니다. 
        //현재의 액션맵을 강제로 변경(샘플에서는 "MenuControl")하여 더이상 플레이어가 안움직이도록 처리합니다.
        //키 입력이 완료되면 람다식으로 "RebindComplete"를 호출합니다. 
        public void StartRebinding(KeyRebindInfo section)
        {
            section.StartRebindObj.SetActive(false);
            section.waitingForInputObj.SetActive(true);

            cubeController.PlayerInput.SwitchCurrentActionMap("MenuControl");

            rebindingOperation = Actions[section.rebindActionIndex].action.PerformInteractiveRebinding(section.rebindBindingIndex)
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(oerration => RebindComplete(section))
                .Start();
        }

        //리바인딩을 완료합니다.
        //입력된 키를 받아와 Text로 표시하고 액션뱁을 다시 원래대로 변경(샘플에서는 "CubeControl")로 변경합니다.
        private void RebindComplete(KeyRebindInfo section)
        {
            int bindingIndex = Actions[section.rebindActionIndex].action.GetBindingIndexForControl(Actions[section.rebindActionIndex].action.controls[section.rebindControlIndex]);

            section.bindingDisplayNameText.text = InputControlPath.ToHumanReadableString(
                Actions[section.rebindActionIndex].action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice 
            );
            
            rebindingOperation.Dispose();

            section.StartRebindObj.SetActive(true);
            section.waitingForInputObj.SetActive(false);

            cubeController.PlayerInput.SwitchCurrentActionMap("CubeControl");
        }
    }
}