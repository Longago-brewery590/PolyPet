using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PolyPet;
using Random = System.Random;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum StartSeedType
{
    None,
    Fixed,
    Random
}

[Serializable]
public struct NullableInt
{
    [SerializeField] private bool _hasValue;
    [SerializeField] private int _value;

    public bool HasValue => _hasValue;
    public int Value => _hasValue ? _value : throw new InvalidOperationException("NullableInt does not have a value.");

    public NullableInt(int? nullableInt)
    {
        _hasValue = nullableInt.HasValue;
        _value = nullableInt.GetValueOrDefault();
    }

    public int? ToNullable()
    {
        return _hasValue ? _value : (int?)null;
    }

    public static implicit operator NullableInt(int? nullableInt)
    {
        return new NullableInt(nullableInt);
    }

    public static implicit operator int?(NullableInt nullableInt)
    {
        return nullableInt.ToNullable();
    }
}

[Serializable]
public sealed class AvatarNullableIntEvent : UnityEvent<PolyPetAvatar, NullableInt>
{
}

public delegate void SeedChangedCallback(PolyPetAvatar avatar, NullableInt seed);
public delegate void NameSeedChangedCallback(PolyPetAvatar avatar, NullableInt nameSeed);

public class PolyPetAvatar : MonoBehaviour
{
    private const float MouthLineWidth = 2f;

    [SerializeField] private int _startSeed;
    [SerializeField] private int _startNameSeed;
    [SerializeField] private StartSeedType _startSeedType = StartSeedType.Fixed;
    [SerializeField] private StartSeedType _startNameSeedType = StartSeedType.Fixed;
    [SerializeField] private Vector2 _frameSize = new Vector2(3f, 3f);
    [SerializeField] private AvatarNullableIntEvent _seedChanged = new AvatarNullableIntEvent();
    [SerializeField] private AvatarNullableIntEvent _nameSeedChanged = new AvatarNullableIntEvent();

    private int? _seed;
    private int? _nameSeed;
    private PetState _state = PetState.Idle;
    private float _time;
    private float _petTime;
    private Mesh _mesh;
    private Material _material;
    private MeshFilter _sceneMeshFilter;
    private MeshRenderer _sceneMeshRenderer;
    private PolyPetAvatarGraphic _uiGraphic;

    public AvatarNullableIntEvent SeedChanged => _seedChanged;
    public AvatarNullableIntEvent NameSeedChanged => _nameSeedChanged;
    public Vector2 FrameSize
    {
        get => _frameSize;
        set => _frameSize = new Vector2(Mathf.Max(0f, value.x), Mathf.Max(0f, value.y));
    }

    public void AddSeedChangedListener(SeedChangedCallback onSeedChanged)
    {
        _seedChanged.AddListener(onSeedChanged.Invoke);
    }

    public void RemoveSeedChangedListener(SeedChangedCallback onSeedChanged)
    {
        _seedChanged.RemoveListener(onSeedChanged.Invoke);
    }

    public void AddNameSeedChangedListener(NameSeedChangedCallback onNameSeedChanged)
    {
        _nameSeedChanged.AddListener(onNameSeedChanged.Invoke);
    }

    public void RemoveNameSeedChangedListener(NameSeedChangedCallback onNameSeedChanged)
    {
        _nameSeedChanged.RemoveListener(onNameSeedChanged.Invoke);
    }

