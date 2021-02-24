using System;

namespace Microsoft.Maui.Controls
{
	public abstract class TypeConverter
	{
		public virtual bool CanConvertFrom(Type sourceType)
		{
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));

			return sourceType == typeof(string);
		}

		public virtual object ConvertFromInvariantString(string value)
		{
			return null;
		}

		public virtual string ConvertToInvariantString(object value) => throw new NotSupportedException();
	}
}