using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class DialogueParagraph : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI textMeshBase;
    public TextMeshProUGUI textMeshSpoken;

    private void Update()
    {
        updateText();
    }

    public void updateText()
    {
        int length = inputField.text.Length;

        if (length <= textMeshBase.text.Length)
        {
            string inputLower = inputField.text.ToLower();
            string baseLower = textMeshBase.text.ToLower();

            StringBuilder dialogueBuilder = new StringBuilder(32);

            for (int i = 0; i < length; i++)
            {
                if (inputLower[i] == baseLower[i])
                {
                    dialogueBuilder.Append(textMeshBase.text[i]);
                }
            }
            textMeshSpoken.text = dialogueBuilder.ToString();
        }
    }
}
