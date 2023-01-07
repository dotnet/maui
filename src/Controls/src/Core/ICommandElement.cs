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

		// note to implementor: implement these properties explicitly
		void OnCanExecuteChanged(object? sender, EventArgs e);
		void SetCanExecute(bool canExecute);
	}
}
