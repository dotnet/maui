using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	#if UITEST
	[Category(UITestCategories.InputTransparent)]
	#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 5552368, "Transparency Cascading", PlatformAffected.All)]
	public class CascadeInputTransparent : TestNavigationPage
	{
		const string Running = "Running...";
		const string Success = "Success";
		const string Failure = "Failure";
		const string UnderButtonText = "Button";
		const string OverButtonText = "+";
		const string Overlay = "overlay";

		const string CascadesStatic = "Cascades";
		const string CascadesChange = "Cascades (changes)";
		const string NotCascadingStatic = "Not Cascading";
		const string NotCascadingChange = "Not Cascading (changes)";

		protected override void Init()
		{
			PushAsync(Menu());
		}

		ContentPage Menu()
		{
			var layout = new StackLayout();

			layout.Children.Add(new Label { Text = "Select a test below" });

			layout.Children.Add(MenuButton(true, false));
			layout.Children.Add(MenuButton(false, false));
			layout.Children.Add(MenuButton(true, true));
			layout.Children.Add(MenuButton(false, true));

			return new ContentPage { Content = layout };
		}

		Button MenuButton(bool cascades, bool transition)
		{
			var text = cascades
				? transition ? CascadesChange : CascadesStatic
				: transition
				? NotCascadingChange
				: NotCascadingStatic;

			var button = new Button { Text = text, AutomationId = text };

			button.Clicked += (sender, args) => PushAsync(CreateTestPage(cascades, transition));

			return button;
		}

		static ContentPage CreateTestPage(bool cascades, bool transition)
		{
			var grid = new Grid
			{
				AutomationId = "testgrid",
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

			var instructions = new Label
			{
				HorizontalOptions = LayoutOptions.Fill,
				HorizontalTextAlignment = TextAlignment.Center,
				Text =
					$"Wait 5 seconds. Tap the button labeled '{UnderButtonText}'. Then tap the button labeled '{OverButtonText}'."
					+ $" If the label below's text changes to '{Success}' the test has passed."
				};

			grid.Children.Add(instructions);

			var results = new Label
			{
				HorizontalOptions = LayoutOptions.Fill,
				HorizontalTextAlignment = TextAlignment.Center,
				Text = Running
			};

			grid.Children.Add(results);
			Grid.SetRow(results, 1);

			var underButton = new Button
			{
				Text = UnderButtonText,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			bool overPressed = false;
			bool underPressed = false;
			bool layoutTapped = false;

			underButton.Clicked += (sender, args) =>
			{
				underPressed = true;
				EvaluateTest(results, cascades, overPressed, underPressed, layoutTapped);
			};

			var overButton = new Button
			{
				Text = OverButtonText,
				HorizontalOptions = LayoutOptions.End
			};

			overButton.Clicked += (sender, args) =>
			{
				overPressed = true;
				EvaluateTest(results, cascades, overPressed, underPressed, layoutTapped);
			};

			var layout = new StackLayout
			{
				AutomationId = Overlay,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				InputTransparent = true,
				CascadeInputTransparent = cascades,
				BackgroundColor = Color.Blue,
				Opacity = 0.2
			};

			layout.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					layoutTapped = true;
					EvaluateTest(results, cascades, overPressed, underPressed, layoutTapped);
				})
			});

			layout.Children.Add(overButton);

			// Bump up the elevation to cover FastRenderer buttons
			layout.On<Android>().SetElevation(10f);

			grid.Children.Add(underButton);
			Grid.SetRow(underButton, 2);

			grid.Children.Add(layout);
			Grid.SetRow(layout, 2);

			var page = new ContentPage { Content = grid, Title = cascades.ToString() };

			if (transition)
			{
				page.Appearing += async (sender, args) =>
				{
					await Task.Delay(1000);
					cascades = !cascades;
					layout.CascadeInputTransparent = cascades;
				};
			}

			return page;
		}

		static void EvaluateTest(Label results, bool cascades, bool overPressed, bool underPressed, bool layoutTapped)
		{
			if (layoutTapped)
			{
				results.Text = Failure;
				return;
			}

			if (cascades)
			{
				if (overPressed)
				{
					results.Text = Failure;
					return;
				}

				if (underPressed)
				{
					results.Text = Success;
					return;
				}
			}
			else
			{
				if (overPressed && underPressed)
				{
					results.Text = Success;
					return;
				}
			}

			results.Text = Running;
		}

		static IEnumerable<string> TestList => new List<string> { CascadesChange, CascadesStatic, NotCascadingChange, NotCascadingStatic };

		#if UITEST
		[Test, TestCaseSource(nameof(TestList))]
		public void TransparencyCascading(string test)
		{
			RunningApp.WaitForElement(test);
			RunningApp.Tap(test);

			Task.Delay(2000).Wait();

			RunningApp.WaitForElement(UnderButtonText);
			RunningApp.Tap(UnderButtonText);
			RunningApp.Tap(OverButtonText);

			RunningApp.WaitForElement(Success);
		}


		#endif
	}
}