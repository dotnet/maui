using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 47971, "UWP doesn't display list items when binding a CommandParameter to BindingContext in a DataTemplate and including a CanExecute method", PlatformAffected.WinRT)]
	public class Bugzilla47971 : TestContentPage
	{
		protected override void Init()
		{
			var viewModel = new _47971ViewModel();

			var lv = new ListView { BindingContext = viewModel };

			lv.SetBinding(ListView.ItemsSourceProperty, new Binding("Models"));
			lv.SetBinding(ListView.SelectedItemProperty, new Binding("SelectedModel"));

			lv.ItemTemplate = new DataTemplate(() =>
			{
				var tc = new TextCell();

				tc.SetBinding(TextCell.TextProperty, new Binding("Name"));
				tc.SetBinding(TextCell.CommandParameterProperty, new Binding("."));
				tc.SetBinding(TextCell.CommandProperty, new Binding("BindingContext.ModelSelectedCommand", source: lv));

				return tc;
			});

			var layout = new StackLayout { Spacing = 10 };
			var instructions = new Label {Text = "The ListView below should display three items (Item1, Item2, and Item3). If it does not, this test has failed." };

			layout.Children.Add(instructions);
			layout.Children.Add(lv);

			Content = layout;
		}

		[Preserve(AllMembers = true)]
		internal class _47971ViewModel : INotifyPropertyChanged
		{
			_47971ItemModel _selectedModel;
			Command<_47971ItemModel> _modelSelectedCommand;
			ObservableCollection<_47971ItemModel> _models;

			public ObservableCollection<_47971ItemModel> Models
			{
				get { return _models; }
				set
				{
					_models = value;
					OnPropertyChanged();
				}
			}

			public _47971ItemModel SelectedModel
			{
				get { return _selectedModel; }
				set
				{
					_selectedModel = value;
					OnPropertyChanged();
				}
			}

			public Command<_47971ItemModel> ModelSelectedCommand => _modelSelectedCommand ??
				(_modelSelectedCommand = new Command<_47971ItemModel>(ModelSelectedCommandExecute, CanExecute));

			bool CanExecute(_47971ItemModel itemModel)
			{
				return true;
			}

			void ModelSelectedCommandExecute(_47971ItemModel model)
			{
				System.Diagnostics.Debug.WriteLine(model.Name);
			}

			public _47971ViewModel()
			{
				_models = new ObservableCollection<_47971ItemModel>(
					new List<_47971ItemModel>()
					{
						new _47971ItemModel() { Name = "Item 1"},
						new _47971ItemModel() { Name = "Item 2"},
						new _47971ItemModel() { Name = "Item 3"}
					});
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[Preserve(AllMembers = true)]
		internal class _47971ItemModel
		{
			public string Name { get; set; }
		}
	}
}