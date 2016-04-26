using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1898, "TabbedPage Page not watching icon changes", PlatformAffected.iOS, NavigationBehavior.PushAsync)]
	public partial class Issue1898 : TabbedPage
	{
		public Issue1898()
		{
			InitializeComponent();
			var vm = new Isseu1898Vm {
				ChangeTitleOneCommand = new Command (() => Issue1898PageOne.Title = "Changed 1"), 
				ChangeIconOneCommand = new Command (() => Issue1898PageOne.Icon = "coffee.png"), 
				ChangeIconOtherPageOneCommand = new Command (() => Issue1898PageTwo.Icon = "coffee.png"), 
				ChangeTitleTwoCommand = new Command (() => Issue1898PageTwo.Title = "Changed 2"), 
				ChangeIconTwoCommand = new Command (() => Issue1898PageTwo.Icon = "bank.png"),
				ChangeIconOtherPageTwoCommand = new Command (() => Issue1898PageOne.Icon = "calculator.png"), 
			};
			BindingContext = vm;
		}
	}

	public class Isseu1898Vm
	{
		public ICommand ChangeTitleOneCommand { get; set; }
		public ICommand ChangeIconOneCommand { get; set; }
		public ICommand ChangeIconOtherPageOneCommand { get; set; }
		
		public ICommand ChangeTitleTwoCommand { get; set; }
		public ICommand ChangeIconTwoCommand { get; set; }
		public ICommand ChangeIconOtherPageTwoCommand { get; set; }
	}
#endif
}
