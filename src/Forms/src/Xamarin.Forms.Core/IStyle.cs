using System;

namespace Xamarin.Forms
{
	interface IStyle
	{
		Type TargetType { get; }

		void Apply(BindableObject bindable);
		void UnApply(BindableObject bindable);
	}
}