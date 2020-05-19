using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5131, "Tab order on Shell flyout menu items", PlatformAffected.Default)]
	public class Issue5131 : TestShell
	{
		ShellItem GenerateItem(string title, int tabIndex, bool tabStop)
		{
			return new ShellItem
			{
				TabIndex = tabIndex,
				IsTabStop = tabStop,
				Title = title,
				Route = $"{title}.{tabIndex}",
				Items =
				{
					new ShellSection
					{
						Items =
						{
							new System.Maui.ShellContent
							{
								Content = new ContentPage()
							}
						}
					}
				}
			};
		}

		protected override void Init()
		{
			StackLayout flyout = new StackLayout();
			FlowDirection = FlowDirection.RightToLeft;
			FlyoutHeader = flyout;
			Items.Add(GenerateItem("First", 1, true));
			Items.Add(GenerateItem("Third", 3, true));
			Items.Add(GenerateItem("Skip", 2, false));
			Items.Add(GenerateItem("Second", 2, true));
		}
	}
}