using Xunit;

namespace PolyPet.Tests
{
    public class PolyPetAnimationTests
    {
        [Fact]
        public void GetIdleFrame_AtZero_ReturnsZeroOffset()
        {
            var frame = PolyPetAnimation.GetIdleFrame(0f);
            Assert.Equal(0f, frame.PositionOffset.Y, 3);
        }

        [Fact]
        public void GetIdleFrame_AtQuarterCycle_ReturnsPositiveY()
        {
            // Peak of sine at 0.5s (quarter of 2s cycle)
            var frame = PolyPetAnimation.GetIdleFrame(0.5f);
            Assert.True(frame.PositionOffset.Y > 0);
        }

        [Fact]
        public void GetIdleFrame_ScaleIsAlwaysOne()
        {
            var frame = PolyPetAnimation.GetIdleFrame(1.23f);
            Assert.Equal(1f, frame.ScaleX, 3);
            Assert.Equal(1f, frame.ScaleY, 3);
        }

        [Fact]
        public void GetPetFrame_AtZero_ScaleYCompressed()
        {
            var frame = PolyPetAnimation.GetPetFrame(0.05f);
            Assert.True(frame.ScaleY < 1f);
            Assert.True(frame.ScaleX > 1f);
        }

        [Fact]
        public void GetPetFrame_AfterDuration_ReturnsNeutralScale()
        {
            var frame = PolyPetAnimation.GetPetFrame(1.0f);
            Assert.Equal(1f, frame.ScaleX, 2);
            Assert.Equal(1f, frame.ScaleY, 2);
        }

        [Fact]
        public void GetFrame_Idle_MatchesGetIdleFrame()
        {
            var t = 0.7f;
            var idle = PolyPetAnimation.GetIdleFrame(t);
            var combined = PolyPetAnimation.GetFrame(PetState.Idle, t, 0f);
            Assert.Equal(idle.PositionOffset.Y, combined.PositionOffset.Y, 3);
        }

        [Fact]
        public void GetFrame_BeingPet_BlendsPetReaction()
        {
            var frame = PolyPetAnimation.GetFrame(PetState.BeingPet, 0f, 0.05f);
            Assert.True(frame.ScaleY < 1f);
        }

        [Fact]
        public void GetEnvelope_BeingPet_IncludesIdleBobYRange()
        {
            var envelope = PolyPetAnimation.GetEnvelope(PetState.BeingPet);

            foreach (var time in new[] { 0f, 0.5f, 1f, 1.5f })
            {
                var frame = PolyPetAnimation.GetFrame(PetState.BeingPet, time, 1f);
                Assert.InRange(frame.PositionOffset.Y, envelope.MinOffsetY, envelope.MaxOffsetY);
            }

            Assert.Equal(-4f, envelope.MinOffsetY, 3);
            Assert.Equal(4f, envelope.MaxOffsetY, 3);
        }

        [Fact]
        public void GetEnvelope_ReturnsUnionOfAllStateEnvelopes()
        {
            var envelope = PolyPetAnimation.GetEnvelope();
            var expected = CombineAllStateEnvelopes();

            Assert.Equal(expected.MinOffsetX, envelope.MinOffsetX, 3);
            Assert.Equal(expected.MaxOffsetX, envelope.MaxOffsetX, 3);
            Assert.Equal(expected.MinOffsetY, envelope.MinOffsetY, 3);
            Assert.Equal(expected.MaxOffsetY, envelope.MaxOffsetY, 3);
            Assert.Equal(expected.MinScaleX, envelope.MinScaleX, 3);
            Assert.Equal(expected.MaxScaleX, envelope.MaxScaleX, 3);
            Assert.Equal(expected.MinScaleY, envelope.MinScaleY, 3);
            Assert.Equal(expected.MaxScaleY, envelope.MaxScaleY, 3);
        }

        private static AnimationEnvelope CombineAllStateEnvelopes()
        {
            var hasValue = false;
            var combined = new AnimationEnvelope();

            foreach (PetState state in System.Enum.GetValues(typeof(PetState)))
            {
                var current = PolyPetAnimation.GetEnvelope(state);
                if (!hasValue)
                {
                    combined = current;
                    hasValue = true;
                    continue;
                }

                combined = new AnimationEnvelope(
                    System.Math.Min(combined.MinOffsetX, current.MinOffsetX),
                    System.Math.Max(combined.MaxOffsetX, current.MaxOffsetX),
                    System.Math.Min(combined.MinOffsetY, current.MinOffsetY),
                    System.Math.Max(combined.MaxOffsetY, current.MaxOffsetY),
                    System.Math.Min(combined.MinScaleX, current.MinScaleX),
                    System.Math.Max(combined.MaxScaleX, current.MaxScaleX),
                    System.Math.Min(combined.MinScaleY, current.MinScaleY),
                    System.Math.Max(combined.MaxScaleY, current.MaxScaleY));
            }

            return combined;
        }
    }
}
