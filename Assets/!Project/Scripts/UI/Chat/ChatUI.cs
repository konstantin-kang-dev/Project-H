using Cysharp.Threading.Tasks;
using DG.Tweening;
using ModestTree;
using Sirenix.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatMessageUI _chatMessagePrefab;
    [SerializeField] ScrollRect _scroll;
    [SerializeField] RectTransform _container;
    [SerializeField] TMP_InputField _chatInput;

    List<ChatMessageUI> _chatMessagesUI = new List<ChatMessageUI>();

    bool _isChatOpened = false;

    Tween _scrollAnim;

    [field: SerializeField] public bool IsInitialized { get; private set; } = false;
    public void Init()
    {
        ChatManager.Instance.OnChatMessageAdded += HandleChatMessageAdd;
        _chatInput.onSubmit.AddListener(HandleChatInputSend);

        GlobalInputManager.Input.OnOpenChat += HandleOpenChat;

        IsInitialized = true;
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
        if (input.IsEmpty() || input.IsNullOrWhitespace()) return;

        ChatManager.Instance.RPC_RequestSendMessage(input, ChatMessageType.PlayerMessage, NetworkGameManager.Instance.NetworkManager.ClientManager.Connection);
        _chatInput.text = "";
        _chatInput.Select();
    }

    void HandleOpenChat()
    {
        if (_isChatOpened) return;

        _chatInput.Select();
        _chatInput.ActivateInputField();
    }

    private void OnDestroy()
    {
        ChatManager.Instance.OnChatMessageAdded -= HandleChatMessageAdd;
        _chatInput.onSubmit.RemoveListener(HandleChatInputSend);

        GlobalInputManager.Input.OnOpenChat -= HandleOpenChat;

    }
}
