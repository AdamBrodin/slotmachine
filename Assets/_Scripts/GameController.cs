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
    private readonly int[] firstColumn = new int[] { 0, 5, 10, 15, 20 };
    private readonly int[] secondColumn = new int[] { 1, 6, 11, 16, 21 };
    private readonly int[] thirdColumn = new int[] { 2, 7, 12, 17, 22 };
    private readonly int[] fourthColumn = new int[] { 3, 8, 13, 18, 23 };
    private readonly int[] fifthColumn = new int[] { 4, 9, 14, 19, 24 };

    // Diagonals
    private readonly int[] topLeftDiagonal = new int[] { 0, 6, 12, 18, 24 };
    private readonly int[] bottomLeftDiagonal = new int[] { 4, 8, 12, 16, 20 };

    #endregion
    private void Start()
    {
        AudioManager.Instance.SetState("BackgroundMusic", true);
        betSizeText.text = $"BET SIZE: ${costPerSpin}";
        UpdateBalance();

        winScenarios.Add(firstColumn);
        winScenarios.Add(secondColumn);
        winScenarios.Add(thirdColumn);
        winScenarios.Add(fourthColumn);
        winScenarios.Add(fifthColumn);
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
        Color panelStartColor = slotPanel.GetComponent<Image>().color;

        balanceText.color = Color.yellow;
        //slotPanel.GetComponent<Image>().color = Color.yellow;
        yield return new WaitForSeconds(g.GetComponent<PopupText>().fadeTime);
        balanceText.color = textStartColor;
        //slotPanel.GetComponent<Image>().color = panelStartColor;
    }

    private double PaylineCalculator()
    {
        double wonAmount = 0;
        SlotItem[] rolledItems = slotMachine.rolledItems;

        // Checks scenarios
        foreach (int[] scenario in winScenarios)
        {
            int counter = 0;
            foreach (int i in scenario)
            {
                if (i < rolledItems.Length - 1)
                {
                    if (rolledItems[i].itemID == rolledItems[i + 1].itemID)
                    {
                        counter++;
                        indexesToReroll.Add(i);
                    }
                }

                if (counter >= 4)
                {
                    wonAmount += costPerSpin * rolledItems[i].payoutMultiplier;
                }
            }
        }

        // Check for 4x or more touching
        Dictionary<SlotItem, int> occurrences = new Dictionary<SlotItem, int>();
        foreach (SlotItem item in rolledItems)
        {
            if (occurrences.ContainsKey(item))
            {
                occurrences[item]++;
            }
            else
            {
                occurrences.Add(item, 1);
            }
        }

        foreach (KeyValuePair<SlotItem, int> pair in occurrences)
        {
            if (pair.Value >= 4)
            {
                Debug.Log($"Pair value of ID {pair.Key.image} is at {pair.Value}");
                List<int> indexTotal = new List<int>();
                foreach (SlotItem i in rolledItems)
                {
                    if (i.itemID == pair.Key.itemID)
                    {
                        indexTotal.Add(i.gameIndex);
                        Debug.Log($"Added index of ID {i.image} at index - {i.gameIndex}");
                    }
                }

                if ((int)(indexTotal.Sum(s => Convert.ToInt32(s)) / pair.Value) <= 5)
                {
                    Debug.Log($"Index distance is at {(int)(indexTotal.Sum(s => Convert.ToInt32(s)) / pair.Value)}");
                    foreach (int i in indexTotal)
                    {
                        indexesToReroll.Add(i);
                        Debug.Log($"Added reroll for index {i}");
                    }

                    wonAmount += (costPerSpin * pair.Key.payoutMultiplier) * pair.Value;
                }
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
