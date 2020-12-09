using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    #region Variable
    [SerializeField]
    private SlotMachine slotMachine;
    [SerializeField]
    private TextMeshProUGUI balanceText, betSizeText, bonusText;
    [SerializeField]
    private GameObject popupText;
    public double cashBalance = 10000;
    public double costPerSpin = 32;
    public bool isSpinning;
    public bool isBonusRound;
    private int bonusRoundsLeft, bonusMultiplier = 1;
    [SerializeField]
    private VideoPlayer backgroundVideo;
    private List<int> indexesToReroll = new List<int>();

    // Rows/Columns payline indexes
    private List<int[]> winScenarios = new List<int[]>();

    // Columns
    private readonly int[] firstColumn = new int[] { 0, 3, 6 };
    private readonly int[] secondColumn = new int[] { 1, 4, 7 };
    private readonly int[] thirdColumn = new int[] { 2, 5, 8 };

    // Diagonals
    private readonly int[] topLeftDiagonal = new int[] { 0, 4, 8 };
    private readonly int[] bottomLeftDiagonal = new int[] { 2, 4, 6 };

    #endregion
    private void Start()
    {
        AudioManager.Instance.SetState("BackgroundMusic", true);
        betSizeText.text = $"BET SIZE: ${costPerSpin}";
        UpdateBalance();

        winScenarios.Add(firstColumn);
        winScenarios.Add(secondColumn);
        winScenarios.Add(thirdColumn);
        winScenarios.Add(topLeftDiagonal);
        winScenarios.Add(bottomLeftDiagonal);
    }

    private void UpdateBalance()
    {
        balanceText.text = $"BALANCE: ${cashBalance}";
    }

    public void ChangeBetSize(double multiplier)
    {
        costPerSpin *= multiplier;
        betSizeText.text = $"BET SIZE: ${costPerSpin}";
    }
    public void Spin(bool isRerollSymbol)
    {
        if (isSpinning && !isRerollSymbol)
        {
            AudioManager.Instance.SetState("ErrorSound", true);
            return;
        }

        if (cashBalance >= costPerSpin)
        {
            isSpinning = true;
            cashBalance -= costPerSpin;
            UpdateBalance();
            AudioManager.Instance.SetState("SpinSound", true);
            if (isBonusRound)
            {
                bonusRoundsLeft--;
                UpdateBonusUI();
            }
            StartCoroutine(slotMachine.Spin(isRerollSymbol));
        }
        else if (cashBalance <= costPerSpin)
        {
            AudioManager.Instance.SetState("ErrorSound", true);
            Debug.Log("Not enough balance!");
        }
    }

    private IEnumerator Win(double payout)
    {
        if (isBonusRound)
        {
            bonusMultiplier++;
            bonusRoundsLeft++;
            payout *= bonusMultiplier;
            UpdateBonusUI();
        }
        cashBalance += payout;
        UpdateBalance();

        if (isBonusRound)
        {
            AudioManager.Instance.TogglePause("BonusRoundMusic");
        }
        else
        {
            AudioManager.Instance.TogglePause("BackgroundMusic");
        }

        if (payout >= costPerSpin * 5)
        {
            AudioManager.Instance.SetState("BigWinSound", true);
        }
        else
        {
            AudioManager.Instance.SetState("WinSound", true);
        }

        GameObject g = Instantiate(popupText, new Vector3(0, balanceText.transform.position.y, balanceText.transform.position.z), transform.rotation, GameObject.Find("Canvas").transform);
        g.GetComponent<TextMeshProUGUI>().text = $"+ ${payout}";

        Color textStartColor = balanceText.color;
        balanceText.color = Color.yellow;
        yield return new WaitForSeconds(g.GetComponent<PopupText>().fadeTime);
        balanceText.color = textStartColor;

        if (isBonusRound)
        {
            AudioManager.Instance.TogglePause("BonusRoundMusic");
        }
        else
        {
            AudioManager.Instance.TogglePause("BackgroundMusic");
        }
    }

    private double PaylineCalculator()
    {
        double wonAmount = 0;
        SlotItem[] rolledItems = slotMachine.rolledItems;

        // Checks scenarios
        foreach (int[] scenario in winScenarios)
        {
            if (rolledItems[scenario[0]].itemID == rolledItems[scenario[1]].itemID && rolledItems[scenario[1]].itemID == rolledItems[scenario[2]].itemID)
            {
                indexesToReroll.Add(scenario[0]);
                indexesToReroll.Add(scenario[1]);
                indexesToReroll.Add(scenario[2]);
                for (int i = 0; i < slotMachine.slotImages.Count; i++)
                {
                    if (i == scenario[0] || i == scenario[1] || i == scenario[2])
                    {
                        slotMachine.slotImages[i].color = new Color(0, 255, 0, 1.0f);
                        continue;
                    }

                    slotMachine.slotImages[i].color = new Color(255, 255, 255, 0.15f);
                }
                wonAmount += costPerSpin * rolledItems[scenario[0]].payoutMultiplier;
            }
        }

        if (wonAmount <= 0)
        {
            indexesToReroll.Clear();
            return 0;
        }
        else
        {
            return wonAmount;
        }
    }

    private IEnumerator StartReroll()
    {
        AudioManager.Instance.SetState("RerollSymbolSound", true);
        yield return new WaitForSeconds(2f);
        Spin(true);
    }

    private void UpdateBonusUI()
    {
        if (isBonusRound)
        {
            if (!bonusText.gameObject.activeSelf)
            {
                bonusText.gameObject.SetActive(true);
            }
            bonusText.text = $"BONUS ROUNDS LEFT: {bonusRoundsLeft} - {bonusMultiplier}X";
        }
        else
        {
            bonusText.gameObject.SetActive(false);
        }
    }
    public void CheckForWin()
    {
        indexesToReroll.Clear();
        double wonAmount = PaylineCalculator();

        if (wonAmount > 0)
        {
            StartCoroutine(Win(wonAmount));
        }
        if (indexesToReroll.Count > 0)
        {
            List<int> withoutDupes = indexesToReroll.Distinct().ToList();
            StartCoroutine(slotMachine.Reroll(withoutDupes.ToArray()));
            return;
        }

        int rerollCounter = 0, bonusCounter = 0;
        foreach (SlotItem item in slotMachine.rolledItems)
        {
            if (item.itemID == 6) // Reroll symbol
            {
                rerollCounter++;
            }

            if (item.itemID == 7) // Bonus symbol
            {
                bonusCounter++;
            }
        }

        if (rerollCounter > 0 || (rerollCounter + bonusCounter) >= 3)
        {
            if ((rerollCounter + bonusCounter) >= 3 && !isBonusRound)
            {
                AudioManager.Instance.SetState("BonusSound", true);
                isBonusRound = true;
                bonusRoundsLeft = Random.Range(5, 20);
                UpdateBonusUI();
                isSpinning = false;
                AudioManager.Instance.SetState("BackgroundMusic", false);
                AudioManager.Instance.SetState("BonusRoundMusic", true);
                backgroundVideo.playbackSpeed = 3;
            }
            else if (rerollCounter > 0)
            {
                bonusRoundsLeft++;
                StartCoroutine(StartReroll());
            }
            return;
        }

        if (isBonusRound && bonusRoundsLeft <= 0)
        {
            isBonusRound = false;
            bonusMultiplier = 1;
            bonusText.gameObject.SetActive(false);
            AudioManager.Instance.SetState("BonusRoundMusic", false);
            AudioManager.Instance.SetState("BackgroundMusic", true);
            backgroundVideo.playbackSpeed = 1f;
        }

        isSpinning = false;
        AudioManager.Instance.SetState("RerollSound", false);
    }
}
