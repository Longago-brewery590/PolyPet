using System;
using Xunit;

namespace PolyPet.Tests
{
    public class PolyPetLayoutTests
    {
        [Fact]
        public void CalculateStaticBounds_ContainsEveryRenderedPart()
        {
            var pet = PolyPetGenerator.Create(42);

            var bounds = PolyPetLayout.CalculateStaticBounds(pet);

            Assert.True(bounds.Width > 0f);
            Assert.True(bounds.Height > 0f);
            AssertVerticesInsideBounds(pet.Body, bounds);
            AssertVerticesInsideBounds(pet.Head, bounds);
            foreach (var ear in pet.Ears) AssertVerticesInsideBounds(ear, bounds);
            foreach (var eye in pet.Eyes) AssertVerticesInsideBounds(eye, bounds);
            foreach (var limb in pet.Limbs) AssertVerticesInsideBounds(limb, bounds);
            AssertVerticesInsideBounds(pet.Mouth, bounds);
            AssertVerticesInsideBounds(pet.Tail, bounds);
        }

        [Fact]
        public void CalculateAnimatedBounds_ContainsStaticBoundsForSeedRange()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var pet = PolyPetGenerator.Create(seed);
                var staticBounds = PolyPetLayout.CalculateStaticBounds(pet);
                var animatedBounds = PolyPetLayout.CalculateAnimatedBounds(pet);

