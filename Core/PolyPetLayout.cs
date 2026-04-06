using System;

namespace PolyPet
{
    public static class PolyPetLayout
    {
        private static readonly AnimationEnvelope CanonicalAnimationEnvelope = PolyPetAnimation.GetEnvelope();

        public static PetBounds CalculateStaticBounds(PolyPetData pet)
        {
            var bounds = CreateEmptyBounds();

            Include(ref bounds, pet.Body);
            Include(ref bounds, pet.Head);
            Include(ref bounds, pet.Mouth);
            Include(ref bounds, pet.Tail);

            if (pet.Ears != null)
            {
                foreach (var ear in pet.Ears)
                    Include(ref bounds, ear);
            }

            if (pet.Eyes != null)
            {
                foreach (var eye in pet.Eyes)
                    Include(ref bounds, eye);
            }

            if (pet.Limbs != null)
            {
                foreach (var limb in pet.Limbs)
                    Include(ref bounds, limb);
            }

            return bounds;
        }

        public static PetBounds CalculateAnimatedBounds(PolyPetData pet)
        {
            return ApplyEnvelope(CalculateStaticBounds(pet), CanonicalAnimationEnvelope);
        }

        public static PetBounds CalculateCanonicalAnimatedBounds()
        {
            return ApplyEnvelope(CalculateCanonicalStaticBounds(), CanonicalAnimationEnvelope);
        }

        public static PetBounds CalculateCanonicalAnimatedBounds(PolyPetData pet)
        {
            return CalculateCanonicalAnimatedBounds();
        }

        public static PetFrameLayout CreateFrameLayout(PolyPetData pet, float frameWidth, float frameHeight)
        {
            var canonicalAnimatedBounds = CalculateCanonicalAnimatedBounds();
            var animatedBounds = CalculateAnimatedBounds(pet);

            if (canonicalAnimatedBounds.Width <= 0f || canonicalAnimatedBounds.Height <= 0f)
                return new PetFrameLayout(1f, 0f, 0f);

            var scaleX = frameWidth / canonicalAnimatedBounds.Width;
            var scaleY = frameHeight / canonicalAnimatedBounds.Height;
            var scale = Math.Min(scaleX, scaleY);

            if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
                scale = 1f;

            var offsetX = frameWidth * 0.5f - animatedBounds.CenterX * scale;
            var offsetY = frameHeight * 0.5f - animatedBounds.CenterY * scale;

            return new PetFrameLayout(scale, offsetX, offsetY);
        }

        private static PetBounds CalculateCanonicalStaticBounds()
        {
            const float bodyMaxScale = 120f;
            const float headScaleRatio = 0.7f;
            const float tailScaleRatio = 0.25f;

            var headMaxScale = bodyMaxScale * headScaleRatio;
            var tailMaxScale = bodyMaxScale * tailScaleRatio;
            var tailMaxX = bodyMaxScale * 0.8f + tailMaxScale * 1.5f;
            var topFromEars = bodyMaxScale + headMaxScale * 1.7f;

            return new PetBounds(
                -bodyMaxScale,
                -bodyMaxScale,
                tailMaxX,
                topFromEars);
        }

        private static PetBounds ApplyEnvelope(PetBounds bounds, AnimationEnvelope envelope)
        {
            var xRange = TransformRange(
                bounds.MinX,
                bounds.MaxX,
                envelope.MinScaleX,
                envelope.MaxScaleX,
                envelope.MinOffsetX,
                envelope.MaxOffsetX);

            var yRange = TransformRange(
                bounds.MinY,
                bounds.MaxY,
                envelope.MinScaleY,
                envelope.MaxScaleY,
                envelope.MinOffsetY,
                envelope.MaxOffsetY);

            return new PetBounds(xRange.Min, yRange.Min, xRange.Max, yRange.Max);
        }

        private static (float Min, float Max) TransformRange(
            float minValue,
            float maxValue,
            float minScale,
            float maxScale,
            float minOffset,
            float maxOffset)
        {
            var a = minValue * minScale + minOffset;
            var b = minValue * minScale + maxOffset;
            var c = minValue * maxScale + minOffset;
            var d = minValue * maxScale + maxOffset;
            var e = maxValue * minScale + minOffset;
            var f = maxValue * minScale + maxOffset;
            var g = maxValue * maxScale + minOffset;
            var h = maxValue * maxScale + maxOffset;

            return (
                Math.Min(Math.Min(Math.Min(a, b), Math.Min(c, d)), Math.Min(Math.Min(e, f), Math.Min(g, h))),
                Math.Max(Math.Max(Math.Max(a, b), Math.Max(c, d)), Math.Max(Math.Max(e, f), Math.Max(g, h)))
            );
        }

        private static PetBounds CreateEmptyBounds()
        {
            return new PetBounds(
                float.PositiveInfinity,
                float.PositiveInfinity,
                float.NegativeInfinity,
                float.NegativeInfinity);
        }

        private static void Include(ref PetBounds bounds, ShapePart part)
        {
            if (part.Vertices == null || part.Vertices.Length == 0)
            {
                Include(ref bounds, part.Position.X, part.Position.Y);
                return;
            }

            foreach (var vertex in part.Vertices)
                Include(ref bounds, part.Position.X + vertex.X, part.Position.Y + vertex.Y);
        }

        private static void Include(ref PetBounds bounds, float x, float y)
        {
            if (x < bounds.MinX) bounds.MinX = x;
            if (x > bounds.MaxX) bounds.MaxX = x;
            if (y < bounds.MinY) bounds.MinY = y;
            if (y > bounds.MaxY) bounds.MaxY = y;
        }
    }
}
