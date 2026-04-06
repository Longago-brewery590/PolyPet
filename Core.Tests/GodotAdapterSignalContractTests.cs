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

        [Fact]
        public void UnityPolyPetAvatar_UsesSerializedUnityEventsInsteadOfPlainEvents()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Unity", "Runtime", "PolyPetAvatar.cs")));

            Assert.Contains("using UnityEngine.Events;", source);
            Assert.Contains("[SerializeField] private UnityEvent _seedChanged = new UnityEvent();", source);
            Assert.Contains("[SerializeField] private UnityEvent _nameSeedChanged = new UnityEvent();", source);
            Assert.Contains("public UnityEvent SeedChanged => _seedChanged;", source);
            Assert.Contains("public UnityEvent NameSeedChanged => _nameSeedChanged;", source);
            Assert.DoesNotContain("public event Action SeedChanged;", source);
            Assert.DoesNotContain("public event Action NameSeedChanged;", source);
        }
    }
}
