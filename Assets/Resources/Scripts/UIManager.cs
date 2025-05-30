using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public enum InteractionMode
    {
        Touch,
        Spawn
    }

    // --- Global Static Properties ---
    public static InteractionMode CurrentMode { get; private set; } = InteractionMode.Touch; // Default
    public static bool IsUIActive { get; private set; } = false;

    // --- Events ---
    public static event System.Action<InteractionMode> OnModeChanged;
    public static event System.Action OnResetSceneConfirmed;

    [Header("Menu Elements")]
    public GameObject sideMenuPanel;
    public Button hamburgerButton;
    public RectTransform sideMenuRectTransform;

    [Header("Mode Controls")]
    public Button resetButton;
    public Button touchModeButton;
    public Button spawnModeButton;
    public TextMeshProUGUI stateVisualizerText;

    [Header("Mode Button Visuals")]
    public Color normalButtonColor = Color.white;
    public Color highlightedButtonColor = Color.yellow;

    [Header("Confirmation Popup")]
    public GameObject confirmationPopupPanel;
    public TextMeshProUGUI popupMessageText;
    public Button confirmYesButton;
    public Button confirmNoButton;
    public Transform spawnablesTransform;

    [Header("Side Menu Animation")]
    public float animationDuration = 0.3f;
    private bool isSideMenuOpen = false;
    private Coroutine menuAnimationCoroutine;
    private float sideMenuWidth;

    void Start()
    {
        // Initial states
        if (sideMenuPanel) sideMenuPanel.SetActive(true);
        if (sideMenuRectTransform)
        {
            sideMenuWidth = sideMenuRectTransform.rect.width;
            sideMenuRectTransform.anchoredPosition = new Vector2(-sideMenuWidth, sideMenuRectTransform.anchoredPosition.y);
        }
        else Debug.LogError("SideMenuRectTransform not assigned!");

        if (confirmationPopupPanel) confirmationPopupPanel.SetActive(false);

        // Assign button listeners
        if (hamburgerButton) hamburgerButton.onClick.AddListener(ToggleSideMenu);

        if (resetButton) resetButton.onClick.AddListener(HandleResetButton);
        if (touchModeButton) touchModeButton.onClick.AddListener(SetTouchMode);
        if (spawnModeButton) spawnModeButton.onClick.AddListener(SetSpawnMode);

        if (confirmYesButton) confirmYesButton.onClick.AddListener(HandleConfirmYes);
        if (confirmNoButton) confirmNoButton.onClick.AddListener(HandleConfirmNo);


        // Initialize Mode and UI
        ApplyModeChange(CurrentMode, false);
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
        UpdateIsUIActive();

        if (menuAnimationCoroutine != null)
        {
            StopCoroutine(menuAnimationCoroutine);
        }
        menuAnimationCoroutine = StartCoroutine(AnimateSideMenu(isSideMenuOpen));
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
        UpdateIsUIActive();
    }

    void CloseSideMenuFromBackground()
    {
        if (isSideMenuOpen)
        {
            ToggleSideMenu();
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
        if (CurrentMode == newMode && !closeMenuAfterChange)
        {
            UpdateModeVisuals();
            return;
        }

        CurrentMode = newMode;
        Debug.Log($"Mode changed to: {CurrentMode}");

        UpdateModeVisuals();

        OnModeChanged?.Invoke(CurrentMode);

        if (closeMenuAfterChange && isSideMenuOpen)
        {
            ToggleSideMenu();
        }
    }

    private void UpdateModeVisuals()
    {
        if (stateVisualizerText != null)
        {
            stateVisualizerText.text = CurrentMode == InteractionMode.Spawn ? "Spawn Objects Mode" : "Touch Mode";
        }

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
        UpdateIsUIActive();

        if (isSideMenuOpen)
        {
            // Don't call ToggleSideMenu as it flips isSideMenuOpen.
            // Just start the animation to close if open.
            if (menuAnimationCoroutine != null) StopCoroutine(menuAnimationCoroutine);
            menuAnimationCoroutine = StartCoroutine(AnimateSideMenu(false)); // Animate to closed
            isSideMenuOpen = false; // Set the state to closed
            // IsUIActive will be re-evaluated by AnimateSideMenu completion or by HandleConfirmYes/No
        }
    }

    void HandleConfirmYes()
    {
        // delete all children
        if (spawnablesTransform != null)
        {
            foreach (Transform child in spawnablesTransform)
            {
                Destroy(child.gameObject);
            }
        }

        OnResetSceneConfirmed?.Invoke(); // For other reset logic like scene reload or player reset
        if (confirmationPopupPanel) confirmationPopupPanel.SetActive(false);
        UpdateIsUIActive();
    }

    void HandleConfirmNo()
    {
        if (confirmationPopupPanel) confirmationPopupPanel.SetActive(false);
        UpdateIsUIActive();
    }
}