using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9054, "[Bug] ImageButton.Aspect Property is always Fill",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ImageButton)]
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
#endif
	public class Issue9054 : TestContentPage
	{
		protected override void Init()
		{
			StackLayout stackLayout = new StackLayout();
			string imageSource = "coffee.png";
			ImageButton imageButton1 = new ImageButton()
			{
				Source = imageSource,
				Aspect = Aspect.AspectFill,
				AutomationId = "TestImage"
			};

			ImageButton imageButton2 = new ImageButton()
			{
				Source = imageSource,
				Aspect = Aspect.AspectFit,
				AutomationId = "TestImage"
			};

			ImageButton imageButton3 = new ImageButton()
			{
				Source = imageSource,
				Aspect = Aspect.Fill,
				AutomationId = "TestImage"
			};

			stackLayout.Children.Add(new Label() { Text = $"Verify Image Button Aspects Are Working" });
			stackLayout.Children.Add(new Label() { Text = $"{imageButton1.Aspect}" });
			stackLayout.Children.Add(imageButton1);
			stackLayout.Children.Add(new Label() { Text = $"{imageButton2.Aspect}" });
			stackLayout.Children.Add(imageButton2);
			stackLayout.Children.Add(new Label() { Text = $"{imageButton3.Aspect}" });
			stackLayout.Children.Add(imageButton3);

			Content = stackLayout;
		}
	}
}
