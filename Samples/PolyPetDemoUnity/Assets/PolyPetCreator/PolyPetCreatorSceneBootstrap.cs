using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(1000)]
[DisallowMultipleComponent]
public sealed class PolyPetCreatorSceneBootstrap : MonoBehaviour
{
    private const int InitialSeed = 5;
    private const int InitialNameSeed = 5;
    private const string CanvasRootPath = "PolyPetCreatorCanvas";
    private const string AvatarPath = CanvasRootPath + "/DesktopLayout/AvatarColumn/AvatarFrame/AvatarInset/AvatarSurface/PolyPetAvatar";
    private const string ControlCardPath = CanvasRootPath + "/DesktopLayout/ControlColumn/ControlCard";
    private const string NameTextPath = ControlCardPath + "/PetName";
    private const string NameSeedInputPath = ControlCardPath + "/NameSeedRow/NameSeedInput";
    private const string BodySeedInputPath = ControlCardPath + "/BodySeedRow/BodySeedInput";
    private const string NameSeedButtonPath = ControlCardPath + "/NameSeedRow/NameSeedButton";
    private const string BodySeedButtonPath = ControlCardPath + "/BodySeedRow/BodySeedButton";

    private static readonly FieldInfo StartSeedField = ResolveField("_startSeed");
    private static readonly FieldInfo StartNameSeedField = ResolveField("_startNameSeed");
    private static readonly FieldInfo StartSeedTypeField = ResolveField("_startSeedType");
    private static readonly FieldInfo StartNameSeedTypeField = ResolveField("_startNameSeedType");

    private static FieldInfo ResolveField(string name)
    {
        var field = typeof(PolyPetAvatar).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
            Debug.LogWarning($"PolyPetCreatorSceneBootstrap: PolyPetAvatar.{name} not found — seed defaults may not apply.");
        return field;
    }

    private static readonly Color PageBackgroundColor = new(1f, 0.965f, 0.91f, 1f);
    private static readonly Color BlobPrimaryColor = new(1f, 0.86f, 0.68f, 0.65f);
    private static readonly Color BlobSecondaryColor = new(0.98f, 0.8f, 0.66f, 0.42f);
    private static readonly Color FrameFillColor = new(1f, 0.92f, 0.8f, 1f);
    private static readonly Color FrameBorderColor = new(0.84f, 0.56f, 0.32f, 1f);
    private static readonly Color FrameInsetColor = new(1f, 0.97f, 0.9f, 1f);
    private static readonly Color CardFillColor = new(1f, 0.95f, 0.87f, 1f);
    private static readonly Color CardBorderColor = new(0.91f, 0.69f, 0.46f, 1f);
    private static readonly Color InputFillColor = new(1f, 0.99f, 0.96f, 1f);
    private static readonly Color InputBorderColor = new(0.9f, 0.76f, 0.58f, 1f);
    private static readonly Color ButtonFillColor = new(1f, 0.77f, 0.43f, 1f);
    private static readonly Color ButtonHighlightColor = new(1f, 0.82f, 0.5f, 1f);
    private static readonly Color ButtonPressedColor = new(0.93f, 0.68f, 0.36f, 1f);
    private static readonly Color TextPrimaryColor = new(0.39f, 0.22f, 0.12f, 1f);
    private static readonly Color TextSecondaryColor = new(0.53f, 0.33f, 0.21f, 1f);
    private static readonly Color PlaceholderColor = new(0.6f, 0.44f, 0.31f, 0.35f);

    private PolyPetAvatar _avatar;
    private TextMeshProUGUI _nameText;
    private PolyPetCreatorPanel _panel;
    private TMP_InputField _nameSeedInput;
    private TMP_InputField _bodySeedInput;
    private Button _nameSeedButton;
    private Button _bodySeedButton;

    private void Awake()
    {
        EnsureSceneHierarchy();
        ConfigureAvatarStartState();
    }

