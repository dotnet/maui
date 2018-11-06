using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39768, "PanGestureRecognizer sometimes won't fire completed event when dragging very slowly")]
	public class Bugzilla39768 : TestContentPage
	{
		Image _Image;
		Label _Label;
		const string ImageName = "image";

		[Preserve(AllMembers = true)]
		public class PanContainer : ContentView
		{
			double x, y;

			public EventHandler<PanUpdatedEventArgs> Panning;
			public EventHandler<PanUpdatedEventArgs> PanningCompleted;

			public PanContainer()
			{
				var panGesture = new PanGestureRecognizer();
				panGesture.PanUpdated += OnPanUpdated;
				GestureRecognizers.Add(panGesture);
			}

			void OnPanUpdated(object sender, PanUpdatedEventArgs e)
			{
				switch (e.StatusType)
				{
					case GestureStatus.Started:
						break;

					case GestureStatus.Running:
						Content.TranslationX = x + e.TotalX;
						Content.TranslationY = y + e.TotalY;

						Panning?.Invoke(sender, e);
						break;

					case GestureStatus.Completed:
						x = Content.TranslationX;
						y = Content.TranslationY;

						PanningCompleted?.Invoke(sender, e);
						break;
				}
			}
		}

		protected override void Init()
		{
			_Image = new Image { Source = ImageSource.FromFile("crimson.jpg"), WidthRequest = 350, HeightRequest = 350, AutomationId = ImageName };
			_Label = new Label { Text = "Press and hold the image for 1 second without moving it, then attempt to drag the image. If the image does not move, this test has failed. If this label does not display 'Success' when you have finished the pan, this test has failed." };

			var panView = new PanContainer()
			{
				Content = _Image,
				Panning = (s, e) =>
				{
					_Label.Text = $"TotalX: {e.TotalX}, TotalY: {e.TotalY}";
				},
				PanningCompleted = (s, e) =>
				{
					_Label.Text = "Success";
				}
			};

			Content = new StackLayout
			{
				Children = {

					panView,
					_Label
				}
			};
		}
	}
}
