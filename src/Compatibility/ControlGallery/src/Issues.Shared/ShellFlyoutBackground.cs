using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Flyout Background",
		   PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
#endif
	public class ShellFlyoutBackground : TestShell
	{
		public ShellFlyoutBackground()
		{
		}

		protected override void Init()
		{
			for (int i = 0; i < 20; i++)
			{
				AddFlyoutItem(CreateContentPage(), $"Item {i}");
			}

			FlyoutBackgroundImage = "photo.jpg";
			FlyoutBackgroundImageAspect = Aspect.AspectFill;
			FlyoutVerticalScrollMode = ScrollMode.Enabled;
		}

		ContentPage CreateContentPage()
		{
			var layout = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						AutomationId = "PageLoaded",
						Text = "Toggle Different Options to Verify Flyout Behaves as Expected"
					},
					new Button()
					{
						Text = "Toggle Image",
						Command = new Command(() =>
						{
							if(FlyoutBackgroundImage == null)
								FlyoutBackgroundImage = "photo.jpg";
							else
								FlyoutBackgroundImage = null;
						})
					},
					new Button()
					{
						Text = "Toggle Red Color",
						Command = new Command(() =>
						{
							FlyoutBackground = null;
							if(FlyoutBackgroundColor == null)
								FlyoutBackgroundColor = Colors.Red;
							else
								FlyoutBackgroundColor = null;
						})
					},
					new Button()
					{
						// Broken on iOS 14
						Text = "Toggle Red Color With Alpha",
						Command = new Command(() =>
						{
							FlyoutBackground = null;
							if(FlyoutBackgroundColor == null)
								FlyoutBackgroundColor = Colors.Red.MultiplyAlpha(0.7f);
							else
								FlyoutBackgroundColor = null;
						})
					},
					new Button()
					{
						Text = "Toggle Brush",
						Command = new Command(() =>
						{
							RadialGradientBrush radialGradientBrush = new RadialGradientBrush(
								new GradientStopCollection()
								{
									new GradientStop(Colors.Red, 0.1f),
									new GradientStop(Colors.DarkBlue, 1.0f),
								});

							FlyoutBackgroundColor = null;
							if(FlyoutBackground != null)
								FlyoutBackground = null;
							else
								FlyoutBackground = radialGradientBrush;
						})
					},
					new Button()
					{
						Text = "Toggle Flyout Content",
						Command = new Command(() =>
						{
							if (FlyoutContent is ScrollView)
								FlyoutContent = null;
							else if (FlyoutContent == null)
								FlyoutContent = new Label()
								{
									AutomationId = "LabelContent",
									Text = "Only Label"
								};
							else
								FlyoutContent = new ScrollView()
								{
									AutomationId = "ScrollViewContent",
									Content = new Label() { Text = "Label inside ScrollView" }
								};
						}),
						AutomationId = "ToggleFlyoutContent"
					},
					new Button()
					{
						Text = "Toggle Header/Footer",
						Command = new Command(() =>
						{
							if (FlyoutHeader == null)
							{
								FlyoutFooter =
									new BoxView()
									{
										BackgroundColor = Colors.Purple,
										HeightRequest = 50
									};

								FlyoutHeader =
									new BoxView()
									{
										BackgroundColor = Colors.Blue,
										HeightRequest = 50
									};
							}
							else
							{
								FlyoutHeader = FlyoutFooter = null;
							}
						}),
						AutomationId = "ToggleHeaderFooter"
					},
					new Button()
					{
						Text = "Toggle Header/Footer Transparent",
						Command = new Command(() =>
						{
							if (FlyoutHeader == null)
							{
								FlyoutFooter =
									new Label()
									{
										Text = "The FOOTER",
										TextColor = Colors.Blue,
										HeightRequest = 50
									};

								FlyoutHeader =
								new StackLayout
								{
									Orientation = StackOrientation.Horizontal,
									HeightRequest = 100,
									Children = {
										new Label()
										{
											Text = "The HEADER",
											FontSize = 25,
											FontAttributes = FontAttributes.Bold,
											VerticalTextAlignment = TextAlignment.Center,
											TextColor = Colors.Blue,
										},
										new Button()
										{
											Text = "OK",
											FontSize = 25,
											TextColor = Colors.Green,
											Command = new Command(() => DisplayAlert("Button", "ThisButtonWorks", "OK"))
										}
									}
								};
							}
							else
							{
								FlyoutHeader = FlyoutFooter = null;
							}
						}),
						AutomationId = "ToggleHeaderFooterTransparent"
					}
				}
			};

			Button aspectBackgroundChange = null;
			aspectBackgroundChange = new Button()
			{
				Text = $"Change Flyout Background Image Aspect: {FlyoutBackgroundImageAspect}",
				Command = new Command(() =>
				{
					int inc = (int)FlyoutBackgroundImageAspect;
					inc++;

					if (inc >= Enum.GetNames(typeof(Aspect)).Length)
					{
						inc = 0;
					}

					FlyoutBackgroundImageAspect = (Aspect)inc;
					aspectBackgroundChange.Text = $"Change Flyout Background Image Aspect: {FlyoutBackgroundImageAspect}";
				})
			};

			layout.Children.Add(aspectBackgroundChange);

			return new ContentPage()
			{
				Content = layout
			};
		}
	}
}
