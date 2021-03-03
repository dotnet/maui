using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	interface ILineHeightElement
	{
		double LineHeight { get; }

		void OnLineHeightChanged(double oldValue, double newValue);
	}
}