using System.Threading.Tasks;
using Xamarin.Essentials;
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

		[Theory]
		[InlineData(null, "")]
		[InlineData("", "")]
		[InlineData(".", ".")]
		[InlineData(".txt", ".txt")]
		[InlineData("*.txt", ".txt")]
		[InlineData("*.*", ".*")]
		[InlineData("txt", ".txt")]
		[InlineData("test.txt", ".test.txt")]
		[InlineData("test.", ".test.")]
		[InlineData("....txt", ".txt")]
		[InlineData("******txt", ".txt")]
		[InlineData("******.txt", ".txt")]
		[InlineData("******.......txt", ".txt")]
		public void Extensions_Clean_Correctly_Cleans_Extensions(string input, string output)
		{
			var cleaned = FileSystem.Extensions.Clean(input);

			Assert.Equal(output, cleaned);
		}
	}
}
