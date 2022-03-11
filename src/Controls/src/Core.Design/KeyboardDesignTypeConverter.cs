namespace Microsoft.Maui.Controls.Design
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;

	public class KeyboardDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public KeyboardDesignTypeConverter()
		{
		}

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
				"Url"
			};
	}
}