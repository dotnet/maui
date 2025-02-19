using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 31967, "Grid Layout on Bound RowDefinition")]
	public partial class Bugzilla31967 : ContentPage
	{
		public Bugzilla31967()
		{
			InitializeComponent();
			BindingContext = new Bugzilla31967Vm();
		}

		public class Bugzilla31967Vm : INotifyPropertyChanged
		{
			public Command Fire
			{
				get { return new Command(() => ToolbarHeight = 50); }
			}

			GridLength _toolbarHeight;
			public GridLength ToolbarHeight
			{
				get { return _toolbarHeight; }
				set
				{
					_toolbarHeight = value;
					OnPropertyChanged("ToolbarHeight");
				}
			}

			protected void OnPropertyChanged(string propertyName)
			{
				var handler = PropertyChanged;
				if (handler != null)
					handler(this, new PropertyChangedEventArgs(propertyName));
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}
	}
#endif
}
