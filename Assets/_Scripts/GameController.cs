using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    #region Variable
    [SerializeField]
    private SlotMachine slotMachine;
    [SerializeField]
    private TextMeshProUGUI balanceText, betSizeText;
    [SerializeField]
    private GameObject slotPanel;
    [SerializeField]
    private GameObject popupText;
    public double cashBalance = 10000;
    public double costPerSpin = 32;
    private bool isSpinning;
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
    public void Spin()
    {
        if (isSpinning)
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
            StartCoroutine(slotMachine.Spin());
        }
        else if (cashBalance <= costPerSpin)
        {
            AudioManager.Instance.SetState("ErrorSound", true);
            Debug.Log("Not enough balance!");
        }
    }

    private IEnumerator Win(double payout)
    {
        cashBalance += payout;
        UpdateBalance();

        if (payout >= costPerSpin * 5)
        {
            AudioManager.Instance.SetState("BigWinSound", true);
        }
        else
        {
            AudioManager.Instance.SetState("WinSound", true);
        }

        GameObject g = Instantiate(popupText, new Vector3(balanceText.transform.position.x + Random.Range(-500, 500), balanceText.transform.position.y + Random.Range(-500, -100), balanceText.transform.position.z), transform.rotation, GameObject.Find("Canvas").transform);
        g.GetComponent<TextMeshProUGUI>().text = $"+ ${payout}";

        Color textStartColor = balanceText.color;
        balanceText.color = Color.yellow;
        yield return new WaitForSeconds(g.GetComponent<PopupText>().fadeTime);
        balanceText.color = textStartColor;
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

        isSpinning = false;
        AudioManager.Instance.SetState("RerollSound", false);
    }
}
