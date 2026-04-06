using System;

namespace PolyPet
{
    public static class PolyPetGenerator
    {
        public static PolyPetData Create(int seed, int? nameSeed = null)
        {
            var rng = new Random(seed);
            var data = new PolyPetData { Seed = seed };

            // 1. Palette
            GeneratePalette(rng, ref data);

            // 2. Body
            data.Body = GenerateBody(rng, data.PrimaryColor);

            // 3. Head
            data.Head = GenerateHead(rng, data.Body, data.PrimaryColor);

            // 4. Eyes
            data.Eyes = GenerateEyes(rng, data.Head);

            // 5. Mouth
            data.Mouth = GenerateMouth(rng, data.Head);

            // 6. Ears
            data.Ears = GenerateEars(rng, data.Head, data.SecondaryColor);

            // 7. Limbs
            data.Limbs = GenerateLimbs(rng, data.Body, data.PrimaryColor);

            // 8. Tail
            data.Tail = GenerateTail(rng, data.Body, data.SecondaryColor);

            // 9. Patterns
            data.BodyPattern = GeneratePattern(rng, data.SecondaryColor);
            data.HeadPattern = GeneratePattern(rng, data.SecondaryColor);

            // 10. Name
            if (nameSeed.HasValue)
                data.Name = PolyPetNameGenerator.Create(nameSeed.Value);

            return data;
        }

        // --- Palette ---

        private static void GeneratePalette(Random rng, ref PolyPetData petData)
        {
            var hue = (float)rng.NextDouble();
            var sat = 0.4f + (float)rng.NextDouble() * 0.4f; // 0.4-0.8
            var val = 0.7f + (float)rng.NextDouble() * 0.3f; // 0.7-1.0
            petData.PrimaryColor = HSVToRGB(hue, sat, val);

            var hue2 = hue + 0.3f + (float)rng.NextDouble() * 0.4f; // complementary/analogous
            petData.SecondaryColor = HSVToRGB(hue2 % 1f, sat * 0.8f, val * 0.9f);

            var hue3 = hue + 0.15f + (float)rng.NextDouble() * 0.2f;
            petData.TertiaryColor = HSVToRGB(hue3 % 1f, sat * 1.2f > 1f ? 1f : sat * 1.2f, val);
        }

        // --- Body ---

        private static ShapePart GenerateBody(Random rng, Color32 color)
        {
            var shape = (ShapeType)rng.Next(Enum.GetValues(typeof(ShapeType)).Length);
            var scale = 80f + (float)rng.NextDouble() * 40f; // 80-120

            Vec2[] vertices;
            var radius = scale;

            if (shape == ShapeType.Circle)
                vertices = GenerateCircleVertices(scale);
            else
                vertices = GenerateRegularPolygon(SidesForShape(shape), scale);

            return new ShapePart
            {
                Vertices = vertices,
                Position = new Vec2(0, 0),
                Color = color,
                Scale = scale,
                Rotation = 0f,
                Radius = radius,
                Shape = shape
            };
        }

        // --- Head ---

        private static ShapePart GenerateHead(Random rng, ShapePart body, Color32 color)
        {
            // Bias toward rounder shapes for cuteness
            ShapeType[] cuteShapes =
            {
                ShapeType.Circle, ShapeType.Circle, ShapeType.Hexagon,
                ShapeType.Octagon, ShapeType.Pentagon, ShapeType.Square
            };
            var shape = cuteShapes[rng.Next(cuteShapes.Length)];
            var scale = body.Scale * (0.5f + (float)rng.NextDouble() * 0.2f); // 50-70% of body

            Vec2[] vertices;
            if (shape == ShapeType.Circle)
                vertices = GenerateCircleVertices(scale);
            else
                vertices = GenerateRegularPolygon(SidesForShape(shape), scale);

            var headY = body.Scale + scale * 0.6f; // Above body

            return new ShapePart
            {
                Vertices = vertices,
                Position = new Vec2(0, headY),
                Color = color,
                Scale = scale,
                Rotation = 0f,
                Radius = scale,
                Shape = shape
            };
        }

        // --- Eyes ---

