using System;
using System.IO;
using Xunit;

namespace PolyPet.Tests
{
    public class GodotAdapterSignalContractTests
    {
        private static string RepoFile(string relativePath)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "PolyPet.sln")))
            {
                directory = directory.Parent;
            }

            if (directory == null)
                throw new DirectoryNotFoundException("Could not locate repository root.");

            return Path.Combine(directory.FullName, relativePath);
        }

        [Fact]
        public void PolyPetAvatar_DeclaresGodotSignalsInsteadOfPlainEvents()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Godot", "addons", "PolyPet", "PolyPetAvatar.cs")));

            Assert.Contains("[Signal]", source);
            Assert.Contains("public delegate void SeedChangedEventHandler();", source);
            Assert.Contains("public delegate void NameSeedChangedEventHandler();", source);
            Assert.DoesNotContain("public event Action SeedChanged;", source);
            Assert.DoesNotContain("public event Action NameSeedChanged;", source);
        }
    }
}
