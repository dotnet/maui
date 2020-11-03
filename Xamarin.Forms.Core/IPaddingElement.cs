using System.ComponentModel;

namespace Xamarin.Forms
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IPaddingElement
	{
		//note to implementor: implement this property publicly
		Thickness Padding { get; }

		//note to implementor: but implement this method explicitly
		void OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue);
		Thickness PaddingDefaultValueCreator();
	}
}