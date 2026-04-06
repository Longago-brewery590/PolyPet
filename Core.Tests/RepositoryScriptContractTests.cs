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
            Assert.Contains("*.sh text eol=lf", source);
        }
    }
}
