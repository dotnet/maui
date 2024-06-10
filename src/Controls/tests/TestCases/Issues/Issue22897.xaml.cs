using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22897, "Display Bug On iOS about BoxView that under initial InVisible Layout", PlatformAffected.iOS)]
	public partial class Issue22897 : ContentPage
	{
		int _selectIndex;
		string _groupName;
		bool _showOptions;

		public Issue22897()
		{
			InitializeComponent();

			SelectIndex = 0;
			GroupName = "TestGroup";
			ShowOptions = false;
			Options = GetOptions(3);

			BindingContext = this;
		}

		public List<Issue22897Option> Options { get; set; }

		public int SelectIndex
		{
			get { return _selectIndex; }
			set
			{
				_selectIndex = value;
				ShowOptions = _selectIndex == 1;
				OnPropertyChanged();
			}
		}

		public string GroupName
		{
			get { return _groupName; }
			set
			{
				_groupName = value;
				OnPropertyChanged();
			}
		}

		public bool ShowOptions
		{
			get { return _showOptions; }
			set
			{
				_showOptions = value;
				OnPropertyChanged();
			}
		}

		List<Issue22897Option> GetOptions(int count)
		{
			var result = new List<Issue22897Option>();

			for (int i = 1; i <= count; i++)
			{
				result.Add(new Issue22897Option { Name = $"A{i}" });
			}

			return result;
		}
	}

	public class Issue22897Option : BindableObject
	{
		string _name;
		bool _isChecked;

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				OnPropertyChanged();
			}
		}

		public bool IsChecked
		{
			get => _isChecked;
			set
			{
				_isChecked = value;
				OnPropertyChanged();
			}
		}
	}
}