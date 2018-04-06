using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class FileSystem_Tests
    {
        [Fact]
        public void FileSystem_Fail_On_NetStandard()
        {
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => FileSystem.AppDataDirectory);
        }

        [Fact]
        public async Task OpenAppPackageFileAsync_Fail_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => FileSystem.OpenAppPackageFileAsync("filename.txt"));
        }
    }
}
