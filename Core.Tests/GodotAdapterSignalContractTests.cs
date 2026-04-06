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
        public void UnityPolyPetAvatar_UsesSerializedTypedUnityEventsInsteadOfPlainEvents()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Unity", "Runtime", "PolyPetAvatar.cs")));

            Assert.Contains("using UnityEngine.Events;", source);
            Assert.Contains("public struct NullableInt", source);
            Assert.Contains("public sealed class AvatarNullableIntEvent : UnityEvent<PolyPetAvatar, NullableInt>",
                source);
            Assert.Contains("public delegate void SeedChangedCallback(PolyPetAvatar avatar, NullableInt seed);",
                source);
            Assert.Contains("public delegate void NameSeedChangedCallback(PolyPetAvatar avatar, NullableInt nameSeed);",
                source);
            Assert.Contains(
                "[SerializeField] private AvatarNullableIntEvent _seedChanged = new AvatarNullableIntEvent();", source);
            Assert.Contains(
                "[SerializeField] private AvatarNullableIntEvent _nameSeedChanged = new AvatarNullableIntEvent();",
                source);
            Assert.Contains("public AvatarNullableIntEvent SeedChanged => _seedChanged;", source);
            Assert.Contains("public AvatarNullableIntEvent NameSeedChanged => _nameSeedChanged;", source);
            Assert.DoesNotContain("public event Action SeedChanged;", source);
            Assert.DoesNotContain("public event Action NameSeedChanged;", source);
        }
    }
}