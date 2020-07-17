using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	[ContentProperty(nameof(GradientStops))]
	public abstract class GradientBrush : Brush
	{
		static bool IsExperimentalFlagSet = false;

		public GradientBrush()
		{
			VerifyExperimental(nameof(GradientBrush));

			GradientStops = new GradientStopCollection();
		}

		internal static void VerifyExperimental([CallerMemberName] string memberName = "", string constructorHint = null)
		{
			if (IsExperimentalFlagSet)
				return;

			ExperimentalFlags.VerifyFlagEnabled(nameof(GradientBrush), ExperimentalFlags.BrushExperimental, constructorHint, memberName);

			IsExperimentalFlagSet = true;
		}

		public static readonly BindableProperty GradientStopsProperty = BindableProperty.Create(
			nameof(GradientStops), typeof(GradientStopCollection), typeof(GradientBrush), null);

		public GradientStopCollection GradientStops
		{
			get => (GradientStopCollection)GetValue(GradientStopsProperty);
			set => SetValue(GradientStopsProperty, value);
		}
	}
}