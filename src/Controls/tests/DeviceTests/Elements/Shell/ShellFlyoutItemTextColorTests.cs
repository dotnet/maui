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
		// Regression test for DevDiv 3032719: an implicit "Style TargetType=Label" that sets TextColor
		// was ignored by Shell flyout items on Android (it worked on iOS/Windows). The Android default
		// flyout item text color used to be applied through the "Default_FlyoutItemLabelStyle" class
		// (StyleLocal specificity), which outranked the user's implicit style. It is now applied at the
		// lowest specificity so any user style overrides it.
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
