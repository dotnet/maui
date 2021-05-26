using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;


#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell AppearanceChange",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
#endif
	public class ShellAppearanceChangeTests : TestShell
	{
		protected override void Init()
		{
			AddContentPage(GetContentPage(Colors.Red));
		}

		ContentPage GetContentPage(Color color)
		{
			var stackLayout = new StackLayout()
			{
				Children =
				{
					new Label() { Text = $"I should have a {color} background"},
					new Button()
					{
						Text = "Push Purple Page",
						Command = new Command(() =>
						{
							var contentPage = GetContentPage(Colors.Purple);
							Navigation.PushAsync(contentPage);
						}),
					},
				}
			};

			if (Navigation?.NavigationStack != null)
			{
				stackLayout.Children.Add(new Button()
				{
					Text = "Insert Orange Page Before Current Page",
					Command = new Command(() =>
					{
						var contentPage = GetContentPage(Colors.Orange);
						Navigation.InsertPageBefore(contentPage, Navigation.NavigationStack.Last());
					}),
				});

				stackLayout.Children.Add(new Button()
				{
					Text = "Pop Page",
					Command = new Command(() =>
					{
						Navigation.PopAsync();
					}),
				});

				stackLayout.Children.Add(new Button()
				{
					Text = "Remove Current Page",
					Command = new Command(() =>
					{
						Navigation.RemovePage(Navigation.NavigationStack.Last());
					}),
				});

				stackLayout.Children.Add(new Button()
				{
					Text = "Pop To Root",
					Command = new Command(() =>
					{
						Navigation.PopToRootAsync();
					}),
				});
			}

			var page = new ContentPage()
			{
				Title = color.ToString(),
				Content = stackLayout
			};

			Shell.SetBackgroundColor(page, color);
			return page;
		}
	}
}