    private void Start()
    {
        if (_avatar == null)
            return;

        InstallNameBinding();
    }

    private void EnsureSceneHierarchy()
    {
        if (TryCaptureSceneReferences())
        {
            WireCapturedReferences();
            return;
        }

        if (transform.childCount > 0)
            ClearRuntimeChildren();

        BuildScene();

        if (!TryCaptureSceneReferences())
            Debug.LogError("PolyPetCreatorSceneBootstrap failed to build the required runtime UI hierarchy.");

        WireCapturedReferences();
    }

    private void BuildScene()
    {
        var fontAsset = ResolveFontAsset();

        EnsureEventSystem();

        var canvasRoot = CreateUiObject("PolyPetCreatorCanvas", transform);
        StretchToParent(canvasRoot);

        var canvas = canvasRoot.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        var scaler = canvasRoot.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasRoot.gameObject.AddComponent<GraphicRaycaster>();

        CreateBackdrop(canvasRoot);

        var layoutRoot = CreateUiObject("DesktopLayout", canvasRoot);
        Stretch(layoutRoot, 48f, 48f, 48f, 48f);

        var layoutGroup = layoutRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 32f;
        layoutGroup.padding = new RectOffset(24, 24, 24, 24);
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = true;
        layoutGroup.childForceExpandWidth = false;

        var avatarColumn = CreateUiObject("AvatarColumn", layoutRoot);
        var avatarColumnLayout = avatarColumn.gameObject.AddComponent<LayoutElement>();
        avatarColumnLayout.minWidth = 560f;
        avatarColumnLayout.preferredWidth = 860f;
        avatarColumnLayout.flexibleWidth = 1f;
        avatarColumnLayout.minHeight = 620f;

        var avatarFrame = CreatePanel(
            "AvatarFrame",
            avatarColumn,
            FrameFillColor,
            FrameBorderColor,
            36f,
            12f,
            false);
        StretchToParent((RectTransform)avatarFrame.transform);

        var avatarInset = CreatePanel(
            "AvatarInset",
            (RectTransform)avatarFrame.transform,
            FrameInsetColor,
            CardBorderColor,
            28f,
            4f,
            false);
        Stretch((RectTransform)avatarInset.transform, 18f, 18f, 18f, 18f);

        var avatarSurface = CreateUiObject("AvatarSurface", (RectTransform)avatarInset.transform);
        Stretch(avatarSurface, 34f, 34f, 34f, 34f);

        var avatarRect = CreateUiObject("PolyPetAvatar", avatarSurface);
        StretchToParent(avatarRect);
        _avatar = avatarRect.gameObject.AddComponent<PolyPetAvatar>();

        var controlColumn = CreateUiObject("ControlColumn", layoutRoot);
        var controlLayout = controlColumn.gameObject.AddComponent<LayoutElement>();
        controlLayout.minWidth = 400f;
        controlLayout.preferredWidth = 420f;
        controlLayout.flexibleWidth = 0f;
        controlLayout.minHeight = 620f;
        controlLayout.preferredHeight = 620f;

        var controlCard = CreatePanel(
            "ControlCard",
            controlColumn,
            CardFillColor,
            CardBorderColor,
            32f,
            8f,
            false);
        StretchToParent((RectTransform)controlCard.transform);

        var cardLayout = controlCard.gameObject.AddComponent<VerticalLayoutGroup>();
        cardLayout.spacing = 22f;
        cardLayout.padding = new RectOffset(28, 28, 32, 28);
        cardLayout.childAlignment = TextAnchor.UpperCenter;
        cardLayout.childControlHeight = true;
        cardLayout.childControlWidth = true;
        cardLayout.childForceExpandHeight = false;
        cardLayout.childForceExpandWidth = true;

        CreateText(
            "PetName",
            (RectTransform)controlCard.transform,
            fontAsset,
            50f,
            TextPrimaryColor,
            TextAlignmentOptions.Center);
        _nameText = FindComponentAtPath<TextMeshProUGUI>(NameTextPath);
        _nameText.enableAutoSizing = true;
        _nameText.fontSizeMin = 28f;
        _nameText.fontSizeMax = 50f;
        _nameText.fontStyle = FontStyles.Bold;
        var nameLayout = _nameText.gameObject.AddComponent<LayoutElement>();
        nameLayout.preferredHeight = 88f;
        nameLayout.minHeight = 72f;

        var nameSeedRow = CreateSeedRow((RectTransform)controlCard.transform, fontAsset, "Name Seed");
        var bodySeedRow = CreateSeedRow((RectTransform)controlCard.transform, fontAsset, "Body Seed");

        _nameSeedInput = nameSeedRow.InputField;
        _bodySeedInput = bodySeedRow.InputField;
        _nameSeedButton = nameSeedRow.Button;
        _bodySeedButton = bodySeedRow.Button;

        _panel = controlCard.gameObject.AddComponent<PolyPetCreatorPanel>();
    }

