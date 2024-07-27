using System;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics;

internal static class BindablePropertyDiagnostics
{
	public static ValueSource GetValueSource(BindableObject bindable, BindableProperty property)
	{
		if (bindable == null)
			throw new ArgumentNullException(nameof(bindable));
		if (property == null)
			throw new ArgumentNullException(nameof(property));

		var context = bindable.GetContext(property);
		if (context == null)
			return new ValueSource(BaseValueSource.Unknown);

		var specificity = context.Values.GetSpecificityAndValue().Key;
		if (specificity == SetterSpecificity.DefaultValue)
			return new ValueSource(BaseValueSource.Default);
		if (specificity == SetterSpecificity.FromBinding)
			return new ValueSource(BaseValueSource.Unknown, isExpression: true);
		if (specificity == SetterSpecificity.ManualValueSetter)
			return new ValueSource(BaseValueSource.Local);
		if (specificity == SetterSpecificity.DynamicResourceSetter)
			return new ValueSource(BaseValueSource.Unknown, isExpression: true);
		if (specificity == SetterSpecificity.VisualStateSetter)
			return new ValueSource(BaseValueSource.Style);
		if (specificity == SetterSpecificity.Trigger)
			return new ValueSource(BaseValueSource.StyleTrigger);
		if (specificity == SetterSpecificity.FromHandler)
			return new ValueSource(BaseValueSource.Unknown, isCurrent: true);

		if (specificity.Vsm > 0)
			return new ValueSource(BaseValueSource.Local);
		if (specificity.Style > 0)
			return new ValueSource(BaseValueSource.Style);
		if (specificity.Id > 0 || specificity.Class > 0 || specificity.Type > 0)
			return new ValueSource(BaseValueSource.Style);
		if (specificity.Manual > 0)
			return new ValueSource(BaseValueSource.Local);
		if (specificity.DynamicResource > 0)
			return new ValueSource(BaseValueSource.Unknown, isExpression: true);
		if (specificity.Binding > 0)
			return new ValueSource(BaseValueSource.Unknown, isExpression: true);

		return new ValueSource(BaseValueSource.Unknown);
	}
}

internal struct ValueSource
{
	public ValueSource(BaseValueSource baseValueSource, bool isCoerced = false, bool isCurrent = false, bool isExpression = false)
	{
		BaseValueSource = baseValueSource;
		IsCoerced = isCoerced;
		IsCurrent = isCurrent;
		IsExpression = isExpression;
	}

	public BaseValueSource BaseValueSource { get; }
	public bool IsCoerced { get; }
	public bool IsCurrent { get; } //FromHandler
	public bool IsExpression { get; }   //Binding, DynamicResource, etc
	public BaseValueSource Unknown { get; }
}

internal enum BaseValueSource
{
	Unknown = 0, //source is not known
	Default = 1, //source is default value, as defined by BindableProperty.DefaultValue or created by BindableProperty.DefaultValueCreator
	Inherited = 2, //source is a value through property value inheritance (not really supported in Maui)
	DefaultStyle = 3,
	DefaultStyleTrigger = 4,
	Style = 5,
	TemplateTrigger = 6,
	StyleTrigger = 7,
	ImplicitStyleReference = 8,
	ParentTemplate = 9,
	ParentTemplateTrigger = 10,
	Local = 11,

}