using System;

namespace PolyPet
{
    public static class PolyPetAnimation
    {
        private const float IdleAmplitude = 4f;
        private const float IdleCycleDuration = 2f;
        private const float PetDuration = 0.5f;

        public static AnimationEnvelope GetEnvelope()
        {
            return Union(GetEnvelope(PetState.Idle), GetEnvelope(PetState.BeingPet));
        }

        public static AnimationEnvelope GetEnvelope(PetState state)
        {
            if (state == PetState.BeingPet)
            {
                return new AnimationEnvelope(
                    0f,
                    0f,
                    0f,
                    0f,
                    0.9f,
                    1.15f,
                    0.8f,
                    1.15f);
            }

            return new AnimationEnvelope(
                0f,
                0f,
                -IdleAmplitude,
                IdleAmplitude,
                1f,
                1f,
                1f,
                1f);
        }

        public static AnimationFrame GetIdleFrame(float time)
        {
            var y = (float)Math.Sin(time / IdleCycleDuration * Math.PI * 2) * IdleAmplitude;
            return new AnimationFrame
            {
                PositionOffset = new Vec2(0f, y),
                ScaleX = 1f,
                ScaleY = 1f
            };
        }

        public static AnimationFrame GetPetFrame(float timeSincePet)
        {
            if (timeSincePet >= PetDuration)
                return new AnimationFrame
                {
                    PositionOffset = new Vec2(0f, 0f),
                    ScaleX = 1f,
                    ScaleY = 1f
                };

            float scaleX, scaleY;

            if (timeSincePet < 0.1f)
            {
                var t = Smoothstep(timeSincePet / 0.1f);
                scaleX = Lerp(1f, 1.15f, t);
                scaleY = Lerp(1f, 0.8f, t);
            }
            else if (timeSincePet < 0.3f)
            {
                var t = Smoothstep((timeSincePet - 0.1f) / 0.2f);
                scaleX = Lerp(1.15f, 0.9f, t);
                scaleY = Lerp(0.8f, 1.15f, t);
            }
            else
            {
                var t = Smoothstep((timeSincePet - 0.3f) / 0.2f);
                scaleX = Lerp(0.9f, 1f, t);
                scaleY = Lerp(1.15f, 1f, t);
            }

            return new AnimationFrame
            {
                PositionOffset = new Vec2(0f, 0f),
                ScaleX = scaleX,
                ScaleY = scaleY
            };
        }

        public static AnimationFrame GetFrame(PetState state, float time, float timeSincePet)
        {
            var idle = GetIdleFrame(time);

            if (state == PetState.Idle)
                return idle;

            var pet = GetPetFrame(timeSincePet);
            return new AnimationFrame
            {
                PositionOffset = new Vec2(idle.PositionOffset.X, idle.PositionOffset.Y + pet.PositionOffset.Y),
                ScaleX = idle.ScaleX * pet.ScaleX,
                ScaleY = idle.ScaleY * pet.ScaleY
            };
        }

        private static float Smoothstep(float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return t * t * (3f - 2f * t);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private static AnimationEnvelope Union(AnimationEnvelope left, AnimationEnvelope right)
        {
            return new AnimationEnvelope(
                Math.Min(left.MinOffsetX, right.MinOffsetX),
                Math.Max(left.MaxOffsetX, right.MaxOffsetX),
                Math.Min(left.MinOffsetY, right.MinOffsetY),
                Math.Max(left.MaxOffsetY, right.MaxOffsetY),
                Math.Min(left.MinScaleX, right.MinScaleX),
                Math.Max(left.MaxScaleX, right.MaxScaleX),
                Math.Min(left.MinScaleY, right.MinScaleY),
                Math.Max(left.MaxScaleY, right.MaxScaleY));
        }
    }
}
