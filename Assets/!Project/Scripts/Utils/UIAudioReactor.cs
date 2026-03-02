using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using GameAudio;
using UnityEngine.UI;

public class UIAudioReactor : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] bool _doReact = true;
    [SerializeField] SoundType _clickSoundType;
    [SerializeField] SoundType _hoverSoundType;

    Button _btn;

    void Awake()
    {
        _btn = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_doReact) return;
        if (_btn != null && !_btn.interactable) return;

        GlobalAudioManager.Instance.Play(_clickSoundType);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_doReact) return; 
        if (_btn != null && !_btn.interactable) return;

        GlobalAudioManager.Instance.Play(_hoverSoundType);
    }

}