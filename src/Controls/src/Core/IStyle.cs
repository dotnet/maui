using System;

namespace Microsoft.Maui.Controls
{
	interface IStyle
	{
		/// <summary>
		/// Gets the target type for this style. May return null for lazy styles if the type was trimmed.
		/// </summary>
		Type? TargetType { get; }

		void Apply(BindableObject bindable, SetterSpecificity specificity);
		void UnApply(BindableObject bindable);
	}
}