    public int? Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            RefreshDataAndMesh();
            EmitSeedChanged();
        }
    }

    public int? NameSeed
    {
        get => _nameSeed;
        set
        {
            _nameSeed = value;
            RefreshDataAndMesh();
            EmitNameSeedChanged();
        }
    }

    public PolyPetData Data { get; private set; }

    public void RandomizeSeed() { Seed = new Random().Next(); }
    public void RandomizeNameSeed() { NameSeed = new Random().Next(); }

    void OnEnable()
    {
        RefreshRenderMode();
        MarkUiGraphicDirty();
    }

    void Start()
    {
        var shader = Shader.Find("Sprites/Default");
        if (shader != null)
            _material = new Material(shader);

        if (_startSeedType == StartSeedType.Fixed) _seed = _startSeed;
        else if (_startSeedType == StartSeedType.Random) _seed = new Random().Next();

        if (_startNameSeedType == StartSeedType.Fixed) _nameSeed = _startNameSeed;
        else if (_startNameSeedType == StartSeedType.Random) _nameSeed = new Random().Next();

        RefreshDataAndMesh();
        RefreshRenderMode();
        EmitSeedChanged();
        EmitNameSeedChanged();
    }

    void Update()
    {
        _time += Time.deltaTime;

        if (_state == PetState.BeingPet && _time - _petTime > 0.5f)
            _state = PetState.Idle;

        RefreshRenderMode();
        if (IsUiRenderMode)
        {
            MarkUiGraphicDirty();
        }
        else
        {
            RefreshSceneMesh();
        }

        if (Data.Body.Vertices == null)
            return;

        if (TryGetPressedPetPosition(out var petPosition))
        {
            var hitRadius = Data.Body.Scale * 1.5f;
            if (petPosition.sqrMagnitude < hitRadius * hitRadius)
            {
                _state = PetState.BeingPet;
                _petTime = _time;
            }
        }
    }

    private void RefreshSceneMesh()
    {
        if (_mesh == null)
            _mesh = new Mesh();
        else
            _mesh.Clear();

        var vertices = new System.Collections.Generic.List<Vector3>();
        var colors = new System.Collections.Generic.List<UnityEngine.Color>();
        var triangles = new System.Collections.Generic.List<int>();
        var frameRect = GetResolvedFrameRect();
        var frameLayout = PolyPetLayout.CreateFrameLayout(Data, frameRect.width, frameRect.height);
        var frame = GetCurrentFrame();

        AddShapeToMesh(Data.Body, vertices, colors, triangles, frameRect, frameLayout, frame);
        AddShapeToMesh(Data.Head, vertices, colors, triangles, frameRect, frameLayout, frame);
        foreach (var ear in Data.Ears) AddShapeToMesh(ear, vertices, colors, triangles, frameRect, frameLayout, frame);
        foreach (var eye in Data.Eyes) AddShapeToMesh(eye, vertices, colors, triangles, frameRect, frameLayout, frame);
        AddMouthToMesh(Data.Mouth, vertices, colors, triangles, frameRect, frameLayout, frame);
        foreach (var limb in Data.Limbs) AddShapeToMesh(limb, vertices, colors, triangles, frameRect, frameLayout, frame);
        if (Data.Tail.Vertices != null && Data.Tail.Vertices.Length >= 3)
            AddShapeToMesh(Data.Tail, vertices, colors, triangles, frameRect, frameLayout, frame);

        _mesh.SetVertices(vertices);
        _mesh.SetColors(colors);
        _mesh.SetTriangles(triangles, 0);
        _mesh.RecalculateBounds();
        SyncSceneRenderer();
    }

    private void AddShapeToMesh(ShapePart part,
        System.Collections.Generic.List<Vector3> vertices,
        System.Collections.Generic.List<UnityEngine.Color> colors,
        System.Collections.Generic.List<int> triangles,
        Rect frameRect,
        PetFrameLayout frameLayout,
        AnimationFrame frame)
    {
        if (part.Vertices == null || part.Vertices.Length < 3) return;

        int startIndex = vertices.Count;
        var color = new UnityEngine.Color(
            part.Color.R / 255f, part.Color.G / 255f, part.Color.B / 255f);

        for (int i = 0; i < part.Vertices.Length; i++)
        {
            var petPoint = new Vector2(
                part.Position.X + part.Vertices[i].X,
                part.Position.Y + part.Vertices[i].Y);
            var position = TransformToFrameSpace(petPoint, frameRect, frameLayout, frame);

            vertices.Add(new Vector3(position.x, position.y, 0f));
            colors.Add(color);
        }

        // Fan triangulation from first vertex
        for (int i = 1; i < part.Vertices.Length - 1; i++)
        {
            triangles.Add(startIndex);
            triangles.Add(startIndex + i);
            triangles.Add(startIndex + i + 1);
        }
    }

    private void AddMouthToMesh(ShapePart mouth,
        System.Collections.Generic.List<Vector3> vertices,
        System.Collections.Generic.List<UnityEngine.Color> colors,
        System.Collections.Generic.List<int> triangles,
        Rect frameRect,
        PetFrameLayout frameLayout,
        AnimationFrame frame)
    {
        if (mouth.Vertices == null || mouth.Vertices.Length < 2) return;

        var color = new UnityEngine.Color(
            mouth.Color.R / 255f, mouth.Color.G / 255f, mouth.Color.B / 255f);

        for (int i = 0; i < mouth.Vertices.Length - 1; i++)
        {
            var start = new Vector2(
                mouth.Position.X + mouth.Vertices[i].X,
                mouth.Position.Y + mouth.Vertices[i].Y);
            var end = new Vector2(
                mouth.Position.X + mouth.Vertices[i + 1].X,
                mouth.Position.Y + mouth.Vertices[i + 1].Y);

            var segment = end - start;
            if (segment.sqrMagnitude <= Mathf.Epsilon)
                continue;

            var normal = new Vector2(-segment.y, segment.x).normalized * (MouthLineWidth * 0.5f);
            int startIndex = vertices.Count;

            var transformedStartLeft = TransformToFrameSpace(start - normal, frameRect, frameLayout, frame);
            var transformedStartRight = TransformToFrameSpace(start + normal, frameRect, frameLayout, frame);
            var transformedEndRight = TransformToFrameSpace(end + normal, frameRect, frameLayout, frame);
            var transformedEndLeft = TransformToFrameSpace(end - normal, frameRect, frameLayout, frame);

            vertices.Add(new Vector3(transformedStartLeft.x, transformedStartLeft.y, 0f));
            vertices.Add(new Vector3(transformedStartRight.x, transformedStartRight.y, 0f));
            vertices.Add(new Vector3(transformedEndRight.x, transformedEndRight.y, 0f));
            vertices.Add(new Vector3(transformedEndLeft.x, transformedEndLeft.y, 0f));

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

            triangles.Add(startIndex);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 3);
        }
    }

    private void RefreshDataAndMesh()
    {
        Data = _seed.HasValue || _nameSeed.HasValue
            ? PolyPetGenerator.Create(_seed ?? 0, _nameSeed)
            : CreateEmptyData();

        if (!IsUiRenderMode)
            RefreshSceneMesh();

        MarkUiGraphicDirty();
    }

    private void EmitSeedChanged()
    {
        _seedChanged.Invoke(this, _seed);
    }

    private void EmitNameSeedChanged()
    {
        _nameSeedChanged.Invoke(this, _nameSeed);
    }

    private static PolyPetData CreateEmptyData()
    {
        return new PolyPetData
        {
            Eyes = Array.Empty<ShapePart>(),
            Ears = Array.Empty<ShapePart>(),
            Limbs = Array.Empty<ShapePart>()
        };
    }

    private AnimationFrame GetCurrentFrame()
    {
        return PolyPetAnimation.GetFrame(_state, _time, _time - _petTime);
    }

    private bool IsUiRenderMode => TryGetUiRenderContext(out _, out _);

    private Vector3 GetLayoutOrigin(Rect frameRect, PetFrameLayout frameLayout, AnimationFrame frame)
    {
        return new Vector3(
            frameRect.xMin + frameLayout.OffsetX + frame.PositionOffset.X * frameLayout.Scale,
            frameRect.yMin + frameLayout.OffsetY + frame.PositionOffset.Y * frameLayout.Scale,
            0f);
    }

    private static Vector3 GetLayoutScale(PetFrameLayout frameLayout, AnimationFrame frame)
    {
        return new Vector3(
            frameLayout.Scale * frame.ScaleX,
            frameLayout.Scale * frame.ScaleY,
            1f);
    }

    private Rect GetResolvedFrameRect()
    {
        if (TryGetUiRenderContext(out var rectTransform, out _))
        {
            var rect = rectTransform.rect;
            return new Rect(
                rect.xMin,
                rect.yMin,
                Mathf.Max(rect.width, 0f),
                Mathf.Max(rect.height, 0f));
        }

        var frameSize = FrameSize;
        return new Rect(
            -frameSize.x * 0.5f,
            -frameSize.y * 0.5f,
            Mathf.Max(frameSize.x, 0f),
            Mathf.Max(frameSize.y, 0f));
    }

    private void RefreshRenderMode()
    {
        if (TryGetUiRenderContext(out _, out _))
        {
            if (_uiGraphic == null)
                _uiGraphic = GetOrAddComponent<PolyPetAvatarGraphic>();

            _uiGraphic.Bind(this);
            _uiGraphic.enabled = true;
            DisableSceneRenderer();
            return;
        }

        if (_uiGraphic != null)
            _uiGraphic.enabled = false;

        EnsureSceneRenderer();
    }

    private void EnsureSceneRenderer()
    {
        if (_sceneMeshFilter == null)
            _sceneMeshFilter = GetOrAddComponent<MeshFilter>();

        if (_sceneMeshRenderer == null)
            _sceneMeshRenderer = GetOrAddComponent<MeshRenderer>();

        _sceneMeshRenderer.enabled = true;
        SyncSceneRenderer();
    }

    private void DisableSceneRenderer()
    {
        if (_sceneMeshRenderer != null)
            _sceneMeshRenderer.enabled = false;
    }

    private void SyncSceneRenderer()
    {
        if (_sceneMeshFilter != null)
            _sceneMeshFilter.sharedMesh = _mesh;

        if (_sceneMeshRenderer != null)
            _sceneMeshRenderer.sharedMaterial = _material;
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        var component = GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();

        return component;
    }

    private void MarkUiGraphicDirty()
    {
        if (_uiGraphic != null && _uiGraphic.enabled)
            _uiGraphic.RefreshGraphic();
    }

    private bool TryGetRectTransform(out RectTransform rectTransform)
    {
        rectTransform = transform as RectTransform;
        return rectTransform != null;
    }

    private bool TryGetUiRenderContext(out RectTransform rectTransform, out Canvas canvas)
    {
        if (TryGetRectTransform(out rectTransform))
        {
            canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null)
                return true;
        }

        canvas = null;
        return false;
    }

    private bool TryGetPressedPetPosition(out Vector2 petPosition)
    {
        if (TryGetPressedPointerLocalPosition(out var localPosition))
        {
            petPosition = GetPetLocalPosition(localPosition, GetCurrentFrame());
            return true;
        }

        petPosition = default;
        return false;
    }

    private Vector2 GetPetLocalPosition(Vector2 localPosition, AnimationFrame frame)
    {
        var frameRect = GetResolvedFrameRect();
        var frameLayout = PolyPetLayout.CreateFrameLayout(Data, frameRect.width, frameRect.height);
        var origin = GetLayoutOrigin(frameRect, frameLayout, frame);
        var scale = GetLayoutScale(frameLayout, frame);

        if (Mathf.Approximately(scale.x, 0f) || Mathf.Approximately(scale.y, 0f))
            return localPosition - new Vector2(origin.x, origin.y);

        return new Vector2(
            (localPosition.x - origin.x) / scale.x,
            (localPosition.y - origin.y) / scale.y);
    }

    private bool TryGetPressedPointerLocalPosition(out Vector2 localPosition)
    {
#if ENABLE_INPUT_SYSTEM
        return TryGetPressedPointerLocalPositionInputSystem(out localPosition);
#elif ENABLE_LEGACY_INPUT_MANAGER
        return TryGetPressedPointerLocalPositionLegacy(out localPosition);
#else
        localPosition = default;
        return false;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private bool TryGetPressedPointerLocalPositionInputSystem(out Vector2 localPosition)
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return TryGetScreenPointLocalPosition(Mouse.current.position.ReadValue(), out localPosition);

        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                    return TryGetScreenPointLocalPosition(touch.position.ReadValue(), out localPosition);
            }
        }

        localPosition = default;
        return false;
    }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
    private bool TryGetPressedPointerLocalPositionLegacy(out Vector2 localPosition)
    {
        if (Input.GetMouseButtonDown(0))
            return TryGetScreenPointLocalPosition(Input.mousePosition, out localPosition);

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                return TryGetScreenPointLocalPosition(touch.position, out localPosition);
        }

        localPosition = default;
        return false;
    }
