namespace PolyPet
{
    public struct Vec2
    {
        public float X;
        public float Y;

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public struct Color32
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color32(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    public enum ShapeType
    {
        Triangle,
        Square,
        Pentagon,
        Hexagon,
        Octagon,
        Circle
    }

    public enum PatternType
    {
        Solid,
        Polkadots,
        Stripes,
        Spots
    }

    public enum EarStyle
    {
        Pointed,
        Round,
        Floppy,
        Antennae
    }

    public enum EyeStyle
    {
        Circle,
        Oval,
        Star,
        Dot,
        HalfMoon
    }

    public enum TailStyle
    {
        None,
        Stub,
        Long,
        Fan
    }

    public struct ShapePart
    {
        public Vec2[] Vertices;
        public Vec2 Position;
        public Color32 Color;
        public float Scale;
        public float Rotation;
        public float Radius;
        public ShapeType Shape;
    }

    public struct PatternData
    {
        public PatternType Type;
        public Color32 Color;
        public float Density;
        public float Angle;
        public float Size;
    }

    public struct PolyPetData
    {
        public string Name;
        public ShapePart Body;
        public PatternData BodyPattern;
        public ShapePart Head;
        public PatternData HeadPattern;
        public ShapePart[] Eyes;
        public ShapePart Mouth;
        public ShapePart[] Ears;
        public ShapePart[] Limbs;
        public ShapePart Tail;
        public Color32 PrimaryColor;
        public Color32 SecondaryColor;
        public Color32 TertiaryColor;
        public int Seed;
    }

    public enum PetState
    {
        Idle,
        BeingPet
    }

    public struct AnimationFrame
    {
        public Vec2 PositionOffset;
        public float ScaleX;
        public float ScaleY;
    }

    public struct PetBounds
    {
        public float MinX;
        public float MinY;
        public float MaxX;
        public float MaxY;

        public PetBounds(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public float Width => MaxX - MinX;
        public float Height => MaxY - MinY;
        public float CenterX => (MinX + MaxX) * 0.5f;
        public float CenterY => (MinY + MaxY) * 0.5f;
    }

    public struct PetFrameLayout
    {
        public float Scale;
        public float OffsetX;
        public float OffsetY;

        public PetFrameLayout(float scale, float offsetX, float offsetY)
        {
            Scale = scale;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }

    public struct AnimationEnvelope
    {
        public float MinOffsetX;
        public float MaxOffsetX;
        public float MinOffsetY;
        public float MaxOffsetY;
        public float MinScaleX;
        public float MaxScaleX;
        public float MinScaleY;
        public float MaxScaleY;

        public AnimationEnvelope(
            float minOffsetX,
            float maxOffsetX,
            float minOffsetY,
            float maxOffsetY,
            float minScaleX,
            float maxScaleX,
            float minScaleY,
            float maxScaleY)
        {
            MinOffsetX = minOffsetX;
            MaxOffsetX = maxOffsetX;
            MinOffsetY = minOffsetY;
            MaxOffsetY = maxOffsetY;
            MinScaleX = minScaleX;
            MaxScaleX = maxScaleX;
            MinScaleY = minScaleY;
            MaxScaleY = maxScaleY;
        }
    }

    public static class PolyPetGenerationLimits
    {
        public const float BodyScaleMin = 80f;
        public const float BodyScaleMax = 120f;

        public const float HeadScaleMinRatio = 0.5f;
        public const float HeadScaleMaxRatio = 0.7f;
        public const float HeadVerticalOffsetRatio = 0.6f;
        public const float EarPositionRatio = 0.7f;
        public const float EarScaleRatio = 0.4f;

        public const float LimbScaleRatio = 0.2f;
        public const float LimbTwoSpacingRatio = 0.4f;
        public const float LimbFourSpacingRatio = 0.5f;
        public const float LimbUpperYRatio = 0.8f;
        public const float LimbLowerYRatio = 0.1f;

        public const float TailScaleRatio = 0.25f;
        public const float TailOffsetRatio = 0.8f;
        public const float TailLongTipRatio = 1.5f;

        public static float MaxHeadScale => BodyScaleMax * HeadScaleMaxRatio;
        public static float MaxTailScale => BodyScaleMax * TailScaleRatio;

        public static PetBounds GetCanonicalStaticBounds()
        {
            return new PetBounds(
                -BodyScaleMax,
                -BodyScaleMax,
                BodyScaleMax * TailOffsetRatio + MaxTailScale * TailLongTipRatio,
                BodyScaleMax + MaxHeadScale * (HeadVerticalOffsetRatio + EarPositionRatio + EarScaleRatio));
        }
    }
}
