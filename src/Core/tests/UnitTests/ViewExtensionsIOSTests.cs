using Xunit;

#if IOS
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
#endif

namespace Microsoft.Maui.UnitTests
{
#if IOS
	public class ViewExtensionsIOSTests
	{
		[Theory]
		[InlineData(false, false, false)]
		[InlineData(false, true, false)]
		[InlineData(true, false, true)]
		[InlineData(true, true, false)]
		public void UpdateIsEnabledSetsUserInteractionForNonUIControls(bool isEnabled, bool inputTransparent, bool expectedUserInteractionEnabled)
		{
			var platformView = new UIView();
			var view = new ViewStub
			{
				IsEnabled = isEnabled,
				InputTransparent = inputTransparent
			};

			platformView.UpdateIsEnabled(view);

			Assert.Equal(expectedUserInteractionEnabled, platformView.UserInteractionEnabled);
		}

		[Theory]
		[InlineData(false, false, false)]
		[InlineData(false, true, false)]
		[InlineData(true, false, true)]
		[InlineData(true, true, false)]
		public void UpdateIsEnabledSetsUserInteractionOverrideForLayouts(bool isEnabled, bool inputTransparent, bool expectedUserInteractionEnabled)
		{
			var platformView = new LayoutView();
			var view = new ViewStub
			{
				IsEnabled = isEnabled,
				InputTransparent = inputTransparent
			};

			platformView.UpdateIsEnabled(view);

			Assert.Equal(expectedUserInteractionEnabled, platformView.UserInteractionEnabledOverride);
		}

		[Theory]
		[InlineData(false, false, false)]
		[InlineData(false, true, false)]
		[InlineData(true, false, true)]
		[InlineData(true, true, false)]
		public void UpdateInputTransparentSetsUserInteractionForNonUIControls(bool isEnabled, bool inputTransparent, bool expectedUserInteractionEnabled)
		{
			var platformView = new UIView();
			var view = new ViewStub
			{
				IsEnabled = isEnabled,
				InputTransparent = inputTransparent
			};

			platformView.UpdateInputTransparent(new ViewHandlerStub(), view);

			Assert.Equal(expectedUserInteractionEnabled, platformView.UserInteractionEnabled);
		}

		[Theory]
		[InlineData(false, false, false)]
		[InlineData(false, true, false)]
		[InlineData(true, false, true)]
		[InlineData(true, true, false)]
		public void UpdateInputTransparentSetsUserInteractionOverrideForLayouts(bool isEnabled, bool inputTransparent, bool expectedUserInteractionEnabled)
		{
			var platformView = new LayoutView();
			var view = new ViewStub
			{
				IsEnabled = isEnabled,
				InputTransparent = inputTransparent
			};

			platformView.UpdateInputTransparent(new ViewHandlerStub(), view);

			Assert.Equal(expectedUserInteractionEnabled, platformView.UserInteractionEnabledOverride);
		}
	}
#endif
}
