using Coffee.UIEffects;
using DG.Tweening;
using GameAudio;
using UnityEngine;

public class LocationTitleUI : MonoBehaviour
{
    [SerializeField] UIEffectTweener _uiEffectTweener;

    public void Init()
    {
        _uiEffectTweener.Play();
        GlobalAudioManager.Instance.Play(SoundType.UILocationIntro);
    }

}
