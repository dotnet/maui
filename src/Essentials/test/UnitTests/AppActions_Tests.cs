using System.Collections.Generic;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
	public class AppActions_Tests
	{
		[Fact]
		public void AppActions_SetActions() =>
			Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => AppActions.SetAsync(new List<AppAction>()));

		[Fact]
		public void AppActions_GetActions() =>
			Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => AppActions.GetAsync());

		[Fact]
		public void AppActions_IsSupported() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => AppActions.IsSupported);
	}
}
