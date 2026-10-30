using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Xunit;

namespace Tests
{
	public class FilePicker_Tests
	{
		[Fact]
		public async Task PickAsync_Fail_On_NetStandard()
		{
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => FilePicker.PickAsync());
		}

		[Fact]
		public async Task PickMultipleAsync_Fail_On_NetStandard()
		{
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => FilePicker.PickMultipleAsync());
		}

		[Fact]
		public void BuiltInFileTypes_Fail_On_NetStandard_Without_A_Backend()
		{
			// No backend (for example the GTK one) has installed FilePickerFileTypeBackend.Factory: stock behaviour.
			Assert.Null(FilePickerFileTypeBackend.Factory);
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => FilePickerFileTypeBackend.Create("Images"));
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => FilePickerFileTypeBackend.Create("Pdf"));
		}
	}
}
