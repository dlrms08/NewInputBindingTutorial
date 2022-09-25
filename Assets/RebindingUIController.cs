using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RebindingUIController : MonoBehaviour
{
    public List<InputActionReference> Actions;
    public List<KeySettingSection> Sections;
    [SerializeField] private CubeController cubeController = null;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation = null;
    private const string RebindsKey = "rebinds";

    private void Start()
    {   
        string rebinds = PlayerPrefs.GetString(RebindsKey, string.Empty);

        if (string.IsNullOrEmpty(rebinds)) { return; }

        cubeController.PlayerInput.actions.LoadBindingOverridesFromJson(rebinds);
        
        int bindingIndex = Actions[1].action.GetBindingIndexForControl(Actions[1].action.controls[0]);

        Sections[4].bindingDisplayNameText.text = InputControlPath.ToHumanReadableString(
            Actions[1].action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
    
    public void Save()
    {
        string rebinds = cubeController.PlayerInput.actions.SaveBindingOverridesAsJson();

        PlayerPrefs.SetString(RebindsKey, rebinds);
    }
    
    public void StartRebinding(KeySettingSection section)
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

    private void RebindComplete(KeySettingSection section)
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