#endif

    private bool TryGetScreenPointLocalPosition(Vector2 screenPoint, out Vector2 localPosition)
    {
        if (TryGetUiRenderContext(out var rectTransform, out var canvas))
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    screenPoint,
                    GetRectTransformEventCamera(canvas),
                    out localPosition)
                && rectTransform.rect.Contains(localPosition))
            {
                return true;
            }

            localPosition = default;
            return false;
        }

        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            localPosition = default;
            return false;
        }

        var avatarPlane = new Plane(transform.forward, transform.position);
        var ray = mainCamera.ScreenPointToRay(screenPoint);
        if (!avatarPlane.Raycast(ray, out var hitDistance))
        {
            localPosition = default;
            return false;
        }

        var worldPosition = ray.GetPoint(hitDistance);
        var local3 = transform.InverseTransformPoint(worldPosition);
        localPosition = new Vector2(local3.x, local3.y);
        return true;
    }

    private static Camera GetRectTransformEventCamera(Canvas canvas)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        if (canvas.worldCamera != null)
            return canvas.worldCamera;

        var rootCanvas = canvas.rootCanvas;
        return rootCanvas != null ? rootCanvas.worldCamera : null;
    }

    internal bool HasRenderableData => Data.Body.Vertices != null;

    internal void PopulateUiMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        if (!HasRenderableData)
            return;

        var frameRect = GetResolvedFrameRect();
        var frameLayout = PolyPetLayout.CreateFrameLayout(Data, frameRect.width, frameRect.height);
        var frame = GetCurrentFrame();

        AddShapeToUiMesh(vertexHelper, Data.Body, frameRect, frameLayout, frame);
        AddShapeToUiMesh(vertexHelper, Data.Head, frameRect, frameLayout, frame);
        foreach (var ear in Data.Ears) AddShapeToUiMesh(vertexHelper, ear, frameRect, frameLayout, frame);
        foreach (var eye in Data.Eyes) AddShapeToUiMesh(vertexHelper, eye, frameRect, frameLayout, frame);
        AddMouthToUiMesh(vertexHelper, Data.Mouth, frameRect, frameLayout, frame);
        foreach (var limb in Data.Limbs) AddShapeToUiMesh(vertexHelper, limb, frameRect, frameLayout, frame);
        if (Data.Tail.Vertices != null && Data.Tail.Vertices.Length >= 3)
            AddShapeToUiMesh(vertexHelper, Data.Tail, frameRect, frameLayout, frame);
    }

    private void AddShapeToUiMesh(
        VertexHelper vertexHelper,
        ShapePart part,
        Rect frameRect,
        PetFrameLayout frameLayout,
        AnimationFrame frame)
    {
        if (part.Vertices == null || part.Vertices.Length < 3)
            return;

        var color = new UnityEngine.Color32(part.Color.R, part.Color.G, part.Color.B, part.Color.A);
        var startIndex = vertexHelper.currentVertCount;

        for (var i = 0; i < part.Vertices.Length; i++)
        {
            var petPoint = new Vector2(
                part.Position.X + part.Vertices[i].X,
                part.Position.Y + part.Vertices[i].Y);
            AddUiVertex(vertexHelper, TransformToFrameSpace(petPoint, frameRect, frameLayout, frame), color);
        }

        for (var i = 1; i < part.Vertices.Length - 1; i++)
            vertexHelper.AddTriangle(startIndex, startIndex + i, startIndex + i + 1);
    }

    private void AddMouthToUiMesh(
        VertexHelper vertexHelper,
        ShapePart mouth,
        Rect frameRect,
        PetFrameLayout frameLayout,
        AnimationFrame frame)
    {
        if (mouth.Vertices == null || mouth.Vertices.Length < 2)
            return;

        var color = new UnityEngine.Color32(mouth.Color.R, mouth.Color.G, mouth.Color.B, mouth.Color.A);

        for (var i = 0; i < mouth.Vertices.Length - 1; i++)
        {
            var start = new Vector2(
                mouth.Position.X + mouth.Vertices[i].X,
                mouth.Position.Y + mouth.Vertices[i].Y);
            var end = new Vector2(
                mouth.Position.X + mouth.Vertices[i + 1].X,
                mouth.Position.Y + mouth.Vertices[i + 1].Y);

            var segment = end - start;
            if (segment.sqrMagnitude <= Mathf.Epsilon)
                continue;

            var normal = new Vector2(-segment.y, segment.x).normalized * (MouthLineWidth * 0.5f);
            var startIndex = vertexHelper.currentVertCount;

            AddUiVertex(vertexHelper, TransformToFrameSpace(start - normal, frameRect, frameLayout, frame), color);
            AddUiVertex(vertexHelper, TransformToFrameSpace(start + normal, frameRect, frameLayout, frame), color);
            AddUiVertex(vertexHelper, TransformToFrameSpace(end + normal, frameRect, frameLayout, frame), color);
            AddUiVertex(vertexHelper, TransformToFrameSpace(end - normal, frameRect, frameLayout, frame), color);

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }
    }

    private void AddUiVertex(VertexHelper vertexHelper, Vector2 position, UnityEngine.Color32 color)
    {
        var vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.color = color;
        vertexHelper.AddVert(vertex);
    }

    private Vector2 TransformToFrameSpace(
        Vector2 point,
        Rect frameRect,
        PetFrameLayout frameLayout,
        AnimationFrame frame)
    {
        var origin = GetLayoutOrigin(frameRect, frameLayout, frame);
        var scale = GetLayoutScale(frameLayout, frame);

        return new Vector2(
            origin.x + point.x * scale.x,
            origin.y + point.y * scale.y);
    }

    void OnRectTransformDimensionsChange()
    {
        MarkUiGraphicDirty();
    }

    void OnDestroy()
    {
        if (_mesh != null) Destroy(_mesh);
        if (_material != null) Destroy(_material);
    }
}

[DisallowMultipleComponent]
internal sealed class PolyPetAvatarGraphic : MaskableGraphic
{
    private PolyPetAvatar _avatar;

    public void Bind(PolyPetAvatar avatar)
    {
        if (_avatar == avatar)
            return;

        _avatar = avatar;
        raycastTarget = false;
        RefreshGraphic();
    }

    public void RefreshGraphic()
    {
        SetVerticesDirty();
        SetMaterialDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        if (_avatar == null || !_avatar.HasRenderableData)
        {
            vertexHelper.Clear();
            return;
        }

        _avatar.PopulateUiMesh(vertexHelper);
    }
}
