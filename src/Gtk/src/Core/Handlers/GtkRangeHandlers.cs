using System;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkScale = Gtk.Scale;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="SliderHandler"/> over a horizontal <c>Gtk.Scale</c>. The minimum/maximum/value live in the adjustment of the scale; the
	/// track colours and the thumb colour are CSS (<c>trough</c>, <c>trough highlight</c>, <c>slider</c>).
	/// </summary>
	/// <remarks><c>ThumbImageSource</c> is drawn as a CSS <c>background-image</c> on the <c>slider</c> node (any source type; it takes precedence over <c>ThumbColor</c>).</remarks>
	public class GtkSliderHandler : SliderHandler
	{
		readonly GtkUpdateGuard _guard = new();
		global::Gtk.GestureClick? _gesture;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkSliderHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkSliderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		readonly GtkImageLoader _thumbLoader = new();
		bool _hasThumbImage;

		/// <inheritdoc/>
		protected override object CreatePlatformView()
		{
			var scale = new GtkScale(global::Gtk.Orientation.Horizontal, 0, 1, 0.01)
			{
				DrawValue = false,
				Hexpand = true,
				Valign = global::Gtk.Align.Center,
			};
			return scale;
		}

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not GtkScale scale)
				return;

			scale.ValueChanged += OnValueChanged;

			// DragStarted/DragCompleted: observed in the capture phase without claiming the sequence.
			_gesture = new global::Gtk.GestureClick { PropagationPhase = global::Gtk.PropagationPhase.Capture };
			_gesture.Pressed += OnPressed;
			_gesture.Released += OnReleased;
			scale.AddController(_gesture);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is GtkScale scale)
			{
				scale.ValueChanged -= OnValueChanged;
				if (_gesture is not null)
				{
					_gesture.Pressed -= OnPressed;
					_gesture.Released -= OnReleased;
					scale.RemoveController(_gesture);
					_gesture = null;
				}
			}

			_thumbLoader.Reset();
			base.DisconnectHandler(platformView);
		}

		/// <summary>
		/// <c>ThumbImageSource</c>: the decoded image replaces the <c>slider</c> node's look through CSS (inline PNG <c>background-image</c>, sized to the
		/// image, no border or shadow); clearing it restores <c>ThumbColor</c>.
		/// </summary>
		void UpdateThumbImage(ISlider view, GtkScale scale)
		{
			_ = _thumbLoader.UpdateAsync(this, new GtkImageSourcePart(view.ThumbImageSource), texture =>
			{
				var style = GtkStyle.For(scale);
				var url = GtkTextureCss.ToUrl(texture);
				_hasThumbImage = url is not null;
				style.ClearPrefix("background", " slider");
				style.Set("min-width", null, " slider");
				style.Set("min-height", null, " slider");
				style.Set("border-color", null, " slider");
				style.Set("box-shadow", null, " slider");

				if (url is null || texture is null)
				{
					if (((IElementHandler)this).VirtualView is ISlider slider)
						SetThumbColor(style, slider.ThumbColor);
					return;
				}

				style.Set("background-image", url, " slider");
				style.Set("background-size", "100% 100%", " slider");
				style.Set("background-repeat", "no-repeat", " slider");
				style.Set("background-color", "transparent", " slider");
				style.Set("border-color", "transparent", " slider");
				style.Set("box-shadow", "none", " slider");
				style.Set("min-width", texture.Width + "px", " slider");
				style.Set("min-height", texture.Height + "px", " slider");
			});
		}

		// The theme paints the thumb with a background-image that would hide the colour.
		static void SetThumbColor(GtkStyle style, Microsoft.Maui.Graphics.Color? color)
		{
			style.Set("background-color", color?.ToCss(), " slider");
			style.Set("background-image", color is null ? null : "none", " slider");
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		ISlider? View => ((IElementHandler)this).VirtualView as ISlider;

		void OnValueChanged(object? sender, EventArgs e)
		{
			if (_guard.IsActive || sender is not GtkScale scale || View is not { } view)
				return;

			if (view.Value != scale.Value)
				view.Value = scale.Value;
		}

		void OnPressed(object o, global::Gtk.PressedArgs args) => View?.DragStarted();

		void OnReleased(object o, global::Gtk.ReleasedArgs args) => View?.DragCompleted();

		static void Guarded(IViewHandler handler, Action action)
		{
			if (handler is GtkSliderHandler gtk)
				gtk._guard.Run(action);
			else
				action();
		}

		static void UpdateRange(IViewHandler handler, ISlider view, GtkScale scale) => Guarded(handler, () =>
		{
			double min = view.Minimum;
			double max = view.Maximum;

			// GTK ranges need min < max (it asserts otherwise); a degenerate range keeps a sliver so the scale stays valid.
			if (!(max > min))
				max = min + 0.0001;

			var adjustment = scale.Adjustment;
			adjustment.Lower = min;
			adjustment.Upper = max;
			adjustment.StepIncrement = (max - min) / 100;
			adjustment.PageIncrement = (max - min) / 10;
			scale.Value = Math.Max(min, Math.Min(max, view.Value));
		});

		internal static readonly IPropertyMapper<ISlider, ISliderHandler> GtkMapper =
			new GtkMapperBuilder<ISlider, ISliderHandler>(SliderHandler.Mapper)
				.AddViewMappings()
				.MapGtk<GtkScale>(nameof(IRange.Minimum), (h, v, w) => UpdateRange(h, v, w))
				.MapGtk<GtkScale>(nameof(IRange.Maximum), (h, v, w) => UpdateRange(h, v, w))
				.MapGtk<GtkScale>(nameof(IRange.Value), (h, v, w) => Guarded(h, () =>
					w.Value = Math.Max(w.Adjustment.Lower, Math.Min(w.Adjustment.Upper, v.Value))))
				.MapGtk<GtkScale>(nameof(ISlider.MinimumTrackColor), (h, v, w) =>
					GtkStyle.For(w).Set("background-color", v.MinimumTrackColor?.ToCss(), " trough highlight"))
				.MapGtk<GtkScale>(nameof(ISlider.MaximumTrackColor), (h, v, w) =>
					GtkStyle.For(w).Set("background-color", v.MaximumTrackColor?.ToCss(), " trough"))
				.MapGtk<GtkScale>(nameof(ISlider.ThumbColor), (h, v, w) =>
				{
					if (!(h as GtkSliderHandler)?._hasThumbImage ?? true)
						SetThumbColor(GtkStyle.For(w), v.ThumbColor);
				})
				.MapGtk<GtkScale>(nameof(ISlider.ThumbImageSource), (h, v, w) => (h as GtkSliderHandler)?.UpdateThumbImage(v, w))
				.Build();

		internal static readonly CommandMapper<ISlider, ISliderHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<ISlider, ISliderHandler>(SliderHandler.CommandMapper).AddViewCommands().Build();
	}

	/// <summary>
	/// A linked pair of "-" / "+" buttons that steps a value between a minimum and a maximum by an interval (MAUI <c>Stepper</c> semantics:
	/// the value is clamped, the "-" button is insensitive at the minimum and the "+" button at the maximum).
	/// </summary>
	public sealed class MauiGtkStepper : global::Gtk.Box
	{
		readonly global::Gtk.Button _minus = new("−");
		readonly global::Gtk.Button _plus = new("+");
		double _minimum;
		double _maximum = 100;
		double _interval = 1;
		double _value;

		/// <summary>Creates the stepper.</summary>
		public MauiGtkStepper()
			: base(global::Gtk.Orientation.Horizontal, 0)
		{
			AddCssClass("linked");
			Halign = global::Gtk.Align.Start;
			Valign = global::Gtk.Align.Center;
			Append(_minus);
			Append(_plus);
			_minus.Clicked += (s, e) => StepBy(-_interval);
			_plus.Clicked += (s, e) => StepBy(_interval);
			UpdateButtons();
		}

		/// <summary>Raised when the value changed through the buttons or <see cref="Value"/>.</summary>
		public event EventHandler? ValueChanged;

		/// <summary>The "-" button.</summary>
		public global::Gtk.Button MinusButton => _minus;

		/// <summary>The "+" button.</summary>
		public global::Gtk.Button PlusButton => _plus;

		/// <summary>The smallest value.</summary>
		public double Minimum
		{
			get => _minimum;
			set
			{
				_minimum = value;
				Value = _value;
				UpdateButtons();
			}
		}

		/// <summary>The largest value.</summary>
		public double Maximum
		{
			get => _maximum;
			set
			{
				_maximum = value;
				Value = _value;
				UpdateButtons();
			}
		}

		/// <summary>The amount a button press adds or subtracts.</summary>
		public double Interval
		{
			get => _interval;
			set => _interval = value;
		}

		/// <summary>The current value, clamped to [<see cref="Minimum"/>, <see cref="Maximum"/>].</summary>
		public double Value
		{
			get => _value;
			set
			{
				double clamped = Math.Max(_minimum, Math.Min(_maximum, value));
				bool changed = clamped != _value;
				_value = clamped;
				UpdateButtons();
				if (changed)
					ValueChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		void StepBy(double delta) => Value = _value + delta;

		void UpdateButtons()
		{
			_minus.Sensitive = _value > _minimum;
			_plus.Sensitive = _value < _maximum;
		}
	}

	/// <summary>
	/// <see cref="StepperHandler"/> over a <see cref="MauiGtkStepper"/>.
	/// </summary>
	public class GtkStepperHandler : StepperHandler
	{
		readonly GtkUpdateGuard _guard = new();

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkStepperHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkStepperHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkStepper();

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			if (platformView is MauiGtkStepper stepper)
				stepper.ValueChanged += OnValueChanged;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkStepper stepper)
				stepper.ValueChanged -= OnValueChanged;

			base.DisconnectHandler(platformView);
		}

		void OnValueChanged(object? sender, EventArgs e)
		{
			if (_guard.IsActive || sender is not MauiGtkStepper stepper || ((IElementHandler)this).VirtualView is not IStepper view)
				return;

			if (view.Value != stepper.Value)
				view.Value = stepper.Value;
		}

		static void Guarded(IViewHandler handler, Action action)
		{
			if (handler is GtkStepperHandler gtk)
				gtk._guard.Run(action);
			else
				action();
		}

		internal static readonly IPropertyMapper<IStepper, IStepperHandler> GtkMapper =
			new GtkMapperBuilder<IStepper, IStepperHandler>(StepperHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkStepper>(nameof(IStepper.Minimum), (h, v, w) => Guarded(h, () => w.Minimum = v.Minimum))
				.MapGtk<MauiGtkStepper>(nameof(IStepper.Maximum), (h, v, w) => Guarded(h, () => w.Maximum = v.Maximum))
				.MapGtk<MauiGtkStepper>(nameof(IStepper.Interval), (h, v, w) => w.Interval = v.Interval)
				.MapGtk<MauiGtkStepper>(nameof(IStepper.Value), (h, v, w) => Guarded(h, () => w.Value = v.Value))
				.Build();

		internal static readonly CommandMapper<IStepper, IStepperHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IStepper, IStepperHandler>(StepperHandler.CommandMapper).AddViewCommands().Build();
	}
}
