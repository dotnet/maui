#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for property changing notifications.</summary>
	public class PropertyChangingEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="PropertyChangingEventArgs"/> with the specified property name.</summary>
		/// <param name="propertyName">The name of the property that is changing.</param>
		public PropertyChangingEventArgs(string propertyName)
		{
			PropertyName = propertyName;
		}

		/// <summary>Gets the name of the property that is changing.</summary>
		public virtual string PropertyName { get; }
	}
}