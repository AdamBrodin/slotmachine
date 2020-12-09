using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private GameController gameController;
    [SerializeField]
    private Button spinBtn, increaseBetBtn, decreaseBetBtn;
    public static event Action startedSpinning = delegate { };
    #endregion
    private void Start()
    {
        SetupListeners();
    }

    private void SetupListeners()
    {
        spinBtn.onClick.AddListener(SpinBtnClick);
        increaseBetBtn.onClick.AddListener(IncreaseBetClick);
        decreaseBetBtn.onClick.AddListener(DecreaseBetClick);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpinBtnClick();
        }
    }

    private void SpinBtnClick()
    {
        gameController.Spin(false);
    }

    private void IncreaseBetClick()
    {
        if (gameController.costPerSpin <= gameController.cashBalance * 2 && !gameController.isSpinning && !gameController.isBonusRound)
        {
            gameController.ChangeBetSize(2);
        }
        else
        {
            AudioManager.Instance.SetState("ErrorSound", true);
        }
    }

    private void DecreaseBetClick()
    {
        if (gameController.costPerSpin >= 0.5f && !gameController.isSpinning && !gameController.isBonusRound)
        {
            gameController.ChangeBetSize(0.5f);
        }
        else
        {
            AudioManager.Instance.SetState("ErrorSound", true);
        }
    }
}
