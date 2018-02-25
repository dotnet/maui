using Caboodle.Samples.Model;
using Caboodle.Samples.View;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caboodle.Samples.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
		public ObservableRangeCollection<SampleItem> Items { get; }
		public HomeViewModel()
		{
			Items = new ObservableRangeCollection<SampleItem>()
			{
				new SampleItem
				{
					Name = "Preferences",
					Page = typeof(PreferencesPage)
				}
			};
		}
    }
}
