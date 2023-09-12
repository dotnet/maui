#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	interface IStyle
	{
		Type TargetType { get; }

		void Apply(BindableObject bindable, SetterSpecificity specificity);
		void UnApply(BindableObject bindable);
	}
}