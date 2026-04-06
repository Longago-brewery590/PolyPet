using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("PolyPet/Sample UI/Rounded Panel Graphic")]
public sealed class RoundedPanelGraphic : MaskableGraphic
{
    [SerializeField, Min(0f)] private float _cornerRadius = 24f;
    [SerializeField, Min(0f)] private float _borderThickness = 8f;
    [SerializeField] private Color _borderColor = new Color(0.82f, 0.57f, 0.33f, 1f);
    [SerializeField, Min(1)] private int _segmentsPerCorner = 6;

    public float CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Mathf.Max(0f, value);
            SetVerticesDirty();
        }
    }

    public float BorderThickness
    {
        get => _borderThickness;
        set
        {
            _borderThickness = Mathf.Max(0f, value);
            SetVerticesDirty();
        }
    }

    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            SetVerticesDirty();
        }
    }

    public int SegmentsPerCorner
    {
        get => _segmentsPerCorner;
        set
        {
            _segmentsPerCorner = Mathf.Max(1, value);
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        var rect = GetPixelAdjustedRect();
        if (rect.width <= 0f || rect.height <= 0f)
            return;

        var fillColor = (Color32)color;
        var borderColor = (Color32)_borderColor;
        var cornerRadius = Mathf.Clamp(_cornerRadius, 0f, Mathf.Min(rect.width, rect.height) * 0.5f);
        var outer = BuildRoundedRectPoints(rect, cornerRadius, _segmentsPerCorner);
        if (outer.Count < 3)
            return;

        var borderThickness = Mathf.Clamp(_borderThickness, 0f, Mathf.Min(rect.width, rect.height) * 0.5f);
        if (borderThickness <= 0f || borderColor.a == 0)
        {
            AddFilledPolygon(vertexHelper, outer, fillColor);
            return;
        }

        var innerRect = Rect.MinMaxRect(
            rect.xMin + borderThickness,
            rect.yMin + borderThickness,
            rect.xMax - borderThickness,
            rect.yMax - borderThickness);

        if (innerRect.width <= 0f || innerRect.height <= 0f)
        {
            AddFilledPolygon(vertexHelper, outer, fillColor);
            return;
        }

        var innerCornerRadius = Mathf.Clamp(cornerRadius - borderThickness, 0f, Mathf.Min(innerRect.width, innerRect.height) * 0.5f);
        var inner = BuildRoundedRectPoints(innerRect, innerCornerRadius, _segmentsPerCorner);
        if (inner.Count < 3 || inner.Count != outer.Count)
            inner = BuildInsetFallbackPoints(outer, innerRect);

        AddFilledPolygon(vertexHelper, outer, borderColor);
        AddFilledPolygon(vertexHelper, inner, fillColor);
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        _cornerRadius = Mathf.Max(0f, _cornerRadius);
        _borderThickness = Mathf.Max(0f, _borderThickness);
        _segmentsPerCorner = Mathf.Max(1, _segmentsPerCorner);
        SetVerticesDirty();
    }

    private static void AddFilledPolygon(VertexHelper vertexHelper, IReadOnlyList<Vector2> points, Color32 color)
    {
        if (points.Count < 3)
            return;

        var center = Vector2.zero;
        for (var i = 0; i < points.Count; i++)
            center += points[i];
        center /= points.Count;

        var centerIndex = vertexHelper.currentVertCount;
        AddVertex(vertexHelper, center, color);

        for (var i = 0; i < points.Count; i++)
            AddVertex(vertexHelper, points[i], color);

        for (var i = 0; i < points.Count - 1; i++)
            vertexHelper.AddTriangle(centerIndex, centerIndex + i + 1, centerIndex + i + 2);

        vertexHelper.AddTriangle(centerIndex, centerIndex + points.Count, centerIndex + 1);
    }

    private static void AddVertex(VertexHelper vertexHelper, Vector2 position, Color32 color)
    {
        var vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.color = color;
        vertexHelper.AddVert(vertex);
    }

    private static List<Vector2> BuildRoundedRectPoints(Rect rect, float radius, int segmentsPerCorner)
    {
        radius = Mathf.Clamp(radius, 0f, Mathf.Min(rect.width, rect.height) * 0.5f);
        segmentsPerCorner = Mathf.Max(1, segmentsPerCorner);

        if (radius <= 0f)
        {
            return new List<Vector2>
            {
                new Vector2(rect.xMin, rect.yMax),
                new Vector2(rect.xMax, rect.yMax),
                new Vector2(rect.xMax, rect.yMin),
                new Vector2(rect.xMin, rect.yMin)
            };
        }

        var points = new List<Vector2>(segmentsPerCorner * 4 + 4);

        AddCorner(points, new Vector2(rect.xMin + radius, rect.yMax - radius), radius, 180f, 90f, segmentsPerCorner);
        AddCorner(points, new Vector2(rect.xMax - radius, rect.yMax - radius), radius, 90f, 0f, segmentsPerCorner);
        AddCorner(points, new Vector2(rect.xMax - radius, rect.yMin + radius), radius, 0f, -90f, segmentsPerCorner);
        AddCorner(points, new Vector2(rect.xMin + radius, rect.yMin + radius), radius, -90f, -180f, segmentsPerCorner);

        return RemoveDuplicatePoints(points);
    }

    private static void AddCorner(
        List<Vector2> points,
        Vector2 center,
        float radius,
        float startAngleDegrees,
        float endAngleDegrees,
        int segmentsPerCorner)
    {
        for (var i = 0; i <= segmentsPerCorner; i++)
        {
            var t = i / (float)segmentsPerCorner;
            var angle = Mathf.Lerp(startAngleDegrees, endAngleDegrees, t) * Mathf.Deg2Rad;
            points.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
    }

    private static List<Vector2> RemoveDuplicatePoints(List<Vector2> points)
    {
        if (points.Count == 0)
            return points;

        var cleaned = new List<Vector2>(points.Count);
        cleaned.Add(points[0]);

        for (var i = 1; i < points.Count; i++)
        {
            if ((points[i] - cleaned[cleaned.Count - 1]).sqrMagnitude > 0.000001f)
                cleaned.Add(points[i]);
        }

        if (cleaned.Count > 1 && (cleaned[0] - cleaned[cleaned.Count - 1]).sqrMagnitude <= 0.000001f)
            cleaned.RemoveAt(cleaned.Count - 1);

        return cleaned;
    }

    private static List<Vector2> BuildInsetFallbackPoints(IReadOnlyList<Vector2> outer, Rect innerRect)
    {
        var outerBounds = GetBounds(outer);
        var center = outerBounds.center;
        var scaleX = outerBounds.width > Mathf.Epsilon ? innerRect.width / outerBounds.width : 1f;
        var scaleY = outerBounds.height > Mathf.Epsilon ? innerRect.height / outerBounds.height : 1f;
        var scale = Mathf.Clamp01(Mathf.Min(scaleX, scaleY));
        var points = new List<Vector2>(outer.Count);

        for (var i = 0; i < outer.Count; i++)
        {
            var offset = outer[i] - center;
            points.Add(center + offset * scale);
        }

        return points;
    }

    private static Rect GetBounds(IReadOnlyList<Vector2> points)
    {
        var minX = points[0].x;
        var maxX = points[0].x;
        var minY = points[0].y;
        var maxY = points[0].y;

        for (var i = 1; i < points.Count; i++)
        {
            var point = points[i];
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxY = Mathf.Max(maxY, point.y);
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
}
