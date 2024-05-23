using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 9196, "[Bug] [iOS] CollectionView EmptyView causes the application to crash",
		PlatformAffected.iOS)]
	public partial class Issue9196 : TestContentPage
	{
		public Issue9196()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new _9196ViewModel();
		}
	}

	public class _9196ViewModel
	{
		public _9196ViewModel()
		{
			ReceiptsList = new List<string>();
		}

		public List<string> ReceiptsList { get; set; }
	}
}