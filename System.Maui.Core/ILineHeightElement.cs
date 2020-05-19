using System.ComponentModel;

namespace System.Maui.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	interface ILineHeightElement
	{
		double LineHeight { get; }

		void OnLineHeightChanged(double oldValue, double newValue);
	}
}