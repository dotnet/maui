using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Tests
{
	public class AppActions_Tests
	{
		[Fact]
		public async Task AppActions_SetActions() =>
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => AppActions.SetAsync(new List<AppAction>()));

		[Fact]
		public async Task AppActions_GetActions() =>
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => AppActions.GetAsync());

		[Fact]
		public void AppActions_IsSupported() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => AppActions.IsSupported);
	}
}
