using System.Collections;
using UnityEngine;

public class ScreenFadeTransition : MonoBehaviour
{
    public static ScreenFadeTransition Instance { get; private set; }

    [SerializeField] private CanvasGroup faderCanvasGroup;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (faderCanvasGroup != null)
        {
            faderCanvasGroup.alpha = 0f;
            faderCanvasGroup.blocksRaycasts = false;
        }
    }

    public void TeleportWithFade(GameObject player, Transform destinationPoint)
    {
        StartCoroutine(FadeRoutine(player, destinationPoint));
    }

    private IEnumerator FadeRoutine(GameObject player, Transform destinationPoint)
    {
        if (faderCanvasGroup == null) yield break;

        faderCanvasGroup.blocksRaycasts = true;

        float duration = 0.18f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            faderCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        faderCanvasGroup.alpha = 1f;

        if (player != null && destinationPoint != null)
        {
            player.transform.position = destinationPoint.position;
            RespawnManager.HasEnteredCastle = true;

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(destinationPoint.position.x, destinationPoint.position.y, mainCam.transform.position.z);
            }
        }

        yield return new WaitForSeconds(0.05f);

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            faderCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        faderCanvasGroup.alpha = 0f;
        faderCanvasGroup.blocksRaycasts = false;
    }
}