    private bool TryCaptureSceneReferences()
    {
        _avatar = FindComponentAtPath<PolyPetAvatar>(AvatarPath);
        _nameText = FindComponentAtPath<TextMeshProUGUI>(NameTextPath);
        _panel = FindComponentAtPath<PolyPetCreatorPanel>(ControlCardPath);
        _nameSeedInput = FindComponentAtPath<TMP_InputField>(NameSeedInputPath);
        _bodySeedInput = FindComponentAtPath<TMP_InputField>(BodySeedInputPath);
        _nameSeedButton = FindComponentAtPath<Button>(NameSeedButtonPath);
        _bodySeedButton = FindComponentAtPath<Button>(BodySeedButtonPath);

        return _avatar != null
            && _nameText != null
            && _panel != null
            && _nameSeedInput != null
            && _bodySeedInput != null
            && _nameSeedButton != null
            && _bodySeedButton != null;
    }

    private void WireCapturedReferences()
    {
        if (_panel == null || _avatar == null)
            return;

        _panel.Initialize(
            _avatar,
            _nameSeedInput,
            _bodySeedInput,
            _nameSeedButton,
            _bodySeedButton);
    }

    private void InstallNameBinding()
    {
        if (_nameText == null || _avatar == null)
            return;

        var nameBinding = _nameText.gameObject.GetComponent<PolyPetName>();
        if (nameBinding == null)
            nameBinding = _nameText.gameObject.AddComponent<PolyPetName>();

        nameBinding.Avatar = _avatar;
        _nameText.text = _avatar.Data != null ? _avatar.Data.Name ?? string.Empty : string.Empty;
    }

    private void ConfigureAvatarStartState()
    {
        if (_avatar == null)
            return;

        StartSeedField?.SetValue(_avatar, InitialSeed);
        StartNameSeedField?.SetValue(_avatar, InitialNameSeed);
        StartSeedTypeField?.SetValue(_avatar, StartSeedType.Fixed);
        StartNameSeedTypeField?.SetValue(_avatar, StartSeedType.Fixed);

        // Rehydrated hierarchies may already have live seed state; snap them back immediately.
        if (_avatar.Data != null || _avatar.Seed.HasValue || _avatar.NameSeed.HasValue)
        {
            if (_avatar.Seed != InitialSeed)
                _avatar.Seed = InitialSeed;

            if (_avatar.NameSeed != InitialNameSeed)
                _avatar.NameSeed = InitialNameSeed;
        }
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
        eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
    }

    private void CreateBackdrop(RectTransform canvasRoot)
    {
        var background = CreatePanel(
            "Background",
            canvasRoot,
            PageBackgroundColor,
            PageBackgroundColor,
            0f,
            0f,
            false);
        StretchToParent((RectTransform)background.transform);

        var blobLeft = CreatePanel(
            "BlobTopLeft",
            (RectTransform)background.transform,
            BlobPrimaryColor,
            BlobPrimaryColor,
            170f,
            0f,
            false);
        SetAnchoredRect((RectTransform)blobLeft.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, -140f), new Vector2(340f, 340f));

