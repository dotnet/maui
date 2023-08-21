// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Windows.Input;
using Maui.Controls.Sample.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.ViewModels
{
	public class SearchBarViewModel : BaseViewModel
	{
		public ICommand SearchCommand => new Command<string>(ExecuteSearchCommand);

		void ExecuteSearchCommand(string searchCommandParameter)
		{
			Debug.WriteLine($"SearchCommand {searchCommandParameter}");
		}
	}
}