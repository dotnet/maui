using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkCheckButton = Gtk.CheckButton;
using GtkSwitch = Gtk.Switch;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="SwitchHandler"/> over a <c>Gtk.Switch</c>. The track colour (checked state) and thumb colour are CSS.
	/// </summary>
	public class GtkSwitchHandler : SwitchHandler
	{
		bool _updatingFromVirtualView;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkSwitchHandler()
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkSwitchHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new GtkSwitch { Halign = global::Gtk.Align.Start, Valign = global::Gtk.Align.Center };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			(platformView as GtkSwitch)?.AddNotification("active", OnActiveChanged);
		}

		void OnActiveChanged(object o, GLib.NotifyArgs args)
		{
			if (_updatingFromVirtualView || o is not GtkSwitch toggle || VirtualView is null)
				return;

			if (VirtualView.IsOn != toggle.Active)
				VirtualView.IsOn = toggle.Active;
		}

		internal static void ApplyMappers()
		{
			var mapper = Mapper;

			mapper.MapGtk<ISwitch, ISwitchHandler, GtkSwitch>(nameof(ISwitch.IsOn), (h, v, w) =>
			{
				if (h is not GtkSwitchHandler gtk)
				{
					w.Active = v.IsOn;
					return;
				}

				gtk._updatingFromVirtualView = true;
				try
				{
					w.Active = v.IsOn;
				}
				finally
				{
					gtk._updatingFromVirtualView = false;
				}
			});
			mapper.MapGtk<ISwitch, ISwitchHandler, GtkSwitch>(nameof(ISwitch.TrackColor), (h, v, w) =>
				GtkStyle.For(w).Set("background-color", v.TrackColor?.ToCss(), ":checked"));
			mapper.MapGtk<ISwitch, ISwitchHandler, GtkSwitch>(nameof(ISwitch.ThumbColor), (h, v, w) =>
				GtkStyle.For(w).Set("background-color", v.ThumbColor?.ToCss(), " slider"));
			// A switch does not stretch: its natural size is what MAUI should use.
			mapper.MapGtk<ISwitch, ISwitchHandler, GtkSwitch>(nameof(IView.MinimumWidth), (h, v, w) => { });
		}
	}

	/// <summary>
	/// <see cref="CheckBoxHandler"/> over a label-less <c>Gtk.CheckButton</c>. The foreground paints the checked box.
	/// </summary>
	public class GtkCheckBoxHandler : CheckBoxHandler
	{
		bool _updatingFromVirtualView;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkCheckBoxHandler()
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkCheckBoxHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new GtkCheckButton { Halign = global::Gtk.Align.Start, Valign = global::Gtk.Align.Center };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			if (platformView is GtkCheckButton check)
				check.Toggled += OnToggled;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is GtkCheckButton check)
				check.Toggled -= OnToggled;

			base.DisconnectHandler(platformView);
		}

		void OnToggled(object? sender, EventArgs e)
		{
			if (_updatingFromVirtualView || sender is not GtkCheckButton check || VirtualView is null)
				return;

			if (VirtualView.IsChecked != check.Active)
				VirtualView.IsChecked = check.Active;
		}

		internal static void ApplyMappers()
		{
			var mapper = Mapper;

			mapper.MapGtk<ICheckBox, ICheckBoxHandler, GtkCheckButton>(nameof(ICheckBox.IsChecked), (h, v, w) =>
			{
				if (h is not GtkCheckBoxHandler gtk)
				{
					w.Active = v.IsChecked;
					return;
				}

				gtk._updatingFromVirtualView = true;
				try
				{
					w.Active = v.IsChecked;
				}
				finally
				{
					gtk._updatingFromVirtualView = false;
				}
			});
			mapper.MapGtk<ICheckBox, ICheckBoxHandler, GtkCheckButton>(nameof(ICheckBox.Foreground), (h, v, w) =>
			{
				var style = GtkStyle.For(w);
				style.ClearPrefix("background", " check:checked");
				style.ClearPrefix("border-color", " check:checked");
				if (v.Foreground is SolidPaint { Color: { } color })
				{
					style.Set("background-color", color.ToCss(), " check:checked");
					style.Set("border-color", color.ToCss(), " check:checked");
				}
			});
			mapper.MapGtk<ICheckBox, ICheckBoxHandler, GtkCheckButton>(nameof(ICheckBox.Background), (h, v, w) => GtkStyle.For(w).SetBackground(v.Background));
		}
	}
}
