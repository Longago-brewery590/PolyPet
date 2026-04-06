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
    }
}