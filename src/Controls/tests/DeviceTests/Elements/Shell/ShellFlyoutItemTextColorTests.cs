using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests : ControlsHandlerTestBase
	{
		// Regression tests for:
		//  - https://github.com/dotnet/maui/issues/18994 (class style "FlyoutItemLabelStyle" TextColor ignored on Android)
		//  - DevDiv 3032719 (implicit "Style TargetType=Label" TextColor ignored on Android)
		// The Android default flyout item text color used to be applied through the
		// "Default_FlyoutItemLabelStyle" class (StyleLocal specificity), which outranked the user's
		// implicit style and tied with the user's flyout class style. It is now applied at the lowest
		// specificity so any user style overrides it (matching iOS/Windows, which set no default color).
		[Fact]
		public async Task ImplicitLabelStyleOverridesDefaultFlyoutItemTextColor()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var shell = new Shell
				{
					Resources = new ResourceDictionary
					{
						new Style(typeof(Label))
						{
							Setters =
							{
								new Setter { Property = Label.TextColorProperty, Value = Colors.Red }
							}
						}
					}
				};

				var flyoutItem = new FlyoutItem { Title = "Item", Items = { new ContentPage() } };
				shell.Items.Add(flyoutItem);

				var label = GetDefaultFlyoutItemLabel(shell, flyoutItem);

				Assert.Equal(Colors.Red, label.TextColor);
			});
		}

		[Fact]
		public async Task FlyoutItemLabelClassStyleOverridesDefaultFlyoutItemTextColor()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var shell = new Shell
				{
					Resources = new ResourceDictionary
					{
						new Style(typeof(Label))
						{
							Class = FlyoutItem.LabelStyle,
							Setters =
							{
								new Setter { Property = Label.TextColorProperty, Value = Colors.Red }
							}
						}
					}
				};

				var flyoutItem = new FlyoutItem { Title = "Item", Items = { new ContentPage() } };
				shell.Items.Add(flyoutItem);

				var label = GetDefaultFlyoutItemLabel(shell, flyoutItem);

				Assert.Equal(Colors.Red, label.TextColor);
			});
		}

		static Label GetDefaultFlyoutItemLabel(Shell shell, BindableObject flyoutItem)
		{
			var content = (shell as IShellController).GetFlyoutItemDataTemplate(flyoutItem).CreateContent();

			if (content is BindableObject bindableContent)
				bindableContent.BindingContext = flyoutItem;

			var element = (Element)content;
			element.Parent = shell;

			return element.LogicalChildrenInternal.OfType<Label>().First();
		}
	}
}
