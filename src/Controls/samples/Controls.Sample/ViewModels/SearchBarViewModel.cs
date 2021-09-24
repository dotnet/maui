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