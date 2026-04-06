using System;
using Xunit;

namespace PolyPet.Tests
{
    public class PolyPetGeneratorTests
    {
        [Fact]
        public void Create_ReturnsSameDataForSameSeed()
        {
            var pet1 = PolyPetGenerator.Create(42);
            var pet2 = PolyPetGenerator.Create(42);
            Assert.Equal(pet1.Seed, pet2.Seed);
            Assert.Equal(pet1.Body.Shape, pet2.Body.Shape);
            Assert.Equal(pet1.PrimaryColor.R, pet2.PrimaryColor.R);
            Assert.Equal(pet1.PrimaryColor.G, pet2.PrimaryColor.G);
            Assert.Equal(pet1.PrimaryColor.B, pet2.PrimaryColor.B);
        }

        [Fact]
        public void Create_StoresSeedInData()
        {
            var pet = PolyPetGenerator.Create(123);
            Assert.Equal(123, pet.Seed);
        }

        [Fact]
        public void Create_BodyHasVertices()
        {
            var pet = PolyPetGenerator.Create(1);
            Assert.NotNull(pet.Body.Vertices);
            Assert.True(pet.Body.Vertices.Length >= 3 || pet.Body.Shape == ShapeType.Circle);
        }

        [Fact]
        public void Create_HeadIsSmallerThanBody()
        {
            var pet = PolyPetGenerator.Create(1);
            Assert.True(pet.Head.Scale < pet.Body.Scale);
        }

        [Fact]
        public void Create_HasTwoEyes()
        {
            var pet = PolyPetGenerator.Create(1);
            Assert.Equal(2, pet.Eyes.Length);
        }

        [Fact]
        public void Create_NonCircularEyesAreNotTaggedAsCircles()
        {
            var pet = PolyPetGenerator.Create(6);

            Assert.All(pet.Eyes, eye =>
            {
                Assert.Equal(10, eye.Vertices.Length);
                Assert.NotEqual(ShapeType.Circle, eye.Shape);
            });
        }

        [Fact]
        public void Create_HasTwoEars()
        {
            var pet = PolyPetGenerator.Create(1);
            Assert.Equal(2, pet.Ears.Length);
        }

        [Fact]
        public void Create_HasTwoOrFourLimbs()
        {
            for (var seed = 0; seed < 50; seed++)
            {
                var pet = PolyPetGenerator.Create(seed);
                Assert.True(pet.Limbs.Length == 2 || pet.Limbs.Length == 4,
                    $"Seed {seed}: expected 2 or 4 limbs, got {pet.Limbs.Length}");
            }
        }

        [Fact]
        public void Create_NameIsNullWithoutNameSeed()
        {
            var pet = PolyPetGenerator.Create(1);
            Assert.Null(pet.Name);
        }

        [Fact]
        public void Create_NameIsSetWithNameSeed()
        {
            var pet = PolyPetGenerator.Create(1, 42);
            Assert.NotNull(pet.Name);
            Assert.True(pet.Name!.Length > 0);
        }

        [Fact]
        public void Create_NameMatchesNameGenerator()
        {
            var pet = PolyPetGenerator.Create(1, 42);
            var expectedName = PolyPetNameGenerator.Create(42);
            Assert.Equal(expectedName, pet.Name);
        }

        [Fact]
        public void Create_DifferentSeedsProduceDifferentPets()
        {
            var pet1 = PolyPetGenerator.Create(1);
            var pet2 = PolyPetGenerator.Create(2);
            // At least one visual property should differ
            var differs = pet1.Body.Shape != pet2.Body.Shape
                          || pet1.PrimaryColor.R != pet2.PrimaryColor.R
                          || pet1.PrimaryColor.G != pet2.PrimaryColor.G
                          || pet1.PrimaryColor.B != pet2.PrimaryColor.B
                          || pet1.Body.Vertices.Length != pet2.Body.Vertices.Length;
            Assert.True(differs);
        }

        [Fact]
        public void Create_PaletteHasThreeDistinctColors()
        {
            var pet = PolyPetGenerator.Create(42);
            var allSame = pet.PrimaryColor.R == pet.SecondaryColor.R
                          && pet.PrimaryColor.G == pet.SecondaryColor.G
                          && pet.PrimaryColor.B == pet.SecondaryColor.B
                          && pet.PrimaryColor.R == pet.TertiaryColor.R
                          && pet.PrimaryColor.G == pet.TertiaryColor.G
                          && pet.PrimaryColor.B == pet.TertiaryColor.B;
            Assert.False(allSame);
        }

