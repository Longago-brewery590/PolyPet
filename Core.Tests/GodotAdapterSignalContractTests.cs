using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace PolyPet.Tests
{
    public class GodotAdapterSignalContractTests
    {
        private static string RepoFile(string relativePath)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "PolyPet.sln")))
                directory = directory.Parent;

            if (directory == null)
                throw new DirectoryNotFoundException("Could not locate repository root.");

            return Path.Combine(directory.FullName, relativePath);
        }

        [Fact]
        public void PolyPetAvatar_DeclaresGodotSignalsInsteadOfPlainEvents()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Godot", "addons", "PolyPet", "PolyPetAvatar.cs")));

            Assert.Contains("[Signal]", source);
            Assert.Contains("public delegate void SeedChangedEventHandler(PolyPetAvatar avatar, Variant seed);",
                source);
            Assert.Contains("public delegate void NameSeedChangedEventHandler(PolyPetAvatar avatar, Variant nameSeed);",
                source);
            Assert.DoesNotContain("public event Action SeedChanged;", source);
            Assert.DoesNotContain("public event Action NameSeedChanged;", source);
        }

        [Fact]
        public void PolyPetAvatar_ExposesGDScriptFriendlyNameSeedBridgeMethods()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Godot", "addons", "PolyPet", "PolyPetAvatar.cs")));

            Assert.Contains("public void SetSeed(long value)", source);
            Assert.Contains("public void ClearSeed()", source);
            Assert.Contains("public void SetNameSeed(long value)", source);
            Assert.Contains("public void ClearNameSeed()", source);
        }

        [Fact]
        public void GodotPolyPetAvatar_InheritsFromControlForFrameBasedLayout()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Godot", "addons", "PolyPet", "PolyPetAvatar.cs")));

            Assert.True(Regex.IsMatch(source, @"class\s+PolyPetAvatar\s*:\s*Control"),
                "Expected PolyPetAvatar to inherit from Control.");
        }

    }
}
