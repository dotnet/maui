using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12484, "Unable to set ControlTemplate for TemplatedView in Xamarin.Forms version 5.0", PlatformAffected.Android)]
	public partial class Issue12484 : TestContentPage
	{
		public Issue12484()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			Title = "Issue 12484";
		}

#if UITEST && __ANDROID__
		[Test]
		public void Issue12484ControlTemplateRendererTest()
		{
			RunningApp.WaitForElement("Success");
		}
#endif
	}

	public class Issue12484CustomView : TemplatedView
	{
		public class Issue12484Template : ContentView
		{
			public Issue12484Template()
			{
				var label = new Label
				{
					AutomationId = "Success",
					Text = "If this text appear, the test has passed.",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				var content = new Grid();
				content.Children.Add(label);

				Content = content;
			}
		}

		public Issue12484CustomView()
		{
			ControlTemplate = new ControlTemplate(typeof(Issue12484Template));
		}
	}
}