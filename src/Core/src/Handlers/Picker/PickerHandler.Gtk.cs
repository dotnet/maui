using System;
using Gtk;

namespace Microsoft.Maui.Handlers
{

	// https://developer.gnome.org/gtk3/stable/GtkComboBox.html

	public partial class PickerHandler : ViewHandler<IPicker, ComboBox>
	{

		protected override ComboBox CreateNativeView()
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

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			SetValues(NativeView, VirtualView);
		}

		protected override void ConnectHandler(ComboBox nativeView)
		{
			base.ConnectHandler(nativeView);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");

			NativeView.Changed += OnNativeViewChanged;
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

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");

			NativeView.Changed -= OnNativeViewChanged;
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

		public static void MapSelectedIndex(PickerHandler handler, IPicker view)
		{
			if (handler.NativeView is { } nativeView)
			{
				nativeView.Active = view.SelectedIndex;
			}
		}

		public static void MapReload(PickerHandler handler, IPicker picker, object? args)
		{
			var nativeView = handler.NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = picker ?? throw new InvalidOperationException($"{nameof(picker)} should have been set by base class.");

			SetValues(nativeView, picker);

		}

		public static void MapFont(PickerHandler handler, IPicker view)
		{
			handler.MapFont(view);

		}

		public static void MapCharacterSpacing(PickerHandler handler, IPicker view)
		{
			handler.NativeView.UpdateCharacterSpacing(view.CharacterSpacing);
		}

		[MissingMapper]
		public static void MapTitle(PickerHandler handler, IPicker view) { }

		public static void MapTextColor(PickerHandler handler, IPicker view)
		{
			handler.NativeView.UpdateTextColor(view?.TextColor);
		}

		public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker view)
		{
			handler.NativeView.UpdateHorizontalTextAlignment(view.HorizontalTextAlignment);
		}

		[MissingMapper]
		public static void MapTitleColor(PickerHandler handler, IPicker view) { }

	}

}