        private static ShapePart[] GenerateEyes(Random rng, ShapePart head)
        {
            var style = (EyeStyle)rng.Next(Enum.GetValues(typeof(EyeStyle)).Length);
            var eyeSize = head.Scale * 0.15f;
            var spacing = head.Scale * 0.3f;
            var eyeY = head.Position.Y + head.Scale * 0.15f;

            var eyes = new ShapePart[2];
            for (var i = 0; i < 2; i++)
            {
                var xDir = i == 0 ? -1f : 1f;
                Vec2[] verts;
                ShapeType shape;

                switch (style)
                {
                    case EyeStyle.Star:
                        verts = GenerateStarVertices(eyeSize, 5);
                        shape = ShapeType.Triangle;
                        break;
                    case EyeStyle.Oval:
                        verts = GenerateCircleVertices(eyeSize, 16);
                        shape = ShapeType.Circle;
                        break;
                    case EyeStyle.Dot:
                        verts = GenerateCircleVertices(eyeSize * 0.6f, 8);
                        shape = ShapeType.Circle;
                        break;
                    case EyeStyle.HalfMoon:
                        verts = GenerateHalfCircleVertices(eyeSize);
                        shape = ShapeType.Triangle;
                        break;
                    default: // Circle
                        verts = GenerateCircleVertices(eyeSize);
                        shape = ShapeType.Circle;
                        break;
                }

                eyes[i] = new ShapePart
                {
                    Vertices = verts,
                    Position = new Vec2(xDir * spacing, eyeY),
                    Color = new Color32(255, 255, 255),
                    Scale = eyeSize,
                    Rotation = 0f,
                    Radius = eyeSize,
                    Shape = shape
                };
            }

            return eyes;
        }

        // --- Mouth ---

        private static ShapePart GenerateMouth(Random rng, ShapePart head)
        {
            var mouthWidth = head.Scale * 0.25f;
            var mouthY = head.Position.Y - head.Scale * 0.2f;
            var points = 3 + rng.Next(3); // 3-5 vertices for arc

            var vertices = new Vec2[points];
            for (var i = 0; i < points; i++)
            {
                var t = (float)i / (points - 1);
                var x = (t - 0.5f) * mouthWidth * 2f;
                var y = -(float)Math.Sin(t * Math.PI) * mouthWidth * 0.3f;
                vertices[i] = new Vec2(x, y);
            }

            return new ShapePart
            {
                Vertices = vertices,
                Position = new Vec2(0, mouthY),
                Color = new Color32(60, 40, 40),
                Scale = mouthWidth,
                Rotation = 0f,
                Radius = mouthWidth,
                Shape = ShapeType.Triangle // arbitrary, vertices define shape
            };
        }

        // --- Ears ---

        private static ShapePart[] GenerateEars(Random rng, ShapePart head, Color32 color)
        {
            var style = (EarStyle)rng.Next(Enum.GetValues(typeof(EarStyle)).Length);
            var earSize = head.Scale * 0.4f;
            var spacing = head.Scale * 0.6f;
            var earY = head.Position.Y + head.Scale * 0.7f;

            var ears = new ShapePart[2];
            for (var i = 0; i < 2; i++)
            {
                var xDir = i == 0 ? -1f : 1f;
                Vec2[] verts;

                switch (style)
                {
                    case EarStyle.Round:
                        verts = GenerateHalfCircleVertices(earSize);
                        break;
                    case EarStyle.Floppy:
                        verts = new[]
                        {
                            new Vec2(0, 0),
                            new Vec2(xDir * earSize * 0.5f, earSize * 0.3f),
                            new Vec2(xDir * earSize * 0.8f, -earSize * 0.5f)
                        };
                        break;
                    case EarStyle.Antennae:
                        verts = new[]
                        {
                            new Vec2(0, 0),
                            new Vec2(xDir * earSize * 0.1f, earSize * 0.8f),
                            new Vec2(xDir * earSize * 0.15f, earSize),
                            new Vec2(xDir * earSize * 0.05f, earSize)
                        };
                        break;
                    default: // Pointed
                        verts = new[]
                        {
                            new Vec2(-earSize * 0.3f, 0),
                            new Vec2(0, earSize),
                            new Vec2(earSize * 0.3f, 0)
                        };
                        break;
                }

                ears[i] = new ShapePart
                {
                    Vertices = verts,
                    Position = new Vec2(xDir * spacing, earY),
                    Color = color,
                    Scale = earSize,
                    Rotation = 0f,
                    Radius = earSize,
                    Shape = ShapeType.Triangle
                };
            }

            return ears;
        }

