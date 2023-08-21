// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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