using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
	[Category(UITestCategories.Gestures)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 57515, "PinchGestureRecognizer not getting called on Android ", PlatformAffected.Android)]
	public class Bugzilla57515 : TestContentPage
	{
		const string ZoomImage = "zoomImg";
		const string ZoomContainer = "zoomContainer";

		protected override void Init()
		{
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = 80 },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var scaleLabel = new Label();
			layout.Children.Add(scaleLabel);

			var pinchToZoomContainer = new PinchToZoomContainer
			{
				Margin = new Thickness(80),
				AutomationId = ZoomContainer,
				Content = new Image
				{
					AutomationId = ZoomImage,
					Source = ImageSource.FromFile("oasis.jpg")
				}
			};

			Grid.SetRow(pinchToZoomContainer, 1);
			layout.Children.Add(pinchToZoomContainer);

			scaleLabel.BindingContext = pinchToZoomContainer;
			scaleLabel.SetBinding(Label.TextProperty, new Binding("CurrentScale"));

			Content = layout;
		}

		class PinchToZoomContainer : ContentView
		{
			public static readonly BindableProperty CurrentScaleProperty =
				BindableProperty.Create("CurrentScale", typeof(double), typeof(PinchToZoomContainer), 1.0);

			public double CurrentScale
			{
				get { return (double)GetValue(CurrentScaleProperty); }
				set { SetValue(CurrentScaleProperty, value); }
			}

			double startScale = 1;
			double xOffset = 0;
			double yOffset = 0;

			public PinchToZoomContainer()
			{
				var pinchGesture = new PinchGestureRecognizer();
				pinchGesture.PinchUpdated += OnPinchUpdated;
				GestureRecognizers.Add(pinchGesture);
			}

			void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
			{
				if (e.Status == GestureStatus.Started)
				{
					// Store the current scale factor applied to the wrapped user interface element,
					// and zero the components for the center point of the translate transform.
					startScale = Content.Scale;
					Content.AnchorX = 0;
					Content.AnchorY = 0;
				}
				if (e.Status == GestureStatus.Running)
				{
					// Calculate the scale factor to be applied.
					CurrentScale += (e.Scale - 1) * startScale;
					CurrentScale = Math.Max(1, CurrentScale);

					// The ScaleOrigin is in relative coordinates to the wrapped user interface element,
					// so get the X pixel coordinate.
					double renderedX = Content.X + xOffset;
					double deltaX = renderedX / Width;
					double deltaWidth = Width / (Content.Width * startScale);
					double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

					// The ScaleOrigin is in relative coordinates to the wrapped user interface element,
					// so get the Y pixel coordinate.
					double renderedY = Content.Y + yOffset;
					double deltaY = renderedY / Height;
					double deltaHeight = Height / (Content.Height * startScale);
					double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

					// Calculate the transformed element pixel coordinates.
					double targetX = xOffset - (originX * Content.Width) * (CurrentScale - startScale);
					double targetY = yOffset - (originY * Content.Height) * (CurrentScale - startScale);

					// Apply translation based on the change in origin.
					Content.TranslationX = targetX.Clamp(-Content.Width * (CurrentScale - 1), 0);
					Content.TranslationY = targetY.Clamp(-Content.Height * (CurrentScale - 1), 0);

					// Apply scale factor
					Content.Scale = CurrentScale;
				}
				if (e.Status == GestureStatus.Completed)
				{
					// Store the translation delta's of the wrapped user interface element.
					xOffset = Content.TranslationX;
					yOffset = Content.TranslationY;
				}
			}
		}

#if UITEST
		[Test]
		public void Bugzilla57515Test()
		{
			RunningApp.WaitForElement(ZoomContainer);
			RunningApp.WaitForElement("1");
			RunningApp.PinchToZoomIn(ZoomContainer);
			RunningApp.WaitForNoElement("1"); // The scale should have changed during the zoom
		}
#endif
	}

	public static class DoubleExtensions
	{
		public static double Clamp(this double self, double min, double max)
		{
			return Math.Min(max, Math.Max(self, min));
		}
	}
}