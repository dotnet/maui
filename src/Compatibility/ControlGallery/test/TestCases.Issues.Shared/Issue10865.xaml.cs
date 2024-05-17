using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10865, "[Bug] CarouselView Throws when removing an item", PlatformAffected.Android)]
	public partial class Issue10865 : TestContentPage
	{
		public Issue10865()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class Issue10865ViewModel
	{
		public Issue10865ViewModel()
		{
			AddCommand = new Command(() =>
			{
				Items.Add($"Items {Items.Count}");
				((Command)RemoveLastCommand).ChangeCanExecute();
			});
			RemoveLastCommand = new Command(() =>
			{
				Items.Remove(Items.Last());
				((Command)RemoveLastCommand).ChangeCanExecute();

				// Workaround
				//if(!(Items.Any()))
				//{
				//    Items = new ObservableCollection<string>();
				//}

			}, () => Items.Any());
		}

		public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>();

		public ICommand AddCommand { get; }
		public ICommand RemoveLastCommand { get; }
	}
}