#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Class that represents a list of property and binding conditions, and a list of setters that are applied when all of the conditions in the list are met.
/// </summary>
/// <remarks>
/// <para>
/// Developers can use a <see cref="MultiTrigger" /> to compare against property values on the control that contains it by using <see cref="Trigger" /> objects, or on any bound property (including those on the enclosing control) by using <see cref="BindingCondition" /> objects. These can be mixed in the same <see cref="Conditions" /> list.
/// </para>
/// <seealso cref="PropertyCondition" />
/// <seealso cref="BindingCondition" />
/// </remarks>
[ContentProperty("Setters")]
public sealed class MultiTrigger : TriggerBase
{
	/// <summary>
	/// Initializes a new <see cref="MultiTrigger" /> instance.
	/// </summary>
	/// <param name="targetType">The type of the trigger target.</param>
	public MultiTrigger([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType) : base(new MultiCondition(), targetType)
	{
	}

	/// <summary>
	/// Gets the list of conditions that must be satisfied in order for the setters in the <see cref="Setters" /> list to be invoked.
	/// </summary>
	public IList<Condition> Conditions
	{
		get { return ((MultiCondition)Condition).Conditions; }
	}

	/// <summary>
	/// Gets the list of <see cref="Setter" /> objects that will be applied when the list of conditions in the <see cref="Conditions" /> property are all met.
	/// </summary>
	public new IList<Setter> Setters
	{
		get { return base.Setters; }
	}
}