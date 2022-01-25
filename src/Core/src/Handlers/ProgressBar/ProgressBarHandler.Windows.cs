#nullable enable
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Microsoft.Maui.Handlers
{
	// To avoid an issue rendering the native ProgressBar on Windows, we wrap it into a Container.
	public partial class ProgressBarHandler : ViewHandler<IProgress, Grid>
	{
		object? _foregroundDefault;

		protected override Grid CreateNativeView() =>
			new Grid();

		public ProgressBar? ProgressBar { get; internal set; }

		protected override void ConnectHandler(Grid nativeView)
		{
			ProgressBar = new ProgressBar { Minimum = 0, Maximum = 1 };
			nativeView.Children.Add(ProgressBar);
			ProgressBar.ValueChanged += OnProgressBarValueChanged;

			SetupDefaults(ProgressBar);
		}

		protected override void DisconnectHandler(Grid nativeView)
		{
			if (ProgressBar != null)
				ProgressBar.ValueChanged -= OnProgressBarValueChanged;
		}

		void SetupDefaults(ProgressBar nativeView)
		{
			_foregroundDefault = nativeView.GetForegroundCache();
		}

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)
		{
			handler.ProgressBar?.UpdateProgress(progress);
		}

		public static void MapProgressColor(ProgressBarHandler handler, IProgress progress)
		{
			handler.ProgressBar?.UpdateProgressColor(progress, handler._foregroundDefault);
		}

		void OnProgressBarValueChanged(object? sender, RangeBaseValueChangedEventArgs rangeBaseValueChangedEventArgs)
		{
			VirtualView?.InvalidateMeasure();
		}
	}
}