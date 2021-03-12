using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class HandlerTestBase<THandler>
	{
		protected abstract Type GetStubType();

		[Fact(DisplayName = "Automation Id is set correctly")]
		[InlineData()]
		public async Task SetAutomationId()
		{
			var view = (StubBase)Activator.CreateInstance(GetStubType());
			view.AutomationId = "TestId";
			var id = await GetValueAsync((IView)view, handler => GetAutomationId(handler));
			Assert.Equal(view.AutomationId, id);
		}
	}
}