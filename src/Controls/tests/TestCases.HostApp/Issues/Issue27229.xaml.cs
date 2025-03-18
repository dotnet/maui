using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 27229, "CollectionView, EmptyView Fills Available Space By Default")]
	public partial class Issue27229
	{
		public Issue27229()
		{
			InitializeComponent();
		}
	}
}
