// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;
using System.Windows.Input;

namespace Microsoft.Maui.Controls.Internals
{
	interface ICommandElement
	{
		// note to implementor: implement these properties publicly
		ICommand? Command { get; }
		object? CommandParameter { get; }

		// implement these explicitly
		void CanExecuteChanged(object? sender, EventArgs e);
	}
}
