using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace PolyPet.Tests
{
    public class UnityAdapterContractTests
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
        public void PolyPetAvatar_UsesSerializedTypedUnityEventsInsteadOfPlainEvents()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Unity", "Runtime", "PolyPetAvatar.cs")));

            Assert.Contains("using UnityEngine.Events;", source);
            Assert.Contains("public struct NullableInt", source);
            Assert.Matches(@"AvatarNullableIntEvent\s*:\s*UnityEvent<PolyPetAvatar,\s*NullableInt>", source);
            Assert.Contains("[SerializeField] private AvatarNullableIntEvent _seedChanged", source);
            Assert.Contains("[SerializeField] private AvatarNullableIntEvent _nameSeedChanged", source);
            Assert.DoesNotContain("public event Action SeedChanged;", source);
            Assert.DoesNotContain("public event Action NameSeedChanged;", source);
        }

        [Fact]
        public void PolyPetAvatar_ExposesFrameSizingAndRectTransformSupport()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Unity", "Runtime", "PolyPetAvatar.cs")));

            Assert.True(Regex.IsMatch(source, @"public\s+Vector2\s+FrameSize\b"),
                "Expected a public FrameSize property.");
            Assert.True(Regex.IsMatch(source, @"\bRectTransform\b"),
                "Expected RectTransform support in the Unity avatar source.");
        }

        [Fact]
        public void PolyPetAvatar_RejectsUiPointerPositionsOutsideTheRectTransformBounds()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Unity", "Runtime", "PolyPetAvatar.cs")));

            Assert.Contains("ScreenPointToLocalPointInRectangle", source);
            Assert.Contains("rectTransform.rect.Contains(localPosition)", source);
        }

        [Fact]
        public void PolyPetAvatar_UsesCameraRayPlaneIntersectionForWorldSpacePointerMapping()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Unity", "Runtime", "PolyPetAvatar.cs")));

            Assert.Contains("ScreenPointToRay", source);
            Assert.Contains("new Plane(", source);
            Assert.DoesNotContain("ScreenToWorldPoint", source);
            Assert.DoesNotContain("WorldToScreenPoint", source);
        }

        [Fact]
        public void UnityPackage_DeclaresUgGuiDependenciesForPolyPetAvatar()
        {
            var asmdef = File.ReadAllText(RepoFile(Path.Combine("Unity", "Runtime", "Shilo.PolyPet.asmdef")));
            var packageManifest = File.ReadAllText(RepoFile(Path.Combine("Unity", "package.json")));

            Assert.Contains("\"UnityEngine.UI\"", asmdef);
            Assert.Contains("\"dependencies\"", packageManifest);
            Assert.Contains("\"com.unity.ugui\"", packageManifest);
        }

        [Fact]
        public void PolyPetAvatar_UsesRectTransformFrameOnlyWhenInUiRenderMode()
        {
            var source = File.ReadAllText(RepoFile(Path.Combine("Unity", "Runtime", "PolyPetAvatar.cs")));

            Assert.Matches(
                @"private\s+Rect\s+GetResolvedFrameRect\(\)\s*\{\s*if\s*\(TryGetUiRenderContext\(out\s+var\s+rectTransform,\s*out\s+_\)\)",
                source);
            Assert.Contains("var frameSize = FrameSize;", source);
        }
    }
}
