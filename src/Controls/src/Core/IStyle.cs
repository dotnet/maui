using System;

namespace Microsoft.Maui.Controls
{
	interface IStyle
	{
		Type TargetType { get; }

		void Apply(BindableObject bindable);
		void UnApply(BindableObject bindable);
	}
}