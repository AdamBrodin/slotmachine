using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnSpinner : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private float columnTopY, columnBotY;
    [SerializeField]
    private float spinSpeed;
    private int randSpinDuration;
    private bool shouldSpin;
    [SerializeField]
    private GameController gameController;
    #endregion
    private void Start()
    {
        UIHandler.startedSpinning += StartRotating;
    }

    private void StartRotating()
    {
        StartCoroutine(Rotate());
    }

    private void FixedUpdate()
    {
        if (shouldSpin)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - spinSpeed, transform.position.z);

            if (transform.position.y <= columnBotY)
            {
                transform.position = new Vector3(transform.position.x, columnTopY, transform.position.z);
            }
        }
    }
    public IEnumerator Rotate()
    {
        shouldSpin = true;
        yield return new WaitForSeconds(5f);
        shouldSpin = false;
    }

}
