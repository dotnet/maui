#nullable disable
using System;
using System.ComponentModel;
using System.Xml;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface INameScope
	{
		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		object FindByName(string name);

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		void RegisterName(string name, object scopedElement);

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		void UnregisterName(string name);
	}
}