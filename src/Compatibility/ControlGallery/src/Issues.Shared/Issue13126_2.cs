using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using static Microsoft.Maui.Controls.ControlGallery.Issues.Issue13126;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 13126, "[Bug] Regression: 5.0.0-pre5 often fails to draw dynamically loaded collection view content",
		PlatformAffected.iOS, issueTestNumber: 1)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
#endif
	public class Issue13126_2 : TestContentPage
	{
		_13126VM _vm;
		const string Success = "Success";

		protected override void Init()
		{
			var collectionView = BindingWithConverter();

			var grid = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition() { Height = GridLength.Star },
				}
			};

			grid.Children.Add(collectionView);

			Content = grid;

			_vm = new _13126VM();
			BindingContext = _vm;
		}

		protected async override void OnParentSet()
		{
			base.OnParentSet();
			_vm.IsBusy = true;

			await Task.Delay(1000);

			using (_vm.Data.BeginMassUpdate())
			{
				_vm.Data.Add(Success);
			}

			_vm.IsBusy = false;
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void CollectionViewShouldSourceShouldResetWhileInvisible()
		{
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
