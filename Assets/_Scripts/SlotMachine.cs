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
    private List<Image> slotImages;
    [SerializeField]
    private Sprite defaultSlotImage;
    [SerializeField]
    private SlotItem[] slotItems;
    //[HideInInspector]
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
            slotImages = new List<Image>();

            // Adds the image reference from UI
            foreach (GameObject g in columns)
            {
                foreach (Image img in g.GetComponentsInChildren<Image>())
                {
                    slotImages.Add(img);
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
        int roll = Random.Range(0, 101); // Between 0-100 (101 doesn't count)
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
    public IEnumerator Spin()
    {
        foreach (Image img in slotImages)
        {
            img.sprite = defaultSlotImage;
        }

        for (int i = 0; i < slotImages.Count; i++)
        {
            for (int t = 0; t <= Random.Range(2, 5); t++)
            {
                // Spin "animation"
                rolledItems[i] = RandomSlotItem();
                slotImages[i].sprite = rolledItems[i].image;
                yield return new WaitForSeconds(0.01f);
            }
            yield return new WaitForSeconds(0.05f);
            rolledItems[i].gameIndex = i;
        }
        gameController.CheckForWin();
    }

    public IEnumerator Reroll(int[] imageIndexes)
    {
        AudioManager.Instance.SetState("RerollSound", true);
        foreach (int i in imageIndexes)
        {
            slotImages[i].color = Color.cyan;
        }

        yield return new WaitForSeconds(1f);
        foreach (int i in imageIndexes)
        {
            rolledItems[i] = RandomSlotItem();
            slotImages[i].sprite = rolledItems[i].image;
            slotImages[i].color = Color.white;
            AudioManager.Instance.SetState("SlotHitSound", true);
            yield return new WaitForSeconds(0.5f);
        }
        gameController.CheckForWin();
    }

}
