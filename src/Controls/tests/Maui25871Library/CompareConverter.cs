using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

#nullable enable

namespace Maui25871Library;
/// <summary>
/// Converts an object that implements IComparable to an object or a boolean based on a comparison.
/// </summary>
[AcceptEmptyServiceProvider]
public sealed partial class CompareConverter : CompareConverter<IComparable, object>
{
}

/// <summary>
/// Converts an object that implements IComparable to an object or a boolean based on a comparison.
/// </summary>
public abstract class CompareConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TValue, TReturnObject> : IValueConverter where TValue : IComparable
{
	object? IValueConverter.Convert(object? value, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type targetType, object? parameter, CultureInfo culture) =>
		true;

	/// <inheritdoc />
	object? IValueConverter.ConvertBack(object? value, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type targetType, object? parameter, CultureInfo culture) =>
		false;

	public object ProvideValue(IServiceProvider serviceProvider) => this;


	/// <summary>
	/// The comparing value.
	/// </summary>
	public TValue? ComparingValue { get; set; }

	/// <summary>
	/// The comparison operator.
	/// </summary>
	public OperatorType ComparisonOperator { get; set; }

	/// <summary>
	/// The object that corresponds to True value.
	/// </summary>
	public TReturnObject? TrueObject { get; set; }

	/// <summary>
	/// The object that corresponds to False value.
	/// </summary>
	public TReturnObject? FalseObject { get; set; }

	/// <summary>
	/// Math operator type
	/// </summary>
	[Flags]
	public enum OperatorType
	{
		/// <summary>
		/// Not Equal Operator
		/// </summary>
		NotEqual = 0,

		/// <summary>
		/// Smaller Operator
		/// </summary>
		Smaller = 1 << 0,

		/// <summary>
		/// Smaller or Equal Operator
		/// </summary>
		SmallerOrEqual = 1 << 1,

		/// <summary>
		/// Equal Operator
		/// </summary>
		Equal = 1 << 2,

		/// <summary>
		/// Greater Operator
		/// </summary>
		Greater = 1 << 3,

		/// <summary>
		/// Greater or Equal Operator
		/// </summary>
		GreaterOrEqual = 1 << 4,
	}
}