        // --- Limbs ---

        private static ShapePart[] GenerateLimbs(Random rng, ShapePart body, Color32 color)
        {
            var count = rng.Next(2) == 0 ? 2 : 4; // 50/50
            var limbSize = body.Scale * 0.2f;
            var limbs = new ShapePart[count];

            if (count == 2)
            {
                var spacing = body.Scale * 0.4f;
                var limbY = -body.Scale * 0.8f;
                for (var i = 0; i < 2; i++)
                {
                    var xDir = i == 0 ? -1f : 1f;
                    limbs[i] = MakeLimb(xDir * spacing, limbY, limbSize, color);
                }
            }
            else
            {
                var spacing = body.Scale * 0.5f;
                float[] yPositions = { -body.Scale * 0.8f, -body.Scale * 0.1f };
                for (var i = 0; i < 4; i++)
                {
                    var xDir = i % 2 == 0 ? -1f : 1f;
                    var y = yPositions[i / 2];
                    limbs[i] = MakeLimb(xDir * spacing, y, limbSize, color);
                }
            }

            return limbs;
        }

        private static ShapePart MakeLimb(float x, float y, float size, Color32 color)
        {
            return new ShapePart
            {
                Vertices = GenerateCircleVertices(size, 8),
                Position = new Vec2(x, y),
                Color = color,
                Scale = size,
                Rotation = 0f,
                Radius = size,
                Shape = ShapeType.Circle
            };
        }

        // --- Tail ---

        private static ShapePart GenerateTail(Random rng, ShapePart body, Color32 color)
        {
            var style = (TailStyle)rng.Next(Enum.GetValues(typeof(TailStyle)).Length);
            var tailSize = body.Scale * 0.25f;
            var tailX = body.Scale * 0.8f;

            Vec2[] vertices;
            switch (style)
            {
                case TailStyle.Stub:
                    vertices = GenerateRegularPolygon(3, tailSize);
                    break;
                case TailStyle.Long:
                    vertices = new[]
                    {
                        new Vec2(0, tailSize * 0.2f),
                        new Vec2(tailSize, tailSize * 0.3f),
                        new Vec2(tailSize * 1.5f, 0),
                        new Vec2(tailSize, -tailSize * 0.3f),
                        new Vec2(0, -tailSize * 0.2f)
                    };
                    break;
                case TailStyle.Fan:
                    var fanCount = 3 + rng.Next(3); // 3-5 fan segments
                    var outerVertices = new Vec2[fanCount];
                    var innerVertices = new Vec2[fanCount];
                    for (var i = 0; i < fanCount; i++)
                    {
                        var angle = -0.4f + 0.8f * i / (fanCount - 1);
                        var len = tailSize * (0.8f + (float)rng.NextDouble() * 0.4f);
                        outerVertices[i] = new Vec2(
                            (float)Math.Cos(angle) * len,
                            (float)Math.Sin(angle) * len);
                        innerVertices[i] = new Vec2(
                            (float)Math.Cos(angle) * len * 0.5f,
                            (float)Math.Sin(angle) * len * 0.5f);
                    }

                    vertices = new Vec2[fanCount * 2];
                    vertices[0] = innerVertices[0];
                    for (var i = 0; i < fanCount; i++)
                        vertices[i + 1] = outerVertices[i];

                    for (var i = fanCount - 1; i > 0; i--)
                        vertices[fanCount + (fanCount - i)] = innerVertices[i];

                    break;
                default: // None
                    vertices = Array.Empty<Vec2>();
                    break;
            }

            return new ShapePart
            {
                Vertices = vertices,
                Position = new Vec2(tailX, 0),
                Color = color,
                Scale = tailSize,
                Rotation = 0f,
                Radius = tailSize,
                Shape = ShapeType.Triangle
            };
        }

