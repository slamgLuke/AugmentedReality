using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public enum InteractionMode
    {
        Touch, // For interacting with existing objects
        Spawn  // For spawning new objects
    }

    // --- Global Static Properties ---
    public static InteractionMode CurrentMode { get; private set; } = InteractionMode.Touch; // Default
    public static bool IsUIActive { get; private set; } = false;

    // --- Events ---
    public static event System.Action<InteractionMode> OnModeChanged;
    public static event System.Action OnResetSceneConfirmed;
    // No longer need OnInteractModeToggled or OnSpawnModeToggled, use OnModeChanged

    [Header("Menu Elements")]
    public GameObject sideMenuPanel;
    public Button hamburgerButton;
    public RectTransform sideMenuRectTransform;
    public GameObject backgroundCatcherPanel; // Assign the transparent full-screen panel

    [Header("Mode Controls")]
    public Button resetButton;
    public Button touchModeButton;
    public Button spawnModeButton;
    public TextMeshProUGUI stateVisualizerText;

    [Header("Mode Button Visuals")]
    public Color normalButtonColor = Color.white;
    public Color highlightedButtonColor = Color.yellow; // Example color

    [Header("Confirmation Popup")]
    public GameObject confirmationPopupPanel;
    public TextMeshProUGUI popupMessageText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("Side Menu Animation")]
    public float animationDuration = 0.3f;
    private bool isSideMenuOpen = false;
    private Coroutine menuAnimationCoroutine;
    private float sideMenuWidth;

    void Start()
    {
        // Initial states
        if (sideMenuPanel) sideMenuPanel.SetActive(true); // Keep active for animation
        if (sideMenuRectTransform)
        {
            sideMenuWidth = sideMenuRectTransform.rect.width;
            sideMenuRectTransform.anchoredPosition = new Vector2(-sideMenuWidth, sideMenuRectTransform.anchoredPosition.y);
        }
        else Debug.LogError("SideMenuRectTransform not assigned!");

        if (confirmationPopupPanel) confirmationPopupPanel.SetActive(false);
        if (backgroundCatcherPanel) backgroundCatcherPanel.SetActive(false);

        // Assign button listeners
        if (hamburgerButton) hamburgerButton.onClick.AddListener(ToggleSideMenu);

        if (resetButton) resetButton.onClick.AddListener(HandleResetButton);
        if (touchModeButton) touchModeButton.onClick.AddListener(SetTouchMode);
        if (spawnModeButton) spawnModeButton.onClick.AddListener(SetSpawnMode);

        if (confirmYesButton) confirmYesButton.onClick.AddListener(HandleConfirmYes);
        if (confirmNoButton) confirmNoButton.onClick.AddListener(HandleConfirmNo);

        // Listener for the background catcher (to close side menu)
        Button bgCatcherButton = backgroundCatcherPanel?.GetComponent<Button>();
        if (bgCatcherButton)
        {
            bgCatcherButton.onClick.AddListener(CloseSideMenuFromBackground);
        }
        else if (backgroundCatcherPanel)
        {
            Debug.LogWarning("BackgroundCatcherPanel does not have a Button component for click detection.");
        }


        // Initialize Mode and UI
        ApplyModeChange(CurrentMode, false); // Apply initial mode without closing menu
        UpdateIsUIActive();
    }

    private void UpdateIsUIActive()
    {
        // IsUIActive is true if the side menu intends to be open OR the popup is currently active.
        // The visual state of the side menu (during animation) might slightly differ.
        IsUIActive = isSideMenuOpen || (confirmationPopupPanel != null && confirmationPopupPanel.activeSelf);
        // Debug.Log($"UIManager.IsUIActive set to: {IsUIActive}");
    }

    void ToggleSideMenu()
    {
        isSideMenuOpen = !isSideMenuOpen;
        UpdateIsUIActive(); // Update global state

        if (menuAnimationCoroutine != null)
        {
            StopCoroutine(menuAnimationCoroutine);
        }
        menuAnimationCoroutine = StartCoroutine(AnimateSideMenu(isSideMenuOpen));

        if (backgroundCatcherPanel)
        {
            backgroundCatcherPanel.SetActive(isSideMenuOpen); // Activate catcher when menu is open
        }
    }

    IEnumerator AnimateSideMenu(bool open)
    {
        float targetX = open ? 0 : -sideMenuWidth;
        float startX = sideMenuRectTransform.anchoredPosition.x;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float newX = Mathf.Lerp(startX, targetX, elapsedTime / animationDuration);
            sideMenuRectTransform.anchoredPosition = new Vector2(newX, sideMenuRectTransform.anchoredPosition.y);
            yield return null;
        }
        sideMenuRectTransform.anchoredPosition = new Vector2(targetX, sideMenuRectTransform.anchoredPosition.y);
        // Ensure IsUIActive is correct after animation, especially if closed via background
        UpdateIsUIActive();
    }

    void CloseSideMenuFromBackground()
    {
        if (isSideMenuOpen)
        {
            ToggleSideMenu(); // This will handle animation and IsUIActive update
        }
    }

    void SetTouchMode()
    {
        ApplyModeChange(InteractionMode.Touch);
    }

    void SetSpawnMode()
    {
        ApplyModeChange(InteractionMode.Spawn);
    }

    private void ApplyModeChange(InteractionMode newMode, bool closeMenuAfterChange = true)
    {
        if (CurrentMode == newMode && !closeMenuAfterChange) // If called from start, don't do much if same
        {
            UpdateModeVisuals(); // Ensure visuals are correct on start
            return;
        }

        CurrentMode = newMode;
        Debug.Log($"Mode changed to: {CurrentMode}");

        UpdateModeVisuals();

        OnModeChanged?.Invoke(CurrentMode); // Notify other systems

        if (closeMenuAfterChange && isSideMenuOpen)
        {
            ToggleSideMenu();
        }
    }

    private void UpdateModeVisuals()
    {
        // Update State Visualizer Text
        if (stateVisualizerText != null)
        {
            stateVisualizerText.text = CurrentMode == InteractionMode.Spawn ? "Spawn Objects Mode" : "Touch Mode";
        }

        // Update Button Highlights
        Image touchBtnImage = touchModeButton?.GetComponent<Image>();
        if (touchBtnImage)
        {
            touchBtnImage.color = (CurrentMode == InteractionMode.Touch) ? highlightedButtonColor : normalButtonColor;
        }

        Image spawnBtnImage = spawnModeButton?.GetComponent<Image>();
        if (spawnBtnImage)
        {
            spawnBtnImage.color = (CurrentMode == InteractionMode.Spawn) ? highlightedButtonColor : normalButtonColor;
        }
    }

    void HandleResetButton()
    {
        if (popupMessageText) popupMessageText.text = "Are you sure you want to reset the scene?";
        if (confirmationPopupPanel) confirmationPopupPanel.SetActive(true);
        UpdateIsUIActive(); // UI is active due to popup

        // Optionally close side menu immediately
        if (isSideMenuOpen)
        {
            // Don't call ToggleSideMenu as it flips isSideMenuOpen.
            // Just start the animation to close if open.
            if (menuAnimationCoroutine != null) StopCoroutine(menuAnimationCoroutine);
            menuAnimationCoroutine = StartCoroutine(AnimateSideMenu(false)); // Animate to closed
            isSideMenuOpen = false; // Set the state to closed
            if (backgroundCatcherPanel) backgroundCatcherPanel.SetActive(false);
            // IsUIActive will be re-evaluated by AnimateSideMenu completion or by HandleConfirmYes/No
        }
    }

    void HandleConfirmYes()
    {
        Debug.Log("Reset Confirmed!");
        OnResetSceneConfirmed?.Invoke();
        if (confirmationPopupPanel) confirmationPopupPanel.SetActive(false);
        // Side menu should already be closed or in the process of closing if HandleResetButton was called
        // isSideMenuOpen should be false here if HandleResetButton closed it.
        UpdateIsUIActive();
    }

    void HandleConfirmNo()
    {
        if (confirmationPopupPanel) confirmationPopupPanel.SetActive(false);
        // Side menu state (isSideMenuOpen) remains as it was before popup.
        UpdateIsUIActive();
    }
}