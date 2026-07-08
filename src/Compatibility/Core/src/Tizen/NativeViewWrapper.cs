using Microsoft.Maui.Graphics;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
#pragma warning disable CS0618 // Type or member is obsolete
	public delegate Size? MeasureDelegate(NativeViewWrapperRenderer renderer, int availableWidth, int availableHeight);

	public class NativeViewWrapper : View
#pragma warning disable CS0618 // Type or member is obsolete
	{
		public NativeViewWrapper(NView obj, MeasureDelegate measureDelegate = null)
		{
			NativeView = obj;
			MeasureDelegate = measureDelegate;

			obj.TransferBindablePropertiesToWrapper(this);
		}

		public NView NativeView
		{
			get;
			private set;
		}

		public MeasureDelegate MeasureDelegate { get; }

		protected override void OnBindingContextChanged()
		{
			// TODO: we should provide a delegate to obtain children of a Container object,
			//       however currently there is no way to get the list of children
			NativeView.SetBindingContext(BindingContext);
			base.OnBindingContextChanged();
		}
	}
}
