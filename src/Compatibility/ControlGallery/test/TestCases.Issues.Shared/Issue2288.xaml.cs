using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2288, "ToolbarItem.Text change", PlatformAffected.iOS | PlatformAffected.Android)]
	public partial class Issue2288 : ContentPage
	{
		int _count = 0;
		public Issue2288()
		{
			InitializeComponent();
			MainText = "initial";
			MainTextCommand = new Command(o =>
				{
					MainText = "changed " + _count++;
				});

			BindingContext = this;
		}

		string _mainText;
		public string MainText
		{
			get { return _mainText; }
			set
			{
				_mainText = value;
				OnPropertyChanged();
			}
		}

		public Command MainTextCommand { get; set; }
	}
#endif
}
