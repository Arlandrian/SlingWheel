using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region GameObject References

    [Header("Component References")]
    [SerializeField]
    private Text _startText;
    [SerializeField]
    private Button _retryButton;
    [SerializeField]
    private Text _progressText;
    [SerializeField]
    private Text _levelText;

    #endregion

    #region Events 

    public event Action RetryClickedEvent;

    #endregion

    public void Init()
    {
        EnableTapToStartText(true);

        _progressText.text = "0";

        _retryButton.onClick.RemoveAllListeners();
        _retryButton.onClick.AddListener(OnRetryButtonClicked);
        _retryButton.gameObject.SetActive(false);

        GameManager.Instance.GameStartedEvent += OnGameStarted;
        GameManager.Instance.LevelFinishedEvent += OnNextLevelReached;
        GameManager.Instance.FailedEvent += OnFail;
    }

    #region Event Receivers

    public void OnCheckPointReached(int newScore)
    {
        _progressText.text = newScore.ToString();
    }

    private void OnGameStarted()
    {
        EnableTapToStartText(false);
    }

    private void OnNextLevelReached(LevelFinishedEventArgs args)
    {
        StartCoroutine(LevelFinished());
    }

    private IEnumerator LevelFinished()
    {
        EnableLevelUpText(true);
        yield return new WaitForSeconds(GlobalConfigs.LevelFinishAnimationTimeInSec);
        EnableLevelUpText(false);
    }

    private void OnFail()
    {
        _retryButton.gameObject.SetActive(true);
    }

    private void OnRetryButtonClicked()
    {
        RetryClickedEvent?.Invoke();
    }

    #endregion

    private void EnableTapToStartText(bool enable) => _startText.enabled = enable;
    private void EnableLevelUpText(bool enable) => _levelText.enabled = enable;

}
