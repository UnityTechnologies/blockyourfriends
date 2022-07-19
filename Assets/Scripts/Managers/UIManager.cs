using System;
using UnityEngine;
using BlockYourFriends.UI;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    public static Action<string> PlayerNameChanged;
    public static Action PaddleChanged;

    [Header("Popups")]
    [SerializeField] private UIActiveState m_PopupWaitingForPlayers;
    [SerializeField] private ScoreToCoinsPopup m_ScoreToCoinsPopup;
    [SerializeField] private UIActiveState m_InGameOptionsPopup;
    [SerializeField] private UIActiveState m_AndroidMicPermissionPopup;

    [Space(20)]
    [Header("Backgrounds")]
    [SerializeField] SpriteRenderer BGImage;
    [SerializeField] Sprite BGSunset;
    [SerializeField] Sprite BGRetro;

    private UIActiveState[] uIActiveStates;
    private SaveLoadSliderValue[] sliderValues;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        uIActiveStates = FindObjectsOfType<UIActiveState>(true);
        ResetUI();

        sliderValues = FindObjectsOfType<SaveLoadSliderValue>(true);
        LoadSliderValues();
    }

    public void ShowMainMenu()
    {
        //Reset active states of UI game objects
        ResetUI();
    }

    public void ShowScoreToCoinsPopup()
    {
        m_ScoreToCoinsPopup.OnEndGame();
        m_ScoreToCoinsPopup.gameObject.GetComponent<UIActiveState>().Show();
    }

    public void OnUserRewarded()
    {
        m_ScoreToCoinsPopup.OnUserRewarded();
    }

    private void ResetUI()
    {
        foreach (UIActiveState uIActiveState in uIActiveStates)
            uIActiveState.ResetUI();
    }

    private void LoadSliderValues()
    {
        foreach (SaveLoadSliderValue slider in sliderValues)
            slider.LoadSliderValue();
    }

    public void UpdateVisibilityWaitingForPlayer(bool isShow)
    {
        if (isShow)
            m_PopupWaitingForPlayers.Show();
        else
            m_PopupWaitingForPlayers.Hide();
    }

    public void HideInGameOptionsPopup()
    {
        m_InGameOptionsPopup.Hide();
    }

    public void ShowAndroidMicPermissionPopup()
    {
        m_AndroidMicPermissionPopup.Show();
    }

    public void HideAndroidMicPermissionPopup()
    {
        m_AndroidMicPermissionPopup.Hide();
    }

    public void SetBackground(string bg)
    {
        switch (bg)
        {
            case "Sunset":
                BGImage.sprite = BGSunset;
                break;
            case "Retro":
                BGImage.sprite = BGRetro;
                break;
        }
    }
}