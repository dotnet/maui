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

		[Fact(DisplayName = "Semantic Heading is set correctly")]
		[InlineData()]
		public async Task SetSemanticHeading()
		{
			var view = new TStub();
			view.Semantics.HeadingLevel = SemanticHeadingLevel.Level1;
			var id = await GetValueAsync((IView)view, handler => GetSemanticHeading(handler));
			Assert.Equal(view.Semantics.HeadingLevel, id);
		}

		[Fact(DisplayName = "Null Semantics Doesnt throw exception")]
		[InlineData()]
		public async Task NullSemanticsClass()
		{
			var view = new TStub();
			view.Semantics = null;
			view.AutomationId = "CreationFailed";
			var id = await GetValueAsync((IView)view, handler => GetAutomationId(handler));
			Assert.Equal(view.AutomationId, id);
		}
	}
}