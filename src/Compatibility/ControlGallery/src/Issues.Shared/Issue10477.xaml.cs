﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 10477, "[Bug] CollectionView Header Controls with Commands Don't work when EmptyView is Visible", PlatformAffected.Android)]
	public partial class Issue10477 : TestContentPage
	{
		public Issue10477()
		{
#if APP
			InitializeComponent();
#endif
			ClickCommand = new Command(Click);
			BindingContext = this;
		}

		public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();
		public ICommand ClickCommand { get; }

		protected override void Init()
		{

		}

		void Click()
		{
			if (BackgroundColor == Colors.DarkOrange)
				BackgroundColor = Colors.Red;
			else
				BackgroundColor = Colors.DarkOrange;
		}
	}
}