        [Fact]
        public void Create_PatternTypeIsValid()
        {
            for (var seed = 0; seed < 50; seed++)
            {
                var pet = PolyPetGenerator.Create(seed);
                Assert.True(Enum.IsDefined(typeof(PatternType), pet.BodyPattern.Type));
                Assert.True(Enum.IsDefined(typeof(PatternType), pet.HeadPattern.Type));
            }
        }

        [Fact]
        public void Create_HeadIsAboveBody()
        {
            var pet = PolyPetGenerator.Create(1);
            // Body is at origin (0,0), head Y should be positive (above)
            Assert.True(pet.Head.Position.Y > pet.Body.Position.Y,
                "Head should be above body");
        }

        [Fact]
        public void Create_BodyScaleStaysWithinSharedGenerationLimits()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var pet = PolyPetGenerator.Create(seed);

                Assert.InRange(pet.Body.Scale, PolyPetGenerationLimits.BodyScaleMin, PolyPetGenerationLimits.BodyScaleMax);
            }
        }

        [Fact]
        public void Create_TailPolygonsStaySimpleAcrossSeedRange()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var pet = PolyPetGenerator.Create(seed);
                if (pet.Tail.Vertices == null || pet.Tail.Vertices.Length < 3)
                    continue;

                Assert.True(IsSimplePolygon(pet.Tail.Vertices),
                    $"Seed {seed}: tail polygon should not self-intersect");
            }
        }

        private static bool IsSimplePolygon(Vec2[] vertices)
        {
            if (vertices.Length < 3)
                return false;

            if (Math.Abs(GetPolygonArea(vertices)) <= 0.001f)
                return false;

            for (var i = 0; i < vertices.Length; i++)
            {
                var a1 = vertices[i];
                var a2 = vertices[(i + 1) % vertices.Length];

                for (var j = i + 1; j < vertices.Length; j++)
                {
                    if (Math.Abs(i - j) <= 1)
                        continue;

                    if (i == 0 && j == vertices.Length - 1)
                        continue;

                    var b1 = vertices[j];
                    var b2 = vertices[(j + 1) % vertices.Length];
                    if (SegmentsIntersect(a1, a2, b1, b2))
                        return false;
                }
            }

            return true;
        }

        private static float GetPolygonArea(Vec2[] vertices)
        {
            float sum = 0f;
            for (var i = 0; i < vertices.Length; i++)
            {
                var next = (i + 1) % vertices.Length;
                sum += vertices[i].X * vertices[next].Y - vertices[next].X * vertices[i].Y;
            }

            return Math.Abs(sum) * 0.5f;
        }

        private static bool SegmentsIntersect(Vec2 p1, Vec2 p2, Vec2 q1, Vec2 q2)
        {
            var d1 = Cross(p1, p2, q1);
            var d2 = Cross(p1, p2, q2);
            var d3 = Cross(q1, q2, p1);
            var d4 = Cross(q1, q2, p2);
            const float epsilon = 0.0001f;

            if (((d1 > epsilon && d2 < -epsilon) || (d1 < -epsilon && d2 > epsilon)) &&
                ((d3 > epsilon && d4 < -epsilon) || (d3 < -epsilon && d4 > epsilon)))
            {
                return true;
            }

            return (Math.Abs(d1) <= epsilon && IsPointOnSegment(p1, q1, p2)) ||
                   (Math.Abs(d2) <= epsilon && IsPointOnSegment(p1, q2, p2)) ||
                   (Math.Abs(d3) <= epsilon && IsPointOnSegment(q1, p1, q2)) ||
                   (Math.Abs(d4) <= epsilon && IsPointOnSegment(q1, p2, q2));
        }

        private static float Cross(Vec2 a, Vec2 b, Vec2 c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        private static bool IsPointOnSegment(Vec2 a, Vec2 point, Vec2 b)
        {
            const float epsilon = 0.0001f;
            return point.X >= Math.Min(a.X, b.X) - epsilon &&
                   point.X <= Math.Max(a.X, b.X) + epsilon &&
                   point.Y >= Math.Min(a.Y, b.Y) - epsilon &&
                   point.Y <= Math.Max(a.Y, b.Y) + epsilon;
        }
    }
}
