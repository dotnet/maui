using Gdk;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Gtk;

#pragma warning disable CS0618 // Type or member is obsolete
public delegate Size? MeasureDelegate(NativeViewWrapperRenderer renderer, int availableWidth, int availableHeight);

public class NativeViewWrapper : View
#pragma warning disable CS0618 // Type or member is obsolete
{

	public NativeViewWrapper(global::Gtk.Widget obj, MeasureDelegate measureDelegate = null)
	{
		NativeView = obj;
		MeasureDelegate = measureDelegate;

		obj.TransferBindablePropertiesToWrapper(this);
	}

	public global::Gtk.Widget NativeView
	{
		get;
		private set;
	}

	public MeasureDelegate MeasureDelegate { get; }

	protected override void OnBindingContextChanged()
	{
		NativeView.SetBindingContext(BindingContext);
		base.OnBindingContextChanged();
	}

}