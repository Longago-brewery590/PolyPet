using System;
using Godot;
using PolyPet;

public enum StartSeedType
{
    None,
    Fixed,
    Random
}

[GlobalClass]
public partial class PolyPetAvatar : Node2D
{
    [Export] private int _startSeed;
    [Export] private int _startNameSeed;
    [Export] private StartSeedType _startSeedType = StartSeedType.Fixed;
    [Export] private StartSeedType _startNameSeedType = StartSeedType.Fixed;

    private int? _seed;
    private int? _nameSeed;
    private PetState _state = PetState.Idle;
    private float _time;
    private float _petTime;

    [Signal]
    public delegate void SeedChangedEventHandler(PolyPetAvatar avatar, Variant seed);

    [Signal]
    public delegate void NameSeedChangedEventHandler(PolyPetAvatar avatar, Variant nameSeed);

    public int? Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            RefreshData();
            EmitSeedChanged();
        }
    }

    public int? NameSeed
    {
        get => _nameSeed;
        set
        {
            _nameSeed = value;
            RefreshData();
            EmitNameSeedChanged();
        }
    }

    public PolyPetData Data { get; private set; }

    public void RandomizeSeed()
    {
        Seed = new System.Random().Next();
    }

    public void SetSeed(long value)
    {
        Seed = checked((int)value);
    }

    public void ClearSeed()
    {
        Seed = null;
    }

    public void RandomizeNameSeed()
    {
        NameSeed = new System.Random().Next();
    }

    public void SetNameSeed(long value)
    {
        NameSeed = checked((int)value);
    }

    public void ClearNameSeed()
    {
        NameSeed = null;
    }

    public override void _Ready()
    {
        if (_startSeedType == StartSeedType.Fixed) _seed = _startSeed;
        else if (_startSeedType == StartSeedType.Random) _seed = new System.Random().Next();

        if (_startNameSeedType == StartSeedType.Fixed) _nameSeed = _startNameSeed;
        else if (_startNameSeedType == StartSeedType.Random) _nameSeed = new System.Random().Next();

        RefreshData();
        EmitSeedChanged();
        EmitNameSeedChanged();
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;

        if (_state == PetState.BeingPet && _time - _petTime > 0.5f)
            _state = PetState.Idle;

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (Data.Body.Vertices == null) return;

        var frame = GetCurrentFrame();
        var offset = new Vector2(frame.PositionOffset.X, frame.PositionOffset.Y);

        DrawTransform(frame, offset, () =>
        {
            DrawShapePart(Data.Body);
            DrawShapePart(Data.Head);
            foreach (var ear in Data.Ears) DrawShapePart(ear);
            foreach (var eye in Data.Eyes) DrawShapePart(eye);
            DrawMouth(Data.Mouth);
            foreach (var limb in Data.Limbs) DrawShapePart(limb);
            if (Data.Tail.Vertices != null && Data.Tail.Vertices.Length > 0)
                DrawShapePart(Data.Tail);
        });
    }

    public override void _Input(InputEvent @event)
    {
        if (Data.Body.Vertices == null) return;

        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            var localPos = GetAnimatedLocalPosition(ToLocal(mb.GlobalPosition));
            float hitRadius = Data.Body.Scale * 1.5f;
            if (localPos.LengthSquared() < hitRadius * hitRadius)
            {
                _state = PetState.BeingPet;
                _petTime = _time;
            }
        }
    }

    private void DrawTransform(AnimationFrame frame, Vector2 offset, Action drawAction)
    {
        var xform = Transform2D.Identity;
        xform = xform.Translated(offset);
        xform = xform.Scaled(new Vector2(frame.ScaleX, frame.ScaleY));
        DrawSetTransform(xform.Origin, 0f, new Vector2(frame.ScaleX, frame.ScaleY));
        drawAction();
        DrawSetTransform(Vector2.Zero);
    }

    private AnimationFrame GetCurrentFrame()
    {
        return PolyPetAnimation.GetFrame(_state, _time, _time - _petTime);
    }

    private Vector2 GetAnimatedLocalPosition(Vector2 localPosition)
    {
        var frame = GetCurrentFrame();
        var offset = new Vector2(frame.PositionOffset.X, frame.PositionOffset.Y);
        var scale = new Vector2(frame.ScaleX, frame.ScaleY);

        if (Mathf.IsZeroApprox(scale.X) || Mathf.IsZeroApprox(scale.Y))
            return localPosition - offset;

        return new Vector2(
            (localPosition.X - offset.X) / scale.X,
            (localPosition.Y - offset.Y) / scale.Y);
    }

    private void DrawShapePart(ShapePart part)
    {
        var pos = new Vector2(part.Position.X, -part.Position.Y); // Flip Y for screen coords
        var color = new Color(part.Color.R / 255f, part.Color.G / 255f, part.Color.B / 255f);

        if (part.Shape == ShapeType.Circle && part.Vertices.Length > 8)
        {
            DrawCircle(pos, part.Radius, color);
            return;
        }

        if (part.Vertices.Length < 3) return;

        var verts = new Vector2[part.Vertices.Length];
        var colors = new Color[part.Vertices.Length];
        for (int i = 0; i < part.Vertices.Length; i++)
        {
            verts[i] = pos + new Vector2(part.Vertices[i].X, -part.Vertices[i].Y);
            colors[i] = color;
        }
        DrawPolygon(verts, colors);
    }

    private void DrawMouth(ShapePart mouth)
    {
        if (mouth.Vertices == null || mouth.Vertices.Length < 2) return;

        var pos = new Vector2(mouth.Position.X, -mouth.Position.Y);
        var color = new Color(mouth.Color.R / 255f, mouth.Color.G / 255f, mouth.Color.B / 255f);

        var points = new Vector2[mouth.Vertices.Length];
        for (int i = 0; i < mouth.Vertices.Length; i++)
            points[i] = pos + new Vector2(mouth.Vertices[i].X, -mouth.Vertices[i].Y);

        DrawPolyline(points, color, 2f);
    }

    private void RefreshData()
    {
        Data = _seed.HasValue || _nameSeed.HasValue
            ? PolyPetGenerator.Create(_seed ?? 0, _nameSeed)
            : CreateEmptyData();
    }

    private void EmitSeedChanged()
    {
        EmitSignal(
            SignalName.SeedChanged,
            Variant.From(this),
            _seed.HasValue ? Variant.From(_seed.Value) : default);
    }

    private void EmitNameSeedChanged()
    {
        EmitSignal(
            SignalName.NameSeedChanged,
            Variant.From(this),
            _nameSeed.HasValue ? Variant.From(_nameSeed.Value) : default);
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
}
