using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IAccelerator
	{
		/// <summary>
		/// Specifies the virtual key used to modify another keypress. 
		/// </summary>
		public IEnumerable<string> Modifiers { get; }

		/// <summary>
		/// Specifies the values for the virtual key.
		/// </summary>
		public string Key { get; }
	}
}
