using UnityEngine;
using TMPro;
using System.Collections;

public class Messaging : MonoBehaviour
{
    [SerializeField]
    private TMP_Text turnText;
    [SerializeField]
    private TMP_Text messageText;

    public GameObject panal;

    public void FlipTurn(bool isWhite)
    {
        turnText.text = isWhite ? "White's Turn" : "Black's Turn";
        turnText.color = isWhite ? Color.white : Color.black;
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        StartCoroutine(HideMessageAfterDelay(3f));
    }

    public void HideMessage()
    {
        messageText.text = "";
        messageText.gameObject.SetActive(false);
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideMessage();
    }

    public void SetPanal(bool isActive)
    {
        panal.SetActive(isActive);
    }
}
