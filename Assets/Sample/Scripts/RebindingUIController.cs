using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RebindingUIController : MonoBehaviour
{
    public List<InputActionReference> Actions;
    public List<KeyRebindInfo> MoveSections;
    public List<KeyRebindInfo> ButtonSections;
    [SerializeField] private CubeController cubeController = null;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation = null;
    private const string RebindsKey = "rebinds";

    private void Start()
    {   
        string rebinds = PlayerPrefs.GetString(RebindsKey, string.Empty);

        if (string.IsNullOrEmpty(rebinds)) { return; }

        cubeController.PlayerInput.actions.LoadBindingOverridesFromJson(rebinds);

        foreach(var binding in Actions)
        {
            Debug.Log(binding.action.name);
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
    
    public void Save()
    {
        string rebinds = cubeController.PlayerInput.actions.SaveBindingOverridesAsJson();

        PlayerPrefs.SetString(RebindsKey, rebinds);
    }
    
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