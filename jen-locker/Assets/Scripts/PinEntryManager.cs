using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Drives the PIN-entry UI defined in PinEntryUI.xml / PinEntryUI.uss.
/// Attach this component to the same GameObject that holds a UIDocument.
/// Set the correct PIN in the Inspector (default: 1234).
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class PinEntryManager : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Tooltip("The correct PIN the user must enter.")]
    [SerializeField] private string correctPin = "1234";

    [Tooltip("Maximum number of digits allowed (should match PIN length).")]
    [SerializeField] private int maxDigits = 4;

    // ── UI references ────────────────────────────────────────────────────────
    private VisualElement _pinPanel;
    private VisualElement _mainContent;
    private Label        _pinDisplay;
    private Label        _errorText;

    // ── State ────────────────────────────────────────────────────────────────
    private string _currentInput = "";

    // ── Lifecycle ────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Cache references
        _pinPanel    = root.Q<VisualElement>("PinPanel");
        _mainContent = root.Q<VisualElement>("MainContent");
        _pinDisplay  = root.Q<Label>("PinDisplay");
        _errorText   = root.Q<Label>("ErrorText");

        // Wire up digit buttons 0-9
        for (int digit = 0; digit <= 9; digit++)
        {
            string btnName  = digit == 0 ? "Btn0" : $"Btn{digit}";
            string captured = digit.ToString();          // capture for lambda
            Button btn      = root.Q<Button>(btnName);

            if (btn != null)
                btn.clicked += () => OnDigitPressed(captured);
            else
                Debug.LogWarning($"[PinEntryManager] Button '{btnName}' not found in UXML.");
        }

        // Wire up backspace
        Button backspace = root.Q<Button>("BtnBackspace");
        if (backspace != null)
            backspace.clicked += OnBackspacePressed;
        else
            Debug.LogWarning("[PinEntryManager] BtnBackspace not found in UXML.");

        // Initial display state
        RefreshPinDisplay();
        HideError();
    }

    // ── Button handlers ───────────────────────────────────────────────────────
    private void OnDigitPressed(string digit)
    {
        if (_currentInput.Length >= maxDigits) return;

        _currentInput += digit;
        RefreshPinDisplay();
        HideError();

        if (_currentInput.Length == maxDigits)
            ValidatePin();
    }

    private void OnBackspacePressed()
    {
        if (_currentInput.Length == 0) return;

        _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
        RefreshPinDisplay();
        HideError();
    }

    // ── PIN logic ─────────────────────────────────────────────────────────────
    private void ValidatePin()
    {
        if (_currentInput == correctPin)
        {
            OnPinAccepted();
        }
        else
        {
            OnPinRejected();
        }
    }

    private void OnPinAccepted()
    {
        // Hide the PIN panel, show the main content
        _pinPanel.AddToClassList("hidden");
        _mainContent.RemoveFromClassList("hidden");
    }

    private void OnPinRejected()
    {
        ShowError();
        _currentInput = "";
        RefreshPinDisplay();
    }

    // ── Display helpers ───────────────────────────────────────────────────────
    /// <summary>
    /// Renders filled dots for entered digits and dashes for remaining slots.
    /// E.g. with 2 of 4 entered: "● ● – –"
    /// </summary>
    private void RefreshPinDisplay()
    {
        if (_pinDisplay == null) return;

        var parts = new System.Text.StringBuilder();
        for (int i = 0; i < maxDigits; i++)
        {
            if (i > 0) parts.Append(' ');
            parts.Append(i < _currentInput.Length ? '●' : '–');
        }

        _pinDisplay.text = parts.ToString();
    }

    private void ShowError()
    {
        _errorText?.RemoveFromClassList("hidden");
    }

    private void HideError()
    {
        _errorText?.AddToClassList("hidden");
    }
}