using System;
using UnityEngine;
using UnityEngine.Events;
using PolyPet;
using Random = System.Random;

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
    private Mesh _mouthMesh;
    private Material _material;

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
        EmitSeedChanged();
        EmitNameSeedChanged();
    }

    void Update()
    {
        _time += Time.deltaTime;

        if (_state == PetState.BeingPet && _time - _petTime > 0.5f)
            _state = PetState.Idle;

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

    void OnRenderObject()
    {
        if (_mesh == null || _material == null) return;

        var frame = GetCurrentFrame();
        var renderMatrix = GetRenderMatrix(frame);

        _material.SetPass(0);
        Graphics.DrawMeshNow(_mesh, renderMatrix);
        if (_mouthMesh != null)
            Graphics.DrawMeshNow(_mouthMesh, renderMatrix);
    }

    private void BuildMesh()
    {
        if (_mesh != null) Destroy(_mesh);
        if (_mouthMesh != null) Destroy(_mouthMesh);
        _mesh = new Mesh();
        _mouthMesh = new Mesh();

        var vertices = new System.Collections.Generic.List<Vector3>();
        var colors = new System.Collections.Generic.List<UnityEngine.Color>();
        var triangles = new System.Collections.Generic.List<int>();
        var mouthVertices = new System.Collections.Generic.List<Vector3>();
        var mouthColors = new System.Collections.Generic.List<UnityEngine.Color>();
        var mouthTriangles = new System.Collections.Generic.List<int>();

        AddShapeToMesh(Data.Body, vertices, colors, triangles);
        AddShapeToMesh(Data.Head, vertices, colors, triangles);
        foreach (var ear in Data.Ears) AddShapeToMesh(ear, vertices, colors, triangles);
        foreach (var eye in Data.Eyes) AddShapeToMesh(eye, vertices, colors, triangles);
        AddMouthToMesh(Data.Mouth, mouthVertices, mouthColors, mouthTriangles);
        foreach (var limb in Data.Limbs) AddShapeToMesh(limb, vertices, colors, triangles);
        if (Data.Tail.Vertices != null && Data.Tail.Vertices.Length >= 3)
            AddShapeToMesh(Data.Tail, vertices, colors, triangles);

        _mesh.SetVertices(vertices);
        _mesh.SetColors(colors);
        _mesh.SetTriangles(triangles, 0);

        _mouthMesh.SetVertices(mouthVertices);
        _mouthMesh.SetColors(mouthColors);
        _mouthMesh.SetTriangles(mouthTriangles, 0);
    }

    private void AddShapeToMesh(ShapePart part,
        System.Collections.Generic.List<Vector3> vertices,
        System.Collections.Generic.List<UnityEngine.Color> colors,
        System.Collections.Generic.List<int> triangles)
    {
        if (part.Vertices == null || part.Vertices.Length < 3) return;

        int startIndex = vertices.Count;
        var color = new UnityEngine.Color(
            part.Color.R / 255f, part.Color.G / 255f, part.Color.B / 255f);

        for (int i = 0; i < part.Vertices.Length; i++)
        {
            vertices.Add(new Vector3(
                part.Position.X + part.Vertices[i].X,
                part.Position.Y + part.Vertices[i].Y,
                0));
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
        System.Collections.Generic.List<int> triangles)
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

            vertices.Add(new Vector3(start.x - normal.x, start.y - normal.y, 0f));
            vertices.Add(new Vector3(start.x + normal.x, start.y + normal.y, 0f));
            vertices.Add(new Vector3(end.x + normal.x, end.y + normal.y, 0f));
            vertices.Add(new Vector3(end.x - normal.x, end.y - normal.y, 0f));

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

        BuildMesh();
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

    private Matrix4x4 GetRenderMatrix(AnimationFrame frame)
    {
        return transform.localToWorldMatrix * GetPetLocalMatrix(frame);
    }

    private Matrix4x4 GetPetLocalMatrix(AnimationFrame frame)
    {
        var frameRect = GetResolvedFrameRect();
        var frameLayout = PolyPetLayout.CreateFrameLayout(Data, frameRect.width, frameRect.height);

        return Matrix4x4.TRS(
            GetLayoutOrigin(frameRect, frameLayout, frame),
            Quaternion.identity,
            GetLayoutScale(frameLayout, frame));
    }

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
        if (transform is RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            return new Rect(
                rect.xMin,
                rect.yMin,
                Mathf.Max(rect.width, Mathf.Epsilon),
                Mathf.Max(rect.height, Mathf.Epsilon));
        }

        var frameSize = FrameSize;
        return new Rect(
            -frameSize.x * 0.5f,
            -frameSize.y * 0.5f,
            Mathf.Max(frameSize.x, Mathf.Epsilon),
            Mathf.Max(frameSize.y, Mathf.Epsilon));
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

    private bool TryGetScreenPointLocalPosition(Vector2 screenPoint, out Vector2 localPosition)
    {
        if (transform is RectTransform rectTransform)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                screenPoint,
                Camera.main,
                out localPosition);
        }

        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            localPosition = default;
            return false;
        }

        var screenDepth = mainCamera.WorldToScreenPoint(transform.position).z;
        var worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, screenDepth));
        var local3 = transform.InverseTransformPoint(worldPosition);
        localPosition = new Vector2(local3.x, local3.y);
        return true;
    }

    void OnDestroy()
    {
        if (_mesh != null) Destroy(_mesh);
        if (_mouthMesh != null) Destroy(_mouthMesh);
        if (_material != null) Destroy(_material);
    }
}
