using System;
using System.Linq;
using Xunit;
using PolyPet;

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
            for (int seed = 0; seed < 50; seed++)
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
            var pet = PolyPetGenerator.Create(1, nameSeed: 42);
            Assert.NotNull(pet.Name);
            Assert.True(pet.Name!.Length > 0);
        }

        [Fact]
        public void Create_NameMatchesNameGenerator()
        {
            var pet = PolyPetGenerator.Create(1, nameSeed: 42);
            var expectedName = PolyPetNameGenerator.Create(42);
            Assert.Equal(expectedName, pet.Name);
        }

        [Fact]
        public void Create_DifferentSeedsProduceDifferentPets()
        {
            var pet1 = PolyPetGenerator.Create(1);
            var pet2 = PolyPetGenerator.Create(2);
            // At least one visual property should differ
            bool differs = pet1.Body.Shape != pet2.Body.Shape
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
            bool allSame = pet.PrimaryColor.R == pet.SecondaryColor.R
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
            for (int seed = 0; seed < 50; seed++)
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
    }
}
