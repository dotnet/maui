namespace Microsoft.Maui.Controls.Design
{
	using System;
	using System.ComponentModel;
	using System.Linq;

	/// <summary>
	/// Base class for design-time type converters that provide a list of known values.
	/// </summary>
	public abstract class KnownValuesDesignTypeConverter : TypeConverter
	{
		/// <summary>
		/// Gets the array of known values for this type converter.
		/// </summary>
		protected abstract string[] KnownValues { get; }

		/// <summary>
		/// Gets a value indicating whether values are exclusive to the known values list.
		/// </summary>
		protected virtual bool ExclusiveToKnownValues { get; } = false;

		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

		/// <inheritdoc/>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			=> new(KnownValues);

		/// <inheritdoc/>
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> ExclusiveToKnownValues;

		/// <inheritdoc/>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			=> true;

		/// <inheritdoc/>
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			if (!ExclusiveToKnownValues)
				return true;

			return KnownValues.Any(v => value?.ToString()?.Equals(v, StringComparison.Ordinal) ?? false);
		}
	}
}