        // --- Patterns ---

        private static PatternData GeneratePattern(Random rng, Color32 baseColor)
        {
            // Weighted: Solid 40%, Polkadots 25%, Stripes 20%, Spots 15%
            var roll = rng.Next(100);
            PatternType type;
            if (roll < 40) type = PatternType.Solid;
            else if (roll < 65) type = PatternType.Polkadots;
            else if (roll < 85) type = PatternType.Stripes;
            else type = PatternType.Spots;

            // Vary color slightly from base
            var colorShift = (byte)rng.Next(20, 60);
            var patternColor = new Color32(
                (byte)Math.Min(255, baseColor.R + colorShift),
                (byte)Math.Min(255, baseColor.G + colorShift),
                (byte)Math.Min(255, baseColor.B + colorShift));

            return new PatternData
            {
                Type = type,
                Color = patternColor,
                Density = 3f + (float)rng.NextDouble() * 7f, // 3-10
                Angle = (float)rng.NextDouble() * 360f,
                Size = 3f + (float)rng.NextDouble() * 8f // 3-11
            };
        }

        // --- Geometry Helpers ---

        private static Vec2[] GenerateRegularPolygon(int sides, float radius)
        {
            var vertices = new Vec2[sides];
            for (var i = 0; i < sides; i++)
            {
                var angle = (float)(2 * Math.PI * i / sides) - (float)(Math.PI / 2);
                vertices[i] = new Vec2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius);
            }

            return vertices;
        }

        private static Vec2[] GenerateCircleVertices(float radius, int segments = 24)
        {
            return GenerateRegularPolygon(segments, radius);
        }

        private static Vec2[] GenerateStarVertices(float radius, int points)
        {
            var vertices = new Vec2[points * 2];
            var innerRadius = radius * 0.4f;
            for (var i = 0; i < points * 2; i++)
            {
                var angle = (float)(Math.PI * i / points) - (float)(Math.PI / 2);
                var r = i % 2 == 0 ? radius : innerRadius;
                vertices[i] = new Vec2(
                    (float)Math.Cos(angle) * r,
                    (float)Math.Sin(angle) * r);
            }

            return vertices;
        }

        private static Vec2[] GenerateHalfCircleVertices(float radius, int segments = 12)
        {
            var vertices = new Vec2[segments + 1];
            for (var i = 0; i <= segments; i++)
            {
                var angle = (float)(Math.PI * i / segments);
                vertices[i] = new Vec2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius);
            }

            return vertices;
        }

        private static int SidesForShape(ShapeType shape)
        {
            switch (shape)
            {
                case ShapeType.Triangle: return 3;
                case ShapeType.Square: return 4;
                case ShapeType.Pentagon: return 5;
                case ShapeType.Hexagon: return 6;
                case ShapeType.Octagon: return 8;
                default: return 24; // Circle approximation
            }
        }

        internal static bool IsPointInPolygon(Vec2 point, Vec2[] polygon)
        {
            var inside = false;
            var j = polygon.Length - 1;
            for (var i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y > point.Y != polygon[j].Y > point.Y &&
                    point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) /
                    (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                    inside = !inside;
                j = i;
            }

            return inside;
        }

        internal static Color32 HSVToRGB(float h, float s, float v)
        {
            var c = v * s;
            var x = c * (1f - Math.Abs(h * 6f % 2f - 1f));
            var m = v - c;

            float r, g, b;
            var sector = (int)(h * 6f) % 6;
            switch (sector)
            {
                case 0:
                    r = c;
                    g = x;
                    b = 0;
                    break;
                case 1:
                    r = x;
                    g = c;
                    b = 0;
                    break;
                case 2:
                    r = 0;
                    g = c;
                    b = x;
                    break;
                case 3:
                    r = 0;
                    g = x;
                    b = c;
                    break;
                case 4:
                    r = x;
                    g = 0;
                    b = c;
                    break;
                default:
                    r = c;
                    g = 0;
                    b = x;
                    break;
            }

            return new Color32(
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255));
        }
    }
}
