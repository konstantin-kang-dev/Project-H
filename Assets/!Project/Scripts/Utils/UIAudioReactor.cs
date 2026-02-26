using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using GameAudio;

public class UIAudioReactor : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] bool _doReact = true;
    [SerializeField] SoundType _clickSoundType;
    [SerializeField] SoundType _hoverSoundType;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_doReact) return;
        GlobalAudioManager.Instance.Play(_clickSoundType);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_doReact) return;
        GlobalAudioManager.Instance.Play(_hoverSoundType);
    }

}