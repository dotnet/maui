using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Xaml
{
	internal interface IProvideParentValues : IProvideValueTarget
	{
		IEnumerable<object> ParentObjects { get; }
	}
}