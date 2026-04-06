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

        [Fact]
        public void GodotPolyPetAvatar_UsesControlSizeAndSharedFrameLayout()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Godot", "addons", "PolyPet", "PolyPetAvatar.cs")));

            Assert.Contains("PolyPetLayout.CreateFrameLayout(Data, Size.X, Size.Y)", source);
        }

        [Fact]
        public void GodotPolyPetAvatar_UsesControlLocalMouseAndTouchInputForHitTesting()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Godot", "addons", "PolyPet", "PolyPetAvatar.cs")));

            Assert.Contains("public override void _GuiInput(InputEvent @event)", source);
            Assert.Contains("InputEventScreenTouch", source);
            Assert.DoesNotContain("public override void _Input(InputEvent @event)", source);
            Assert.DoesNotContain("ToLocal(mb.GlobalPosition)", source);
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

        [Fact]
        public void UnityPolyPetAvatar_ExposesFrameSizingAndRectTransformSupport()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Unity", "Runtime", "PolyPetAvatar.cs")));

            Assert.True(Regex.IsMatch(source, @"using\s+UnityEngine\s*;"),
                "Expected UnityEngine support in the Unity avatar source.");
            Assert.True(Regex.IsMatch(source, @"public\s+Vector2\s+FrameSize\b"),
                "Expected a public FrameSize property.");
            Assert.True(Regex.IsMatch(source, @"\bRectTransform\b"),
                "Expected RectTransform support in the Unity avatar source.");
        }
    }
}
