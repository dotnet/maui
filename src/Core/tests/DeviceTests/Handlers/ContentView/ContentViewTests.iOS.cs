using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Handlers.ContentView
{
	[Category(TestCategory.ContentView)]
	public partial class ContentViewTests
	{
		[Fact, Category(TestCategory.FlowDirection)]
		public async Task FlowDirectionPropagatesToContent()
		{
			var contentView = new ContentViewStub();
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			contentView.PresentedContent = label;

			// Have to set this manually with the stubs, and the propagation code relies on Parentage
			label.Parent = contentView;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var contentViewHandler = CreateHandler<ContentViewHandler>(contentView);

				contentView.FlowDirection = FlowDirection.RightToLeft;
				contentViewHandler.UpdateValue(nameof(IView.FlowDirection));

				return labelHandler.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.RightToLeft, labelFlowDirection);
		}

		[Fact, Category(TestCategory.FlowDirection)]
		public async Task FlowDirectionPropagatesToDescendants()
		{
			var contentView = new ContentViewStub();
			var layout1 = new LayoutStub() { FlowDirection = FlowDirection.MatchParent };
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			contentView.PresentedContent = layout1;
			layout1.Add(label);
			layout1.Parent = contentView;
			label.Parent = layout1;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var layout1Handler = CreateHandler<LayoutHandler>(layout1);
				var contentViewHandler = CreateHandler<ContentViewHandler>(contentView);

				contentView.FlowDirection = FlowDirection.RightToLeft;
				contentViewHandler.UpdateValue(nameof(IView.FlowDirection));

				return labelHandler.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.RightToLeft, labelFlowDirection);
		}

		[Fact, Category(TestCategory.FlowDirection)]
		public async Task FlowDirectionPropagatesToUpdatedContent()
		{
			var contentView = new ContentViewStub() { FlowDirection = FlowDirection.RightToLeft };
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			var label2 = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			contentView.PresentedContent = label;
			label.Parent = contentView;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var labelHandler2 = CreateHandler<LabelHandler>(label2);
				var contentViewHandler = CreateHandler<ContentViewHandler>(contentView);

				contentView.PresentedContent = label2;
				label.Parent = null;
				label2.Parent = contentView;
				contentViewHandler.UpdateValue(nameof(IContentView.Content));

				return labelHandler2.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.RightToLeft, labelFlowDirection);
		}

		[Fact, Category(TestCategory.FlowDirection)]
		public async Task DoesNotPropagateToContentWithExplicitFlowDirection()
		{
			var contentView = new ContentViewStub();
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.LeftToRight };
			contentView.PresentedContent = label;
			label.Parent = contentView;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var contentViewHandler = CreateHandler<ContentViewHandler>(contentView);

				contentView.FlowDirection = FlowDirection.RightToLeft;
				contentViewHandler.UpdateValue(nameof(IView.FlowDirection));

				return labelHandler.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.LeftToRight, labelFlowDirection);
		}
	}
}
