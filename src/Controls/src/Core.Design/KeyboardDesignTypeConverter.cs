namespace Microsoft.Maui.Controls.Design
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// Provides design-time type conversion for Keyboard values.
	/// </summary>
	public class KeyboardDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyboardDesignTypeConverter"/> class.
		/// </summary>
		public KeyboardDesignTypeConverter()
		{
		}

		/// <inheritdoc/>
		protected override bool ExclusiveToKnownValues => true;

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[]
			{
				"Plain",
				"Chat",
				"Default",
				"Email",
				"Numeric",
				"Telephone",
				"Text",
				"Url",
				"Password",
				"Date",
				"Time"
			};
	}
}