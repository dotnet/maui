using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class HandlerTestBase<THandler, TStub>
	{
		[Fact(DisplayName = "Automation Id is set correctly")]
		[InlineData()]
		public async Task SetAutomationId()
		{
			var view = new TStub
			{
				AutomationId = "TestId"
			};
			var id = await GetValueAsync(view, handler => GetAutomationId(handler));
			Assert.Equal(view.AutomationId, id);
		}

		[Theory(DisplayName = "Visibility is set correctly")]
		[InlineData(Visibility.Collapsed)]
		[InlineData(Visibility.Hidden)]
		public virtual async Task SetVisibility(Visibility visibility)
		{
			var view = new TStub
			{
				Visibility = visibility
			};

			var id = await GetValueAsync(view, handler => GetVisibility(handler));
			Assert.Equal(view.Visibility, id);
		}

		[Fact(DisplayName = "Semantic Description is set correctly")]
		[InlineData()]
		public async Task SetSemanticDescription()
		{
			var view = new TStub();
			view.Semantics.Description = "Test";
			var id = await GetValueAsync(view, handler => GetSemanticDescription(handler));
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
			var id = await GetValueAsync(view, handler => GetSemanticDescription(handler));
			Assert.Equal(view.Semantics.Description, id);
		}

		[Fact(DisplayName = "Semantic Heading is set correctly")]
		[InlineData()]
		public async Task SetSemanticHeading()
		{
			var view = new TStub();
			view.Semantics.HeadingLevel = SemanticHeadingLevel.Level1;
			var id = await GetValueAsync(view, handler => GetSemanticHeading(handler));
			Assert.Equal(view.Semantics.HeadingLevel, id);
		}

		[Fact(DisplayName = "Null Semantics Doesnt throw exception")]
		[InlineData()]
		public async Task NullSemanticsClass()
		{
			var view = new TStub
			{
				Semantics = null,
				AutomationId = "CreationFailed"
			};
			var id = await GetValueAsync(view, handler => GetAutomationId(handler));
			Assert.Equal(view.AutomationId, id);
		}
	}
}