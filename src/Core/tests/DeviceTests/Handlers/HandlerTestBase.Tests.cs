using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class HandlerTestBase<THandler, TStub>
	{
		[Fact(DisplayName = "Automation Id is set correctly")]
		[InlineData()]
		public async Task SetAutomationId()
		{
			var view = new TStub();
			view.AutomationId = "TestId";
			var id = await GetValueAsync((IView)view, handler => GetAutomationId(handler));
			Assert.Equal(view.AutomationId, id);
		}

		[Fact(DisplayName = "Semantic Description is set correctly")]
		[InlineData()]
		public async Task SetSemanticDescription()
		{
			var view = new TStub();
			view.Semantics.Description = "Test";
			var id = await GetValueAsync((IView)view, handler => GetSemanticDescription(handler));
			Assert.Equal(view.Semantics.Description, id);
		}

		[Fact(DisplayName = "Semantic Hint is set correctly"
#if MONOANDROID
			, Skip = "This value can't be validated through automated tests"
#endif
		)]
		[InlineData()]
		public async Task SetSemanticHint()
		{
			var view = new TStub();
			view.Semantics.Description = "Test";
			var id = await GetValueAsync((IView)view, handler => GetSemanticDescription(handler));
			Assert.Equal(view.Semantics.Description, id);
		}
	}
}