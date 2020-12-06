using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private GameController gameController;
    [SerializeField]
    private Button spinBtn, increaseBetBtn, decreaseBetBtn;
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

    private void SpinBtnClick()
    {
        gameController.Spin();
    }

    private void IncreaseBetClick()
    {
        if (gameController.costPerSpin <= gameController.cashBalance * 2)
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
        if (gameController.costPerSpin >= 0.5f)
        {
            gameController.ChangeBetSize(0.5f);
        }
        else
        {
            AudioManager.Instance.SetState("ErrorSound", true);
        }
    }
}
