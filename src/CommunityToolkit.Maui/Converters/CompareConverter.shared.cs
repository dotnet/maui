using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CommunityToolkit.Maui.Converters;

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
public abstract class CompareConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TValue, TReturnObject> : BaseConverterOneWay<TValue, object> where TValue : IComparable
{
	/// <inheritdoc/>
	public override object ConvertFrom(TValue value, CultureInfo culture)
	{
		if (value is null)
		{
			throw new ArgumentNullException(nameof(value));
		}

		var result = value.CompareTo(ComparingValue);

		return ComparisonOperator switch
		{
			OperatorType.Smaller => EvaluateCondition(result < 0, shouldReturnObjectResult),
			OperatorType.SmallerOrEqual => EvaluateCondition(result <= 0, shouldReturnObjectResult),
			OperatorType.Equal => EvaluateCondition(result is 0, shouldReturnObjectResult),
			OperatorType.NotEqual => EvaluateCondition(result is not 0, shouldReturnObjectResult),
			OperatorType.GreaterOrEqual => EvaluateCondition(result >= 0, shouldReturnObjectResult),
			OperatorType.Greater => EvaluateCondition(result > 0, shouldReturnObjectResult),
			_ => throw new NotSupportedException($"\"{ComparisonOperator}\" is not supported."),
		};
	}

	object EvaluateCondition(bool comparisonResult, bool shouldReturnObject) => (comparisonResult, shouldReturnObject) switch
	{
		(true, true) => TrueObject ?? throw new InvalidOperationException($"{nameof(TrueObject)} cannot be null"),
		(false, true) => FalseObject ?? throw new InvalidOperationException($"{nameof(FalseObject)} cannot be null"),
		(true, _) => true,
		_ => false
	};
}
