using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Image)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13109, "[Bug] NullReference Exception thrown when load image from ImageSource in Xamarin Forms UWP", PlatformAffected.UWP)]
	public class Issue13109 : TestContentPage
	{
		public Issue13109()
		{
			Title = "Issue 13109";

			Issue13109Icon issue13109Icon = new Issue13109Icon();

			var layout = new StackLayout();

			var instructions = new Label
			{
				AutomationId = "TestReady",
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Without exceptions, the test has passed."
			};

			layout.Children.Add(instructions);

			Content = layout;
		}

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class Issue13109Icon
	{
		ImageSource _icon = "Microsoft.Maui.Controls.ControlGallery.WindowsUniversal/calculator.png";

		public ImageSource Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				_icon = value;
			}
		}

		public Issue13109Icon()
		{
			DependencyService.Get<IIssue13109Helper>().SetImage(Icon);
		}
	}

	[Preserve(AllMembers = true)]
	public interface IIssue13109Helper
	{
		void SetImage(ImageSource imageSource);
	}
}
