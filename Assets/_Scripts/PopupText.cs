using System.Collections;
using TMPro;
using UnityEngine;

public class PopupText : MonoBehaviour
{
    #region Variables
    public float fadeTime = 1f, moveSpeedY = 10;
    private TextMeshProUGUI textObject;
    #endregion

    private void Start()
    {
        textObject = GetComponent<TextMeshProUGUI>();
        StartCoroutine(SelfDestruct());
    }

    private IEnumerator SelfDestruct()
    {
        float currentTime = 0;
        while (currentTime <= fadeTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, currentTime / fadeTime);
            textObject.color = new Color(textObject.color.r, textObject.color.b, textObject.color.g, alpha);
            currentTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
        yield break;
    }
    private void Update()
    {
        transform.Translate(new Vector3(0, moveSpeedY * Time.deltaTime));
    }
}
