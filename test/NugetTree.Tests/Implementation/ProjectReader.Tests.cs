using System;
using System.Threading.Tasks;
using Xunit;

namespace NugetTree.Implementation
{
    public static class ProjectReaderTests
    {
        [Fact]
        public static async Task ReadProjectFile_ThrowsArgumentNullException_WhenFilenameMissing()
        {
            var sut = new ProjectReader();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ReadProjectFile(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ReadProjectFile(String.Empty));
        }

        [Fact]
        public static async Task ReadProjectFile_ThrowArgumentException_WhenFileDoesNotExist()
        {
            var sut = new ProjectReader();

            await Assert.ThrowsAsync<ArgumentException>(() => sut.ReadProjectFile("fakefile"));
        }
    }
}
