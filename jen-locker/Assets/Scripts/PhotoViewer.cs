using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhotoViewer : MonoBehaviour
{
    public static PhotoViewer Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject viewerPanel;
    [SerializeField] private RawImage photoImage;
    [SerializeField] private Button closeButton;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;

    private CanvasGroup _canvasGroup;
    private AspectRatioFitter _aspectRatioFitter;

    private void Awake()
    {
        Instance = this;
        _canvasGroup = viewerPanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = viewerPanel.AddComponent<CanvasGroup>();

        _aspectRatioFitter = photoImage.GetComponent<AspectRatioFitter>();
        if (_aspectRatioFitter == null)
            _aspectRatioFitter = photoImage.gameObject.AddComponent<AspectRatioFitter>();

        _aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

        viewerPanel.SetActive(false);
        closeButton.onClick.AddListener(Close);
    }

    public void Open(Sprite sprite)
    {
        photoImage.texture = sprite.texture;

        _aspectRatioFitter.aspectRatio = (float)sprite.texture.width / sprite.texture.height;

        viewerPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Animate(0f, 1f));
    }

    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        yield return Animate(1f, 0f);
        viewerPanel.SetActive(false);
    }

    private IEnumerator Animate(float from, float to)
    {
        float elapsed = 0f;
        _canvasGroup.alpha = from;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
            _canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        _canvasGroup.alpha = to;
    }
}