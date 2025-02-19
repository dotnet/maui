using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Issue(IssueTracker.Bugzilla, 39483, "ListView Context Menu localization", PlatformAffected.iOS)]
	public partial class Bugzilla39483 : ContentPage
	{
		public Bugzilla39483()
		{
#if APP

			InitializeComponent();

			BindingContext = new DemoViewModel();

#endif
		}
	}

	public class DemoViewModel : ViewModelBase
	{
		public DemoViewModel()
		{
			DataList = new List<string>();
			DataList.Add("Listenelement 1");
			DataList.Add("Listenelement 2");
			DataList.Add("Listenelement 3");
			DataList.Add("Listenelement 4");
		}

		List<string> _dataList;
		public List<string> DataList
		{
			get
			{
				return _dataList;
			}
			set
			{
				_dataList = value;
				OnPropertyChanged();
			}
		}
	}
}
