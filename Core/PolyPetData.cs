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
}