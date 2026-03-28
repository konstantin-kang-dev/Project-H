using System;
using UnityEngine;
using UnityEngine.UI;

public class ContactsUI : BasicCustomWindow
{
    [SerializeField] Button _backBtn;

    [SerializeField] Button _linkedInBtn;
    [SerializeField] Button _githubBtn;
    [SerializeField] Button _telegramBtn;

    protected override void BindControls()
    {
        base.BindControls();

        _backBtn.onClick.AddListener(WindowsNavigator.Instance.GoBack);

        _linkedInBtn.onClick.AddListener(HandleClickLinkedInBtn);
        _githubBtn.onClick.AddListener(HandleClickGithubBtn);
        _telegramBtn.onClick.AddListener(HandleClickTelegramBtn);
    }

    protected override void UnbindControls()
    {
        base.UnbindControls();

        _backBtn.onClick.RemoveListener(WindowsNavigator.Instance.GoBack);

        _linkedInBtn.onClick.RemoveListener(HandleClickLinkedInBtn);
        _githubBtn.onClick.RemoveListener(HandleClickGithubBtn);
        _telegramBtn.onClick.RemoveListener(HandleClickTelegramBtn);
    }

    void HandleClickLinkedInBtn()
    {
        Application.OpenURL(ProjectConstants.CONTACTS_LINKEDIN_URL);
    }

    void HandleClickGithubBtn()
    {
        Application.OpenURL(ProjectConstants.CONTACTS_GITHUB_URL);
    }

    void HandleClickTelegramBtn()
    {
        Application.OpenURL(ProjectConstants.CONTACTS_TELEGRAM_URL);
    }
}
