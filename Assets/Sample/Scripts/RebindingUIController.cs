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
        [Tooltip("리바인딩 대상액션의 리스트 입니다.")]
        public List<InputActionReference> Actions;

        [Tooltip("'KeyRebindInfo' 클래스를 어태치한 게임 오브젝트 중, 이동관련 오브젝트(상/하/좌/우)의 리스트 입니다.")]
        //컨트롤러용 바인딩을 생성한 경우에는 "MoveSections_controller" 같은 변수명으로 리스트를 추가해서 사용해 주세요.
        //편의에 따라서 "MoveSections"과 "ButtonSections"을 구분했지만, 통합한 리스트를 생성하셔도 상관없습니다.
        public List<TB_Tool.KeyRebindInfo> MoveSections;

        [Tooltip("'KeyRebindInfo' 클래스를 어태치한 게임 오브젝트 중, 일반 버튼(키)의 리스트 입니다.")]
        //컨트롤러용 바인딩을 생성한 경우에는 "ButtonSections_controller" 같은 변수명으로 리스트를 추가해서 사용해 주세요.
        //편의에 따라서 "MoveSections"과 "ButtonSections"을 구분했지만, 통합한 리스트를 생성하셔도 상관없습니다.
        public List<TB_Tool.KeyRebindInfo> ButtonSections;

        [Tooltip("키 바인딩 UI 오브젝트 입니다.")]
        public GameObject keyBindingUI;

        //키 바인딩 UI 메뉴 셀렉트용 게임 오브젝트(현재 선택된 메뉴 표시용 체크박스) 리스트 입니다.
        //오직 키 입력으로 메뉴를 선택하기 위한 리스트 이므로 마우스만 사용할꺼라면 주석처리해도 무방합니다.
        //"keyBindingUI"의 자식 오브젝트를 참조하기 때문에 반드시 "keyBindingUI"이 설정되어 있어야 합니다.
        [HideInInspector] 
        public List<GameObject> selectObjects;

        //키 바인딩 버튼 리스트 입니다.
        //오직 키 입력으로 메뉴를 선택하기 위한 리스트 이므로 마우스만 사용할꺼라면 주석처리해도 무방합니다.
        //"keyBindingUI"의 자식 오브젝트를 참조하기 때문에 반드시 "keyBindingUI"이 설정되어 있어야 합니다.
        [HideInInspector] 
        public List<Button> keyBindingButtons;

        [Tooltip("리바인딩 된 키를 반영할 대상이될 타겟 오브젝트입니다.")]
        //"PlayerInput" 컴포넌트가 어태치 되어 있는 게임 오브젝트를 지정합니다.(보통 플레이어 캐릭터를 지정)"
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
            //데이터 초기화용.
            //PlayerPrefs.DeleteAll();

            //세이브 파일이 없다면 세이브파일을 생성합니다.
            if(!PlayerPrefs.HasKey(RebindsKey))
            {
                MakeSave();
            }

            SetUI();

            //"keyBindingUI"의 자식 오브젝트들에서 키 바인딩 UI의 셀렉트 표시용 오브젝트를 리스트업 합니다.
            for(int i = 0; i < keyBindingUI.transform.childCount; i++ )
            {
                selectObjects.Add(keyBindingUI.transform.GetChild(i).Find("Select").gameObject);
            }

            //keyBindingUI"의 자식 오브젝트들에서 키 바인딩 버튼들을 리스트업 합니다.
            for(int i = 0; i < keyBindingUI.transform.childCount; i++ )
            {
                keyBindingButtons.Add(keyBindingUI.transform.GetChild(i).Find("Button").GetComponent<Button>());
            }

        }

        //현재의 바인딩 데이터를 참조하여 UI에 반영시킵니다.
        private void SetUI()
        {
             //세이브 데이터 불러오기.
             string rebinds = PlayerPrefs.GetString(RebindsKey, string.Empty);

            //"rebinds"의 스트링이 비어 있으면 리턴.
            if (string.IsNullOrEmpty(rebinds)) { return; }

            //불러 온 데이터의 바인딩 정보를 타겟에 적용한다.
            cubeController.PlayerInput.actions.LoadBindingOverridesFromJson(rebinds);

            //"불러 온 바인딩 데이터를 토대로 UI를 갱힌한다.
            //예제에서는 "Actions" 리스트에서 바인딩 대싱이 되는 액션들을 직접 지정했습니다.
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
        }

        //rebindButtons List를 기준으로 지정한 번호(num)의 선택 오브젝트만 활성화 시킵니다.
        public void buttonUIControl(int num)
        {
            for(int i = 0; i < selectObjects.Count; i++)
            {
                if(i == num)
                {
                    selectObjects[i].SetActive(true);
                }
                else
                {
                    selectObjects[i].SetActive(false);
                }
            }
        }
        
        //현재 바인딩 상태를 저장합니다.
        public void Save()
        {
            string rebinds = cubeController.PlayerInput.actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(RebindsKey, rebinds);
        }

        //바인딩을 초기 설정으로 변경합니다.
        public void Reset()
        {
            MakeSave();
            SetUI();
        }


        //초기 세이브 파일을 생성합니다.
        //샘플에서는 "Actions"리스트의 바인딩들을 모조리 수동으로 등록해서 Path를 변경했지만, 좀더 이쁘게 다듬어서 사용할 수 있다면, 넌 천재!
        private void MakeSave()
        {
            Actions[0].action.ApplyBindingOverride(new InputBinding { path = Actions[0].action.bindings[1].path, overridePath = "<Keyboard>/w"});
            Actions[0].action.ApplyBindingOverride(new InputBinding { path = Actions[0].action.bindings[2].path, overridePath = "<Keyboard>/s"});
            Actions[0].action.ApplyBindingOverride(new InputBinding { path = Actions[0].action.bindings[3].path, overridePath = "<Keyboard>/a"});
            Actions[0].action.ApplyBindingOverride(new InputBinding { path = Actions[0].action.bindings[4].path, overridePath = "<Keyboard>/d"});
            Actions[1].action.ApplyBindingOverride(new InputBinding { path = Actions[1].action.bindings[0].path, overridePath = "<Keyboard>/space"});
            
            //수동으로 초기화한 바인딩 데이터를 저장합니다.
            Save();
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

            //실질적으로 키입력 대기상태로 바꾸는 부분! ".WithControlsExcluding()"로 입력을 무시할 패스들을 등록할 수 있습니다.
            rebindingOperation = Actions[section.rebindActionIndex].action.PerformInteractiveRebinding(section.rebindBindingIndex)
                .WithControlsExcluding("Mouse")
                .WithControlsExcluding("<Gamepad>/Start")
                .WithControlsExcluding("<Keyboard>/escape")
                .WithControlsExcluding("<Keyboard>/upArrow")
                .WithControlsExcluding("<Keyboard>/downArrow")
                .WithControlsExcluding("<Keyboard>/leftArrow")
                .WithControlsExcluding("<Keyboard>/rightArrow")
                .WithControlsExcluding("<Keyboard>/enter")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(oerration => RebindComplete(section))
                .Start();
        }

        //리바인딩을 완료합니다.
        //입력된 키를 받아와 Text로 표시하고 액션뱁을 다시 원래대로 변경(샘플에서는 "CubeControl")로 변경합니다.
        private void RebindComplete(KeyRebindInfo section)
        {
            int bindingIndex = Actions[section.rebindActionIndex].action.GetBindingIndexForControl(Actions[section.rebindActionIndex].action.controls[section.rebindControlIndex]);

            //입력한 path를 버튼의 Text에 표시합니다.
            section.bindingDisplayNameText.text = InputControlPath.ToHumanReadableString(
                Actions[section.rebindActionIndex].action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice 
            );
            
            //리바인딩 오퍼레이션을 종료합니다.
            rebindingOperation.Dispose();

            section.StartRebindObj.SetActive(true);
            section.waitingForInputObj.SetActive(false);

            cubeController.PlayerInput.SwitchCurrentActionMap("CubeControl");
        }
    }
}