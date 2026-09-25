using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Tests
{
	public class Launcher_Tests
	{
		[Theory]
		[InlineData("Not Valid Uri")]
		public async Task InvalidUri(string uri) =>
			await Assert.ThrowsAsync<UriFormatException>(() => Launcher.CanOpenAsync(uri));

		[Fact]
		public async Task CanOpen_String_NetStandard() =>
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Launcher.CanOpenAsync("http://www.xamarin.com"));

		[Fact]
		public async Task CanOpen_Uri_NetStandard() =>
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Launcher.CanOpenAsync(new Uri("http://www.xamarin.com")));

		[Fact]
		public async Task Open_String_NetStandard() =>
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Launcher.OpenAsync("http://www.xamarin.com"));

		[Fact]
		public async Task Open_Uri_NetStandard() =>
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Launcher.OpenAsync(new Uri("http://www.xamarin.com")));

		[Fact]
		public async Task Open_File_NetStandard() =>
			await Assert.ThrowsAsync<ArgumentNullException>(() => Launcher.OpenAsync(new OpenFileRequest()));
	}
}
