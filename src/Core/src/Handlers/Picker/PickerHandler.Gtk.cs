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

			SetValues(PlatformView, VirtualView);
		}

		protected override void ConnectHandler(ComboBox nativeView)
		{
			base.ConnectHandler(nativeView);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.Changed += OnNativeViewChanged;
		}

		void OnNativeViewChanged(object? sender, EventArgs args)
		{
			if (sender is ComboBox nativeView && VirtualView is { } virtualView)
			{
				virtualView.SelectedIndex = nativeView.Active;
			}
		}

		protected override void DisconnectHandler(ComboBox nativeView)
		{
			base.DisconnectHandler(nativeView);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.Changed -= OnNativeViewChanged;
		}

		public static void SetValues(ComboBox nativeView, IPicker virtualView)
		{
			var list = new ItemDelegateList<string>(virtualView);

			if (nativeView.Model is not ListStore model)
				return;

			model.Clear();

			foreach (var text in list)
			{
				model.AppendValues(text);
			}

			nativeView.Active = virtualView.SelectedIndex;
		}

		internal static void MapItems(IPickerHandler handler, IPicker picker)
		{
			SetValues(handler.PlatformView,picker);
		}

		public static void MapSelectedIndex(PickerHandler handler, IPicker view)
		{
			if (handler.PlatformView is { } nativeView)
			{
				nativeView.Active = view.SelectedIndex;
			}
		}

		public static void MapReload(PickerHandler handler, IPicker picker, object? args)
		{
			var nativeView = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = picker ?? throw new InvalidOperationException($"{nameof(picker)} should have been set by base class.");

			SetValues(nativeView, picker);

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