                Assert.True(animatedBounds.MinX <= staticBounds.MinX);
                Assert.True(animatedBounds.MaxX >= staticBounds.MaxX);
                Assert.True(animatedBounds.MinY <= staticBounds.MinY);
                Assert.True(animatedBounds.MaxY >= staticBounds.MaxY);
            }
        }

        [Fact]
        public void CalculateCanonicalAnimatedBounds_ContainsAnimatedBoundsForSeedRange()
        {
            var canonicalBounds = PolyPetLayout.CalculateCanonicalAnimatedBounds();

            for (var seed = 0; seed < 100; seed++)
            {
                var pet = PolyPetGenerator.Create(seed);
                var animatedBounds = PolyPetLayout.CalculateAnimatedBounds(pet);

                Assert.True(canonicalBounds.MinX <= animatedBounds.MinX);
                Assert.True(canonicalBounds.MaxX >= animatedBounds.MaxX);
                Assert.True(canonicalBounds.MinY <= animatedBounds.MinY);
                Assert.True(canonicalBounds.MaxY >= animatedBounds.MaxY);
            }
        }

        [Fact]
        public void CalculateCanonicalAnimatedBounds_MatchesSharedGenerationLimits()
        {
            var canonicalBounds = PolyPetLayout.CalculateCanonicalAnimatedBounds();
            var expectedStaticBounds = new PetBounds(
                -PolyPetGenerationLimits.BodyScaleMax,
                -PolyPetGenerationLimits.BodyScaleMax,
                PolyPetGenerationLimits.BodyScaleMax * PolyPetGenerationLimits.TailOffsetRatio +
                    PolyPetGenerationLimits.BodyScaleMax * PolyPetGenerationLimits.TailScaleRatio * 1.5f,
                PolyPetGenerationLimits.BodyScaleMax +
                    (PolyPetGenerationLimits.BodyScaleMax * PolyPetGenerationLimits.HeadScaleMaxRatio) *
                    (PolyPetGenerationLimits.HeadVerticalOffsetRatio +
                     PolyPetGenerationLimits.EarPositionRatio +
                     PolyPetGenerationLimits.EarScaleRatio));
            var expectedAnimatedBounds = ApplyEnvelope(expectedStaticBounds, PolyPetAnimation.GetEnvelope());

            Assert.Equal(expectedAnimatedBounds.MinX, canonicalBounds.MinX, 3);
            Assert.Equal(expectedAnimatedBounds.MaxX, canonicalBounds.MaxX, 3);
            Assert.Equal(expectedAnimatedBounds.MinY, canonicalBounds.MinY, 3);
            Assert.Equal(expectedAnimatedBounds.MaxY, canonicalBounds.MaxY, 3);
        }

        [Fact]
        public void CreateFrameLayout_FitsAnimatedBoundsInsideRequestedFrame()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var pet = PolyPetGenerator.Create(seed);
                var layout = PolyPetLayout.CreateFrameLayout(pet, 320f, 180f);
                var animatedBounds = PolyPetLayout.CalculateAnimatedBounds(pet);

                var left = animatedBounds.MinX * layout.Scale + layout.OffsetX;
                var right = animatedBounds.MaxX * layout.Scale + layout.OffsetX;
                var top = animatedBounds.MinY * layout.Scale + layout.OffsetY;
                var bottom = animatedBounds.MaxY * layout.Scale + layout.OffsetY;

                AssertWithinRange(left, 0f, 320f);
                AssertWithinRange(right, 0f, 320f);
                AssertWithinRange(top, 0f, 180f);
                AssertWithinRange(bottom, 0f, 180f);
            }
        }

        [Fact]
        public void CreateFrameLayout_PreservesAnimatedWidthOrdering()
        {
            const float tolerance = 0.0001f;
            var samples = new (float AnimatedWidth, float OccupiedWidth)[100];

            for (var seed = 0; seed < samples.Length; seed++)
            {
                var firstPet = PolyPetGenerator.Create(seed);
                var firstBounds = PolyPetLayout.CalculateAnimatedBounds(firstPet);
                var firstLayout = PolyPetLayout.CreateFrameLayout(firstPet, 320f, 180f);

                samples[seed] = (firstBounds.Width, firstBounds.Width * firstLayout.Scale);
            }

            for (var left = 0; left < samples.Length; left++)
            {
                for (var right = left + 1; right < samples.Length; right++)
                {
                    var animatedWidthDelta = Math.Abs(samples[left].AnimatedWidth - samples[right].AnimatedWidth);
                    if (animatedWidthDelta <= tolerance)
                        continue;

                    var animatedOrderedLarger = samples[left].AnimatedWidth > samples[right].AnimatedWidth;
                    var occupiedOrderedLarger = samples[left].OccupiedWidth > samples[right].OccupiedWidth;

                    Assert.True(animatedOrderedLarger == occupiedOrderedLarger,
                        $"Seeds {left} and {right} should preserve animated width ordering.");
                }
            }
        }

        [Fact]
        public void CreateFrameLayout_ZeroSizedFrame_CollapsesAnimatedBoundsInsideFrame()
        {
            var pet = PolyPetGenerator.Create(42);
            var layout = PolyPetLayout.CreateFrameLayout(pet, 0f, 0f);
            var animatedBounds = PolyPetLayout.CalculateAnimatedBounds(pet);

            var left = animatedBounds.MinX * layout.Scale + layout.OffsetX;
            var right = animatedBounds.MaxX * layout.Scale + layout.OffsetX;
            var top = animatedBounds.MinY * layout.Scale + layout.OffsetY;
            var bottom = animatedBounds.MaxY * layout.Scale + layout.OffsetY;

            Assert.Equal(0f, layout.Scale, 3);
            AssertWithinRange(left, 0f, 0f);
            AssertWithinRange(right, 0f, 0f);
            AssertWithinRange(top, 0f, 0f);
            AssertWithinRange(bottom, 0f, 0f);
        }

        private static void AssertVerticesInsideBounds(ShapePart part, PetBounds bounds)
        {
            if (part.Vertices == null || part.Vertices.Length == 0)
                return;

            foreach (var vertex in part.Vertices)
            {
                var x = part.Position.X + vertex.X;
                var y = part.Position.Y + vertex.Y;
                AssertWithinRange(x, bounds.MinX, bounds.MaxX);
                AssertWithinRange(y, bounds.MinY, bounds.MaxY);
            }
        }

        private static void AssertWithinRange(float value, float min, float max)
        {
            const float tolerance = 0.0001f;

            Assert.True(value >= min - tolerance && value <= max + tolerance,
                $"Expected {value} to be within [{min}, {max}] with tolerance {tolerance}.");
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
    }
}
