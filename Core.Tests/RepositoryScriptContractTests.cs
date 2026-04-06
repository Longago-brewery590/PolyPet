using System;
using System.IO;
using Xunit;

namespace PolyPet.Tests
{
    public class RepositoryScriptContractTests
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

        [Theory]
        [InlineData("scripts/test-sync-scripts.sh")]
        [InlineData("scripts/sync-core-to-adapters.sh")]
        [InlineData("scripts/sync-godot-sample.sh")]
        [InlineData("scripts/sync-unity-sample.sh")]
        public void ShellScripts_UseLfLineEndings(string relativePath)
        {
            var source = File.ReadAllText(RepoFile(relativePath));

            Assert.DoesNotContain("\r\n", source);
        }

        [Fact]
        public void GitAttributes_EnforcesLfForShellScripts()
        {
            var attributesPath = RepoFile(".gitattributes");

            Assert.True(File.Exists(attributesPath));

            var source = File.ReadAllText(attributesPath);
            Assert.Contains("* text=auto", source);
            Assert.Contains("*.sh text eol=lf", source);
        }

        [Fact]
        public void EditorConfig_EnforcesWindowsFriendlyLineEndingsRepositoryWide()
        {
            var editorConfigPath = RepoFile(".editorconfig");

            Assert.True(File.Exists(editorConfigPath));

            var source = File.ReadAllText(editorConfigPath);
            Assert.Contains("root = true", source);
            Assert.Contains("end_of_line = crlf", source);
        }

        [Fact]
        public void GodotSampleEditorConfig_EnforcesWindowsFriendlyLineEndings()
        {
            var editorConfigPath = RepoFile("Samples/PolyPetDemoGodot/.editorconfig");

            Assert.True(File.Exists(editorConfigPath));

            var source = File.ReadAllText(editorConfigPath);
            Assert.Contains("root = true", source);
            Assert.Contains("end_of_line = crlf", source);
        }

        [Fact]
        public void GodotSampleGitAttributes_DoesNotOverrideWindowsCheckoutLineEndings()
        {
            var attributesPath = RepoFile("Samples/PolyPetDemoGodot/.gitattributes");

            Assert.True(File.Exists(attributesPath));

            var source = File.ReadAllText(attributesPath);
            Assert.Contains("* text=auto", source);
            Assert.DoesNotContain("eol=lf", source);
        }
    }
}
