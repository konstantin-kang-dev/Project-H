using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatMessageUI _chatMessagePrefab;
    [SerializeField] ScrollRect _scroll;
    [SerializeField] RectTransform _container;
    [SerializeField] TMP_InputField _chatInput;

    List<ChatMessageUI> _chatMessagesUI = new List<ChatMessageUI>();

    Tween _scrollAnim;
    public void Init()
    {
        for (int i = 0; i < _container.childCount; i++)
        {
            Destroy(_container.GetChild(i).gameObject);
        }

        _chatMessagesUI.Clear();

        ChatManager.Instance.OnChatMessageAdded += HandleChatMessageAdd;
        _chatInput.onSubmit.AddListener(HandleChatInputSend);
    }

    async void HandleChatMessageAdd(ChatMessageData chatMessageData)
    {
        ChatMessageUI chatMessageUI = Instantiate(_chatMessagePrefab, _container);

        chatMessageUI.Init(chatMessageData);

        _chatMessagesUI.Add(chatMessageUI);

        chatMessageUI.SetVisibility(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_container);

        if (_scrollAnim != null)
        {
            _scrollAnim.Kill();
        }
        _scrollAnim = _scroll.DOVerticalNormalizedPos(0f, 0.5f);
    }

    void HandleChatInputSend(string input)
    {
        ChatManager.Instance.RPC_RequestSendMessage(input);
        _chatInput.text = "";
    }

    private void OnDestroy()
    {
        ChatManager.Instance.OnChatMessageAdded -= HandleChatMessageAdd;
        _chatInput.onSubmit.RemoveListener(HandleChatInputSend);
    }
}
