using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class SlotItem
{
    public int itemID, gameIndex;
    public Sprite image;
    public double rollLow, rollHigh;
    public double payoutMultiplier;
}
public class SlotMachine : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private GameObject[] columns;
    [SerializeField]
    private GameController gameController;
    public List<SpriteRenderer> slotImages;
    [SerializeField]
    private Sprite defaultSlotImage;
    [SerializeField]
    private SlotItem[] slotItems;
    [HideInInspector]
    public SlotItem[] rolledItems; // Only to keep track of roll, read-only!
    [SerializeField]
    #endregion

    private void Start()
    {
        rolledItems = new SlotItem[columns.Length * columns.Length];
        if (columns != null)
        {
            FetchImages();
        }
        else
        {
            Debug.Log("No column objects found!");
            Application.Quit();
        }
    }

    private void FetchImages()
    {
        try
        {
            slotImages = new List<SpriteRenderer>();

            // Adds the image reference from UI
            foreach (GameObject g in columns)
            {
                foreach (SpriteRenderer spr in g.GetComponentsInChildren<SpriteRenderer>())
                {
                    slotImages.Add(spr);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            throw;
        }
    }

    private SlotItem RandomSlotItem()
    {
        int roll = Random.Range(0, 1001); // Between 0-1000 (1001 doesn't count)
        foreach (SlotItem item in slotItems)
        {
            if (item.rollLow <= roll && item.rollHigh >= roll)
            {
                return item;
            }
        }

        Debug.Log("An error occurred, no item was rolled, are roll limits correctly set?");
        return slotItems[0];
    }
    public IEnumerator Spin(bool isRerollSymbol)
    {
        if (rolledItems[0] != null)
        {
            for (int i = 0; i < slotImages.Count; i++)
            {
                if (rolledItems[i].itemID == 7 && isRerollSymbol)
                {
                    continue;
                }
                slotImages[i].sprite = defaultSlotImage;
            }
        }
        yield return new WaitForSeconds(0.75f);

        for (int i = 0; i < slotImages.Count; i++)
        {
            if (isRerollSymbol && rolledItems[i].itemID == 7)
            {
                continue;
            }

            for (int t = 0; t <= Random.Range(5, 10); t++)
            {
                // Spin "animation"
                SlotItem newRollItem = RandomSlotItem();
                if (gameController.isBonusRound)
                {
                    while (newRollItem.itemID == 7)
                    {
                        newRollItem = RandomSlotItem();
                    }
                }
                rolledItems[i] = newRollItem;
                slotImages[i].sprite = rolledItems[i].image;
                yield return new WaitForSeconds(0.01f);
            }
            yield return new WaitForSeconds(Random.Range(0.03f, 0.06f));
            rolledItems[i].gameIndex = i;
        }
        gameController.CheckForWin();
    }

    public IEnumerator Reroll(int[] imageIndexes)
    {
        AudioManager.Instance.SetState("RerollSound", true);
        yield return new WaitForSeconds(1f);
        foreach (int i in imageIndexes)
        {
            SlotItem newRollItem = RandomSlotItem();
            if (gameController.isBonusRound)
            {
                while (newRollItem.itemID == 7)
                {
                    newRollItem = RandomSlotItem();
                }
            }
            rolledItems[i] = newRollItem;
            slotImages[i].sprite = rolledItems[i].image;
            slotImages[i].color = Color.white;

            if (gameController.isBonusRound)
            {
                AudioManager.Instance.SetState("BonusHitSound", true);
            }
            else
            {
                AudioManager.Instance.SetState("SlotHitSound", true);
            }
            yield return new WaitForSeconds(0.5f);
        }

        foreach (SpriteRenderer spr in slotImages)
        {
            spr.color = new Color(255, 255, 255, 1.0f);
        }
        gameController.CheckForWin();
    }

}
