using ElmSharp;
using ESize = ElmSharp.Size;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public delegate ESize? MeasureDelegate(NativeViewWrapperRenderer renderer, int availableWidth, int availableHeight);

#pragma warning disable CS0618 // Type or member is obsolete
	public class NativeViewWrapper : View
#pragma warning disable CS0618 // Type or member is obsolete
	{
		public NativeViewWrapper(EvasObject obj, MeasureDelegate measureDelegate = null)
		{
			EvasObject = obj;
			MeasureDelegate = measureDelegate;

			obj.TransferBindablePropertiesToWrapper(this);
		}

		public EvasObject EvasObject
		{
			get;
			private set;
		}

		public MeasureDelegate MeasureDelegate { get; }

		protected override void OnBindingContextChanged()
		{
			// TODO: we should provide a delegate to obtain children of a Container object,
			//       however currently there is no way to get the list of children
			EvasObject.SetBindingContext(BindingContext);
			base.OnBindingContextChanged();
		}
	}
}
