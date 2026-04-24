#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>The secret key to a BindableProperty, used to implement read-only bindable properties.</summary>
	public sealed class BindablePropertyKey
	{
		internal BindablePropertyKey(BindableProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			BindableProperty = property;
		}

		/// <summary>Gets the BindableProperty associated with this key.</summary>
		public BindableProperty BindableProperty { get; private set; }
	}
}