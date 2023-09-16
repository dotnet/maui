using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 9735, "SwipeView broken in 4.5.0.356; creating huge number of overlapping SwipeItemViews on reveal",
		PlatformAffected.Android)]
	public partial class Issue9735 : TestContentPage
	{
		public Issue9735()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new Issue9735ViewModel();
		}
	}

	public class Issue9735Model
	{
		public string Text { get; set; }
	}

	public class Issue9735ViewModel : BindableObject
	{
		public List<Issue9735Model> Items { get; set; }

		public Issue9735ViewModel()
		{
			List<Issue9735Model> items = new List<Issue9735Model>
			{
				new Issue9735Model() { Text = "Item 1" },
				new Issue9735Model() { Text = "Item 2" },
				new Issue9735Model() { Text = "Item 3" },
				new Issue9735Model() { Text = "Item 4" },
				new Issue9735Model() { Text = "Item 5" },
				new Issue9735Model() { Text = "Item 6" }
			};

			Items = items;
		}
	}
}