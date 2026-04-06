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
    private const float WorldScale = 0.01f;
    private const float MouthLineWidth = 2f * WorldScale;

    [SerializeField] private int _startSeed;
    [SerializeField] private int _startNameSeed;
    [SerializeField] private StartSeedType _startSeedType = StartSeedType.Fixed;
    [SerializeField] private StartSeedType _startNameSeedType = StartSeedType.Fixed;
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
    private Vector3 _initialLocalPosition;
    private Vector3 _initialLocalScale;

    public AvatarNullableIntEvent SeedChanged => _seedChanged;
    public AvatarNullableIntEvent NameSeedChanged => _nameSeedChanged;

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
        _initialLocalPosition = transform.localPosition;
        _initialLocalScale = transform.localScale;

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

        var frame = PolyPetAnimation.GetFrame(_state, _time, _time - _petTime);
        transform.localPosition = _initialLocalPosition + new Vector3(
            frame.PositionOffset.X * WorldScale,
            frame.PositionOffset.Y * WorldScale,
            0f);
        transform.localScale = Vector3.Scale(
            _initialLocalScale,
            new Vector3(frame.ScaleX, frame.ScaleY, 1f));

        if (Data.Body.Vertices == null || !Input.GetMouseButtonDown(0))
            return;

        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        var worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float hitRadius = Data.Body.Scale * 0.02f; // Scale for world units
        if (Vector2.Distance(new Vector2(worldPos.x, worldPos.y),
            new Vector2(transform.position.x, transform.position.y)) < hitRadius)
        {
            _state = PetState.BeingPet;
            _petTime = _time;
        }
    }

    void OnRenderObject()
    {
        if (_mesh == null || _material == null) return;
        _material.SetPass(0);
        Graphics.DrawMeshNow(_mesh, transform.localToWorldMatrix);
        if (_mouthMesh != null)
            Graphics.DrawMeshNow(_mouthMesh, transform.localToWorldMatrix);
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

        AddShapeToMesh(Data.Body, vertices, colors, triangles, WorldScale);
        AddShapeToMesh(Data.Head, vertices, colors, triangles, WorldScale);
        foreach (var ear in Data.Ears) AddShapeToMesh(ear, vertices, colors, triangles, WorldScale);
        foreach (var eye in Data.Eyes) AddShapeToMesh(eye, vertices, colors, triangles, WorldScale);
        AddMouthToMesh(Data.Mouth, mouthVertices, mouthColors, mouthTriangles, WorldScale);
        foreach (var limb in Data.Limbs) AddShapeToMesh(limb, vertices, colors, triangles, WorldScale);
        if (Data.Tail.Vertices != null && Data.Tail.Vertices.Length >= 3)
            AddShapeToMesh(Data.Tail, vertices, colors, triangles, WorldScale);

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
        System.Collections.Generic.List<int> triangles,
        float scale)
    {
        if (part.Vertices == null || part.Vertices.Length < 3) return;

        int startIndex = vertices.Count;
        var color = new UnityEngine.Color(
            part.Color.R / 255f, part.Color.G / 255f, part.Color.B / 255f);

        for (int i = 0; i < part.Vertices.Length; i++)
        {
            vertices.Add(new Vector3(
                (part.Position.X + part.Vertices[i].X) * scale,
                (part.Position.Y + part.Vertices[i].Y) * scale,
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
        System.Collections.Generic.List<int> triangles,
        float scale)
    {
        if (mouth.Vertices == null || mouth.Vertices.Length < 2) return;

        var color = new UnityEngine.Color(
            mouth.Color.R / 255f, mouth.Color.G / 255f, mouth.Color.B / 255f);

        for (int i = 0; i < mouth.Vertices.Length - 1; i++)
        {
            var start = new Vector2(
                (mouth.Position.X + mouth.Vertices[i].X) * scale,
                (mouth.Position.Y + mouth.Vertices[i].Y) * scale);
            var end = new Vector2(
                (mouth.Position.X + mouth.Vertices[i + 1].X) * scale,
                (mouth.Position.Y + mouth.Vertices[i + 1].Y) * scale);

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

    void OnDestroy()
    {
        if (_mesh != null) Destroy(_mesh);
        if (_mouthMesh != null) Destroy(_mouthMesh);
        if (_material != null) Destroy(_material);
    }
}
