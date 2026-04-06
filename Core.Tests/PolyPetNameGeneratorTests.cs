using Xunit;

namespace PolyPet.Tests
{
    public class PolyPetNameGeneratorTests
    {
        [Fact]
        public void Create_ReturnsSameNameForSameSeed()
        {
            var name1 = PolyPetNameGenerator.Create(42);
            var name2 = PolyPetNameGenerator.Create(42);
            Assert.Equal(name1, name2);
        }

        [Fact]
        public void Create_ReturnsDifferentNamesForDifferentSeeds()
        {
            var name1 = PolyPetNameGenerator.Create(1);
            var name2 = PolyPetNameGenerator.Create(999);
            Assert.NotEqual(name1, name2);
        }

        [Fact]
        public void Create_ReturnsCapitalizedName()
        {
            var name = PolyPetNameGenerator.Create(42);
            Assert.True(char.IsUpper(name[0]));
        }

        [Fact]
        public void Create_ReturnsNameWith4To9Characters()
        {
            // 2 syllables = 4-6 chars, 3 syllables = 6-9 chars
            for (var seed = 0; seed < 100; seed++)
            {
                var name = PolyPetNameGenerator.Create(seed);
                Assert.InRange(name.Length, 4, 9);
            }
        }

        [Fact]
        public void Create_ReturnsOnlyLetters()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var name = PolyPetNameGenerator.Create(seed);
                Assert.All(name.ToCharArray(), c => Assert.True(char.IsLetter(c)));
            }
        }
    }
}