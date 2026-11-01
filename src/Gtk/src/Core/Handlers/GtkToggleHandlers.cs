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
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkSwitchHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
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

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			(platformView as GtkSwitch)?.RemoveNotification("active", OnActiveChanged);
			base.DisconnectHandler(platformView);
		}

		void OnActiveChanged(object o, GLib.NotifyArgs args)
		{
			if (_updatingFromVirtualView || o is not GtkSwitch toggle || ((IElementHandler)this).VirtualView is not ISwitch view)
				return;

			if (view.IsOn != toggle.Active)
				view.IsOn = toggle.Active;
		}

		internal static readonly IPropertyMapper<ISwitch, ISwitchHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<ISwitch, ISwitchHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<ISwitch, ISwitchHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<ISwitch, ISwitchHandler>(SwitchHandler.Mapper).AddViewMappings();
			Configure(mapper);
			return mapper.Build();
		}

		static CommandMapper<ISwitch, ISwitchHandler> CreateCommandMapper() =>
			new GtkCommandMapperBuilder<ISwitch, ISwitchHandler>(SwitchHandler.CommandMapper).AddViewCommands().Build();

		static void Configure(GtkMapperBuilder<ISwitch, ISwitchHandler> mapper)
		{
			mapper.MapGtk<GtkSwitch>(nameof(ISwitch.IsOn), (h, v, w) =>
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
			mapper.MapGtk<GtkSwitch>(nameof(ISwitch.TrackColor), (h, v, w) =>
				GtkStyle.For(w).Set("background-color", v.TrackColor?.ToCss(), ":checked"));
			mapper.MapGtk<GtkSwitch>(nameof(ISwitch.ThumbColor), (h, v, w) =>
				GtkStyle.For(w).Set("background-color", v.ThumbColor?.ToCss(), " slider"));
			// A switch does not stretch: its natural size is what MAUI should use.
			mapper.MapGtk<GtkSwitch>(nameof(IView.MinimumWidth), (h, v, w) => { });
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
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkCheckBoxHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
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
			if (_updatingFromVirtualView || sender is not GtkCheckButton check || ((IElementHandler)this).VirtualView is not ICheckBox view)
				return;

			if (view.IsChecked != check.Active)
				view.IsChecked = check.Active;
		}

		internal static readonly IPropertyMapper<ICheckBox, ICheckBoxHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<ICheckBox, CheckBoxHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<ICheckBox, ICheckBoxHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<ICheckBox, ICheckBoxHandler>(CheckBoxHandler.Mapper).AddViewMappings();
			Configure(mapper);
			return mapper.Build();
		}

		static CommandMapper<ICheckBox, CheckBoxHandler> CreateCommandMapper() =>
			new GtkCommandMapperBuilder<ICheckBox, CheckBoxHandler>(CheckBoxHandler.CommandMapper).AddViewCommands().Build();

		static void Configure(GtkMapperBuilder<ICheckBox, ICheckBoxHandler> mapper)
		{
			mapper.MapGtk<GtkCheckButton>(nameof(ICheckBox.IsChecked), (h, v, w) =>
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
			mapper.MapGtk<GtkCheckButton>(nameof(ICheckBox.Foreground), (h, v, w) =>
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
			mapper.MapGtk<GtkCheckButton>(nameof(ICheckBox.Background), (h, v, w) => GtkBackgroundImage.Update(h, w, v.Background));
		}
	}
}
