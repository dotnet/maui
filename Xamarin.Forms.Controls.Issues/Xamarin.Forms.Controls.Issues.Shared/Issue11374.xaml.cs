using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11374,
		"[Bug] [Android] SwipeView in ListView is not working with RippleEffect and Release configuration",
		PlatformAffected.Android)]
	public partial class Issue11374 : TestContentPage
	{
		public Issue11374()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new Issue11374ViewModel();
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue11374ViewModel : BindableObject
	{
		public ObservableCollection<string> Items { get; set; }

		public Command LoadItemsCommand { get; set; }

		public Issue11374ViewModel()
		{
			LoadItems();
		}

		void LoadItems()
		{
			Items = new ObservableCollection<string>
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5"
			};
		}
	}

	[Preserve(AllMembers = true)]
	public class RippleEffect : RoutingEffect
	{
		public RippleEffect() : base($"{Effects.ResolutionGroupName}.{nameof(RippleEffect)}")
		{

		}
	}
}