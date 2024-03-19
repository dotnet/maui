using System;
using Gtk;

namespace Microsoft.Maui.Handlers
{
	// https://docs.gtk.org/gtk3/class.ComboBox.html

	public partial class PickerHandler : ViewHandler<IPicker, ComboBox>
	{
		protected override ComboBox CreatePlatformView()
		{
			var model = new ListStore(typeof(string));
			var cell = new CellRendererText();

			var cb = new ComboBox(model);
			cb.PackStart(cell, false);
			cb.SetAttributes(cell, "text", 0);

			return cb;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			SetValues(this, VirtualView);
		}

		protected override void ConnectHandler(ComboBox nativeView)
		{
			base.ConnectHandler(nativeView);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.Changed += OnPlatformViewChanged;
		}

		void OnPlatformViewChanged(object? sender, EventArgs args)
		{
			if (_virtualViewSelectionMapping)
				return;
			if (sender is ComboBox platformView && VirtualView is { } virtualView)
			{
				virtualView.SelectedIndex = platformView.Active;
			}
		}

		protected override void DisconnectHandler(ComboBox platformView)
		{
			base.DisconnectHandler(platformView);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.Changed -= OnPlatformViewChanged;
		}

		public static void SetValues(PickerHandler handler, IPicker virtualView)
		{
			var platformView = handler.PlatformView;

			var list = new ItemDelegateList<string>(virtualView);

			if (platformView.Model is not ListStore model)
				return;

			handler._virtualViewSelectionMapping = true;
			model.Clear();

			foreach (var text in list)
			{
				model.AppendValues(text);
			}

			platformView.Active = virtualView.SelectedIndex;
			handler._virtualViewSelectionMapping = false;
		}

		internal static void MapItems(PickerHandler handler, IPicker picker)
		{
			SetValues(handler, picker);
		}

		internal bool _virtualViewSelectionMapping = false;

		public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
		{
			if (handler.PlatformView is { } platformView)
			{
				handler._virtualViewSelectionMapping = true;
				platformView.Active = picker.SelectedIndex;
				handler._virtualViewSelectionMapping = false;
			}
		}

		public static void MapReload(PickerHandler handler, IPicker picker, object? args)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = picker ?? throw new InvalidOperationException($"{nameof(picker)} should have been set by base class.");

			SetValues(handler, picker);
		}

		public static void MapFont(PickerHandler handler, IPicker view)
		{
			handler.MapFont(view);
		}

		public static void MapCharacterSpacing(PickerHandler handler, IPicker view)
		{
			handler.PlatformView.UpdateCharacterSpacing(view.CharacterSpacing);
		}

		[MissingMapper]
		public static void MapTitle(PickerHandler handler, IPicker view) { }

		public static void MapTextColor(PickerHandler handler, IPicker view)
		{
			handler.PlatformView.UpdateTextColor(view?.TextColor);
		}

		public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker view)
		{
			handler.PlatformView.UpdateHorizontalTextAlignment(view.HorizontalTextAlignment);
		}

		[MissingMapper]
		public static void MapTitleColor(PickerHandler handler, IPicker view) { }

		[MissingMapper]
		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker view) { }
	}
}