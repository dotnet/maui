#nullable disable
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Xaml
{
	// Used by MAUI XAML Hot Reload.
	// Consult XET if updating!
	internal interface IProvideParentValues : IProvideValueTarget
	{
		IEnumerable<object> ParentObjects { get; }
	}
}