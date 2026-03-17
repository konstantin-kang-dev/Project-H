using Cysharp.Threading.Tasks;
using Saves;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsHintsUI : MonoBehaviour
{
    [SerializeField] List<RectTransform> _layouts = new List<RectTransform>();
    [SerializeField] TextMeshProUGUI _mapKeyInteractionTMP;
    [SerializeField] TextMeshProUGUI _mapKeySprintTMP;
    [SerializeField] TextMeshProUGUI _mapKeyCrouchTMP;

    private void Awake()
    {

    }
    public async void Init(GameSave gameSave)
    {
        ControlsSave controlsSave = gameSave.SettingsSave.ControlsSave;
        _mapKeyInteractionTMP.text = $"[{controlsSave.InteractBind}]";
        _mapKeySprintTMP.text = $"[{controlsSave.SprintBind}]";
        _mapKeyCrouchTMP.text = $"[{controlsSave.CrouchBind}]";

        await UniTask.WaitForEndOfFrame();

        foreach (var layout in _layouts)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
        }
    }

    private void OnEnable()
    {
        SaveManager.OnSaveUpdated += Init;
        Init(SaveManager.GameSave);
    }

    private void OnDisable()
    {
        SaveManager.OnSaveUpdated -= Init;
    }
}
