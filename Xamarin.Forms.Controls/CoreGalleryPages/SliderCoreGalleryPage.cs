using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class SliderCoreGalleryPage : CoreGalleryPage<Slider>
	{
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override bool SupportsTapGestureRecognizer
		{
			get { return false; }
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);
			var maximumContainer = new ValueViewContainer<Slider>(Test.Slider.Maximum, new Slider { Maximum = 10, Minimum = 5 }, "Value", value => value.ToString());
			var minimumContainer = new ValueViewContainer<Slider>(Test.Slider.Minimum, new Slider { Maximum = 10 }, "Value", value => value.ToString());
			var valueContainer = new ValueViewContainer<Slider>(Test.Slider.Value, new Slider { Value = 0.5 }, "Value", value => value.ToString());

			var minTrackColorContainer = new ValueViewContainer<Slider>(Test.Slider.MinimumTrackColor, new Slider { MinimumTrackColor = Color.Red, Value = 0.5 }, "Value", value => value.ToString());
			var maxTrackColorContainer = new ValueViewContainer<Slider>(Test.Slider.MaximumTrackColor, new Slider { MaximumTrackColor = Color.Red, Value = 0.5 }, "Value", value => value.ToString());
			var thumbColorContainer = new ValueViewContainer<Slider>(Test.Slider.ThumbColor, new Slider { ThumbColor = Color.Red, Value = 0.5 }, "Value", value => value.ToString());
			var thumbImageContainer = new ValueViewContainer<Slider>(Test.Slider.ThumbImage, new Slider { ThumbImageSource = "coffee.png", Value = 0.5 }, "Value", value => value.ToString());

			var dragStartedContainer = new EventViewContainer<Slider>(Test.Slider.DragStarted, new Slider { Value = 0.5 });
			dragStartedContainer.View.DragStarted += (sender, args) => dragStartedContainer.EventFired();
			var dragCompletedContainer = new EventViewContainer<Slider>(Test.Slider.DragCompleted, new Slider { Value = 0.5 });
			dragCompletedContainer.View.DragCompleted += (sender, args) => dragCompletedContainer.EventFired();

			Add(maximumContainer);
			Add(minimumContainer);
			Add(valueContainer);
			Add(minTrackColorContainer);
			Add(maxTrackColorContainer);
			Add(thumbColorContainer);
			Add(thumbImageContainer);
			Add(dragStartedContainer);
			Add(dragCompletedContainer);
		}
	}
}