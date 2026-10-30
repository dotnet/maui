using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkProgressBar = Gtk.ProgressBar;
using GtkSpinner = Gtk.Spinner;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="ActivityIndicatorHandler"/> over a <c>Gtk.Spinner</c>. <c>IsRunning</c> starts and stops the spinner; the colour is the
	/// CSS <c>color</c> the spinner icon is drawn with.
	/// </summary>
	public class GtkActivityIndicatorHandler : ActivityIndicatorHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkActivityIndicatorHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkActivityIndicatorHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new GtkSpinner { Halign = global::Gtk.Align.Center, Valign = global::Gtk.Align.Center };

		internal static readonly IPropertyMapper<IActivityIndicator, IActivityIndicatorHandler> GtkMapper =
			new GtkMapperBuilder<IActivityIndicator, IActivityIndicatorHandler>(ActivityIndicatorHandler.Mapper)
				.AddViewMappings()
				.MapGtk<GtkSpinner>(nameof(IActivityIndicator.IsRunning), (h, v, w) => w.Spinning = v.IsRunning)
				.MapGtk<GtkSpinner>(nameof(IActivityIndicator.Color), (h, v, w) => GtkTextStyle.SetColor(w, v.Color))
				// The spinner's size is its natural size (the layout applies explicit sizes).
				.MapGtk<GtkSpinner>(nameof(IView.Width), (h, v, w) => { })
				.MapGtk<GtkSpinner>(nameof(IView.Height), (h, v, w) => { })
				.Build();

		internal static readonly CommandMapper<IActivityIndicator, IActivityIndicatorHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IActivityIndicator, IActivityIndicatorHandler>(ActivityIndicatorHandler.CommandMapper).AddViewCommands().Build();
	}

	/// <summary>
	/// <see cref="ProgressBarHandler"/> over a <c>Gtk.ProgressBar</c>. <c>Progress</c> is the fraction (clamped to 0..1); the colour is the
	/// background of the bar's <c>progress</c> node; a right-to-left flow direction inverts the bar.
	/// </summary>
	public class GtkProgressBarHandler : ProgressBarHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkProgressBarHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkProgressBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new GtkProgressBar { Valign = global::Gtk.Align.Center, Hexpand = true };

		internal static readonly IPropertyMapper<IProgress, IProgressBarHandler> GtkMapper =
			new GtkMapperBuilder<IProgress, IProgressBarHandler>(ProgressBarHandler.Mapper)
				.AddViewMappings()
				.MapGtk<GtkProgressBar>(nameof(IProgress.Progress), (h, v, w) => w.Fraction = System.Math.Max(0, System.Math.Min(1, v.Progress)))
				.MapGtk<GtkProgressBar>(nameof(IProgress.ProgressColor), (h, v, w) =>
					GtkStyle.For(w).Set("background-color", v.ProgressColor?.ToCss(), " progress"))
				.MapGtk<GtkProgressBar>(nameof(IView.FlowDirection), (h, v, w) =>
				{
					w.Inverted = v.FlowDirection == FlowDirection.RightToLeft;
					w.Direction = v.FlowDirection == FlowDirection.RightToLeft ? global::Gtk.TextDirection.Rtl : global::Gtk.TextDirection.Ltr;
				})
				.Build();

		internal static readonly CommandMapper<IProgress, IProgressBarHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IProgress, IProgressBarHandler>(ProgressBarHandler.CommandMapper).AddViewCommands().Build();
	}
}
