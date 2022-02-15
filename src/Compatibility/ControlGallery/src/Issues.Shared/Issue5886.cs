using System;
#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
using System.Linq;
#endif
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Layout)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5886, "Value does not fall with in the expected range Exception while creating NativeView Xamarin Forms UWP", PlatformAffected.UWP)]
	public class Issue5886 : TestContentPage
	{
		public interface IReplaceUWPRendererService
		{
			void ConvertToNative(View formsView);
			void CreateRenderer(View formsView);
		}

		ScrollView scrollView;
		Label label;
		Button button0;
		Button button1;
		protected override void Init()
		{
			scrollView = new ScrollView();

			var grid = new Grid();
			grid.Children.Add(new Label { Text = "Discard Draft ?" });

			scrollView.Content = grid;

			button0 = new Button
			{
				Text = "Create native renderer",
				AutomationId = "Step1"
			};

			button0.Clicked += Button_Clicked1;

			button1 = new Button
			{
				Text = "Start native view conversion",
				AutomationId = "Step2",
				IsVisible = false
			};

			button1.Clicked += Button_Clicked;

			label = new Label
			{
				Text = "You should be able to push first the top button, then the bottom without any exception (Element is already the child of another element.)",
				AutomationId = "ResultLabel"
			};

			var stack = new StackLayout();
			stack.Children.Add(button0);
			stack.Children.Add(button1);
			stack.Children.Add(label);

			Content = stack;
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			DependencyService.Get<IReplaceUWPRendererService>().ConvertToNative(this.scrollView);
			label.Text = "Step 2 OK";
		}

		void Button_Clicked1(object sender, EventArgs e)
		{
			DependencyService.Get<IReplaceUWPRendererService>().CreateRenderer(this.scrollView);
			label.Text = "Step 1 OK";
			button1.IsVisible = true;
		}

#if UITEST && WINDOWS
		[Test]
		public void ReplaceRenderer()
		{
			RunningApp.WaitForElement("Step1");
			RunningApp.Tap("Step1");

			RunningApp.WaitForElement("Step2");
			RunningApp.Tap("Step2");

			var resultLabel = RunningApp.Query("ResultLabel").FirstOrDefault();

			Assert.AreEqual("Step 2 OK", resultLabel.Description);
		}
#endif
	}
}
