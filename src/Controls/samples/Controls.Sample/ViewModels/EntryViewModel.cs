// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Windows.Input;
using Maui.Controls.Sample.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.ViewModels
{
	public class EntryViewModel : BaseViewModel
	{
		public ICommand ReturnCommand => new Command<string>(ExecuteReturnCommand);

		void ExecuteReturnCommand(string returnCommandParameter)
		{
			Debug.WriteLine($"ReturnCommand {returnCommandParameter}");
		}
	}
}