using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class Game : MonoBehaviour
{
    [Header("UI")]
    [Space(10)]
    public RectTransform menuRect;
    public RawImage renderTextureOverlay;
    RenderTexture renderTex;
    public Image fadeOverlay;
    [Space(10)]
    public CanvasGroup arrowKeyContainer;
    public RectTransform[] arrowKeys = new RectTransform[] { };
    float arrowKeysTargetAlpha = 0;
    float arrowFadeStartTime = -Mathf.Infinity;
    float arrowFadeDuration = 3f;
    public Image blackScreen;

    float fadeInStartTime = -Mathf.Infinity;
    float fadeOutStartTime = -Mathf.Infinity;

    void fadeArrowKeys(float targetAlpha)
    {
        arrowFadeStartTime = Time.timeSinceLevelLoad;
        arrowKeysTargetAlpha = targetAlpha;
    }

    void updateArrowKeys(Node node)
    {
        if (node.forwardNode != null)
        {
            arrowKeys[0].gameObject.SetActive(true);
            arrowKeys[0].GetComponentInChildren<TextMeshProUGUI>().text = node.forwardNode.name;
        }
        else
        {
            arrowKeys[0].gameObject.SetActive(false);
        }


        if (node.leftNode != null)
        {
            arrowKeys[1].gameObject.SetActive(true);
            arrowKeys[1].GetComponentInChildren<TextMeshProUGUI>().text = node.leftNode.name;
        }
        else
        {
            arrowKeys[1].gameObject.SetActive(false);
        }


        if (node.rightNode != null)
        {
            arrowKeys[2].gameObject.SetActive(true);
            arrowKeys[2].GetComponentInChildren<TextMeshProUGUI>().text = node.rightNode.name;
        }
        else
        {
            arrowKeys[2].gameObject.SetActive(false);
        }


        if (node.backNode != null)
        {
            arrowKeys[3].gameObject.SetActive(true);
            arrowKeys[3].GetComponentInChildren<TextMeshProUGUI>().text = node.backNode.name;
        }
        else
        {
            arrowKeys[3].gameObject.SetActive(false);
        }
    }

    void fadeIn()
    {
        fadeInStartTime = Time.time;
        blackScreen.color = new Color(0, 0, 0, 1);
    }

    void fadeOut()
    {
        fadeOutStartTime = Time.time;
        blackScreen.color = new Color(0, 0, 0, 0);
    }
}
