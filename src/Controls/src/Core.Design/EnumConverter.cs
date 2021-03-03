using System.ComponentModel;

namespace Microsoft.Maui.Controls.Core.Design
{
	/// <summary>
	/// Generic version of the <see cref="EnumConverter"/> for reuse.
	/// </summary>
	internal class EnumConverter<T> : EnumConverter
	{
		public EnumConverter() : base(typeof(T))
		{
		}
	}
}