        var blobRight = CreatePanel(
            "BlobBottomRight",
            (RectTransform)background.transform,
            BlobSecondaryColor,
            BlobSecondaryColor,
            200f,
            0f,
            false);
        SetAnchoredRect((RectTransform)blobRight.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-210f, 130f), new Vector2(400f, 400f));
    }

    private SeedRow CreateSeedRow(RectTransform parent, TMP_FontAsset fontAsset, string label)
    {
        var row = CreateUiObject(label.Replace(" ", string.Empty) + "Row", parent);
        var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 12f;
        rowLayout.padding = new RectOffset(0, 0, 0, 0);
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlHeight = true;
        rowLayout.childControlWidth = true;
        rowLayout.childForceExpandHeight = false;
        rowLayout.childForceExpandWidth = false;

        var rowElement = row.gameObject.AddComponent<LayoutElement>();
        rowElement.preferredHeight = 64f;
        rowElement.minHeight = 64f;

        var labelText = CreateText(
            label.Replace(" ", string.Empty) + "Label",
            row,
            fontAsset,
            23f,
            TextSecondaryColor,
            TextAlignmentOptions.MidlineLeft);
        labelText.text = label;
        labelText.fontStyle = FontStyles.Bold;
        var labelLayout = labelText.gameObject.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 96f;
        labelLayout.minWidth = 96f;

        var inputField = CreateInputField(label.Replace(" ", string.Empty) + "Input", row, fontAsset);
        var inputLayout = inputField.gameObject.AddComponent<LayoutElement>();
        inputLayout.flexibleWidth = 1f;
        inputLayout.minWidth = 120f;
        inputLayout.preferredHeight = 64f;

        var button = CreateButton(label.Replace(" ", string.Empty) + "Button", row, fontAsset, "Reroll");
        var buttonLayout = button.gameObject.AddComponent<LayoutElement>();
        buttonLayout.preferredWidth = 88f;
        buttonLayout.minWidth = 88f;
        buttonLayout.preferredHeight = 64f;

        return new SeedRow(inputField, button);
    }

    private TMP_InputField CreateInputField(string name, RectTransform parent, TMP_FontAsset fontAsset)
    {
        var inputRoot = CreateUiObject(name, parent);
        var background = inputRoot.gameObject.AddComponent<RoundedPanelGraphic>();
        background.color = InputFillColor;
        background.BorderColor = InputBorderColor;
        background.CornerRadius = 18f;
        background.BorderThickness = 3f;
        background.SegmentsPerCorner = 8;
        background.raycastTarget = true;

        var inputField = inputRoot.gameObject.AddComponent<TMP_InputField>();
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
        inputField.lineType = TMP_InputField.LineType.SingleLine;
        inputField.caretWidth = 3;
        inputField.customCaretColor = true;
        inputField.caretColor = TextPrimaryColor;
        inputField.selectionColor = new Color(1f, 0.8f, 0.45f, 0.35f);
        inputField.targetGraphic = background;

        var textArea = CreateUiObject("Text Area", inputRoot);
        Stretch(textArea, 16f, 16f, 10f, 10f);
        textArea.gameObject.AddComponent<RectMask2D>();

        var text = CreateText("Text", textArea, fontAsset, 24f, TextPrimaryColor, TextAlignmentOptions.MidlineLeft);
        text.enableWordWrapping = false;
        text.richText = false;
        StretchToParent((RectTransform)text.transform);

        var placeholder = CreateText("Placeholder", textArea, fontAsset, 24f, PlaceholderColor, TextAlignmentOptions.MidlineLeft);
        placeholder.text = "Enter seed";
        placeholder.enableWordWrapping = false;
        placeholder.fontStyle = FontStyles.Italic;
        StretchToParent((RectTransform)placeholder.transform);

        inputField.textViewport = textArea;
        inputField.textComponent = text;
        inputField.placeholder = placeholder;
        return inputField;
    }

    private Button CreateButton(string name, RectTransform parent, TMP_FontAsset fontAsset, string label)
    {
        var buttonRoot = CreateUiObject(name, parent);
        var background = buttonRoot.gameObject.AddComponent<RoundedPanelGraphic>();
        background.color = ButtonFillColor;
        background.BorderColor = FrameBorderColor;
        background.CornerRadius = 18f;
        background.BorderThickness = 3f;
        background.SegmentsPerCorner = 8;
        background.raycastTarget = true;

        var button = buttonRoot.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = new ColorBlock
        {
            normalColor = ButtonFillColor,
            highlightedColor = ButtonHighlightColor,
            pressedColor = ButtonPressedColor,
            selectedColor = ButtonHighlightColor,
            disabledColor = new Color(0.92f, 0.86f, 0.78f, 1f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        var labelText = CreateText("Label", buttonRoot, fontAsset, 18f, TextPrimaryColor, TextAlignmentOptions.Center);
        labelText.text = label;
        labelText.fontStyle = FontStyles.Bold;
        labelText.enableAutoSizing = true;
        labelText.fontSizeMin = 14f;
        labelText.fontSizeMax = 18f;
        StretchToParent((RectTransform)labelText.transform);

        return button;
    }

    private RoundedPanelGraphic CreatePanel(
        string name,
        RectTransform parent,
        Color fillColor,
        Color borderColor,
        float cornerRadius,
        float borderThickness,
        bool raycastTarget)
    {
        var rectTransform = CreateUiObject(name, parent);
        var graphic = rectTransform.gameObject.AddComponent<RoundedPanelGraphic>();
        graphic.color = fillColor;
        graphic.BorderColor = borderColor;
        graphic.CornerRadius = cornerRadius;
        graphic.BorderThickness = borderThickness;
        graphic.SegmentsPerCorner = 10;
        graphic.raycastTarget = raycastTarget;
        return graphic;
    }

    private static TextMeshProUGUI CreateText(
        string name,
        RectTransform parent,
        TMP_FontAsset fontAsset,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment)
    {
        var rectTransform = CreateUiObject(name, parent);
        var text = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = fontAsset;
        text.text = name == "PetName" ? "PolyPet" : string.Empty;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        return text;
    }

    private static TMP_FontAsset ResolveFontAsset()
    {
        if (TMP_Settings.defaultFontAsset != null)
            return TMP_Settings.defaultFontAsset;

        var fallback = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (fallback != null)
            return fallback;

        Debug.LogWarning("PolyPetCreatorSceneBootstrap could not find a TMP font asset. The sample UI text may render incorrectly.");
        return null;
    }

    private static RectTransform CreateUiObject(string name, Transform parent)
    {
        var gameObject = new GameObject(name, typeof(RectTransform));
        var rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.localScale = Vector3.one;
        return rectTransform;
    }

    private T FindComponentAtPath<T>(string path) where T : Component
    {
        var target = transform.Find(path);
        return target != null ? target.GetComponent<T>() : null;
    }

    private void ClearRuntimeChildren()
    {
        while (transform.childCount > 0)
        {
            var child = transform.GetChild(0).gameObject;
            DestroyImmediate(child);
        }
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        Stretch(rectTransform, 0f, 0f, 0f, 0f);
    }

    private static void Stretch(RectTransform rectTransform, float left, float right, float top, float bottom)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(left, bottom);
        rectTransform.offsetMax = new Vector2(-right, -top);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void SetAnchoredRect(
        RectTransform rectTransform,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
    }

    private readonly struct SeedRow
    {
        public SeedRow(TMP_InputField inputField, Button button)
        {
            InputField = inputField;
            Button = button;
        }

        public TMP_InputField InputField { get; }
        public Button Button { get; }
    }
}
