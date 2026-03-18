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

    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] float _autoOpenChatDuration = 2f;

    List<ChatMessageUI> _chatMessagesUI = new List<ChatMessageUI>();

    bool _isChatOpened = false;
    float _autoOpenChatTimer = 0f;

    Tween _scrollAnim;

    [field: SerializeField] public bool IsInitialized { get; private set; } = false;
    public void Init()
    {
        ChatManager.Instance.OnChatMessageAdded += HandleChatMessageAdd;
        _chatInput.onSubmit.AddListener(HandleChatInputSend);
        _chatInput.onSelect.AddListener(HandleOpenChat);
        _chatInput.onDeselect.AddListener(HandleCloseChat);
        _chatInput.onValueChanged.AddListener(HandleTextInput);

        GlobalInputManager.Input.OnOpenChat += HandleOpenChatPressed;
        NetworkGameManager.Instance.OnLocalClientConnected += Clear;
        _canvasGroup.alpha = 0.1f;

        IsInitialized = true;
    }
    private void OnDestroy()
    {
        ChatManager.Instance.OnChatMessageAdded -= HandleChatMessageAdd;

        _chatInput.onSubmit.RemoveListener(HandleChatInputSend);
        _chatInput.onSelect.RemoveListener(HandleOpenChat);
        _chatInput.onDeselect.RemoveListener(HandleCloseChat);
        _chatInput.onValueChanged.RemoveListener(HandleTextInput);

        GlobalInputManager.Input.OnOpenChat -= HandleOpenChatPressed;

        NetworkGameManager.Instance.OnLocalClientConnected -= Clear;
    }

    private void FixedUpdate()
    {
        if(_autoOpenChatTimer > 0)
        {
            _autoOpenChatTimer -= Time.fixedDeltaTime;
        }
        else
        {
            HandleCloseChat();
        }
    }

    void HandleTextInput(string input)
    {
        UpdateAutoOpenChatTimer();
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

        HandleOpenChat();
    }

    void HandleChatInputSend(string input)
    {
        if (input.IsEmpty() || input.IsNullOrWhitespace()) return;

        ChatManager.Instance.RPC_RequestSendMessage(input, ChatMessageType.PlayerMessage, NetworkGameManager.Instance.NetworkManager.ClientManager.Connection);
        _chatInput.text = "";
        _chatInput.Select();
    }

    void HandleOpenChatPressed()
    {
        _chatInput.Select();
        _chatInput.ActivateInputField();

        HandleOpenChat();
    }

    void HandleOpenChat(string value = "")
    {
        if (_isChatOpened) return;

        UpdateAutoOpenChatTimer();
        _canvasGroup.DOFade(1f, 0.2f);
        _isChatOpened = true;

        GlobalInputManager.Input.SetLock(true);
    }

    void HandleCloseChat(string value = "")
    {
        if (!_isChatOpened) return;

        _autoOpenChatTimer = 0;
        _canvasGroup.DOFade(0.1f, 0.2f);
        _isChatOpened = false;

        GlobalInputManager.Input.SetLock(false);
    }

    void UpdateAutoOpenChatTimer()
    {
        _autoOpenChatTimer = _autoOpenChatDuration;
    }

    public void Clear()
    {
        foreach (var chatMessage in _chatMessagesUI)
        {
            if (chatMessage == null) continue;
            Destroy(chatMessage.gameObject);
        }

        _chatMessagesUI.Clear();
    }

}
