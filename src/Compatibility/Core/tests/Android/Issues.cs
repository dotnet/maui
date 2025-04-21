using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Platform;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
{
	[TestFixture]
	public class Issues : PlatformTestFixture
	{
		[Test, Category("Entry")]
		[Description("The HorizontalAlignment of an Entry's renderer should match the Entry")]
		[Issue(IssueTracker.Github, 8137, "[Bug] XF 4.3 Entry HorizontalTextAlignment display wrong position")]
		public async Task EntryHorizontalAlignmentCenterInRenderer()
		{
			bool supportsRTL = IsRTLSupported;

			try
			{
				// Test with RTL support off 
				SetIsRTLSupported(false);

				await Application.Current.Dispatcher.DispatchAsync(() =>
				{
					var entry1 = new Entry { Text = "foo", HorizontalTextAlignment = TextAlignment.Center };
					using (var editText = GetNativeControl(entry1))
					{
						var centeredHorizontal =
						(editText.Gravity & GravityFlags.HorizontalGravityMask) == GravityFlags.CenterHorizontal;

						Assert.That(centeredHorizontal, Is.True);
					}

					// Now turn it back on and verify it works
					SetIsRTLSupported(true);

					var entry2 = new Entry { Text = "foo", HorizontalTextAlignment = TextAlignment.Center };
					using (var editText = GetNativeControl(entry2))
					{
						Assert.That(editText.TextAlignment, Is.EqualTo(global::Android.Views.TextAlignment.Center));
					}
				});
			}
			finally
			{
				// If something went wrong, make sure we leave the value as it was when we started
				SetIsRTLSupported(supportsRTL);
			}
		}

		[Test(Description = "No exceptions should be thrown")]
		[Issue(IssueTracker.Github, 9185, "[Bug] Java.Lang.IllegalArgumentException: 'order does not contain a valid category.'")]
		public async Task ToolbarItemWithReallyHighPriorityDoesntCrash()
		{
			try
			{
				ContentPage page = new ContentPage()
				{
					ToolbarItems =
					{
						new ToolbarItem() { Text = "2", Priority = int.MaxValue },
						new ToolbarItem() { Text = "1", Priority = int.MaxValue - 1 }
					}
				};

				var navPage = new NavigationPage(page);

				await Application.Current.Dispatcher.DispatchAsync(() => GetRenderer(navPage));
			}
			catch (Exception exc)
			{
				Assert.Fail($"{exc}");
			}
		}

		[Test(Description = "No ObjectDisposedException should be thrown")]
		[Issue(IssueTracker.Github, 5560, "[Android] Disposed EntryCell throws ObjectDisposed exception after updating an object that the EntryCell was previously bound to")]
		public async Task EntryCellDisposed()
		{
			var text1 = "Foo";
			var text2 = "Bar";
			var model = new _5560Model() { Text = text1 };
			for (int m = 0; m < 3; m++)
			{
				var entryCell = new EntryCell();
				entryCell.SetBinding(EntryCell.TextProperty, new Binding("Text"));
				entryCell.SetBinding(EntryCell.BindingContextProperty, new Binding("BindingContext"));
				entryCell.BindingContext = model;
				CellFactory.GetCell(entryCell, null, null, Context, null);

				if (m == 1)
				{
					GC.Collect();
				}

				model.Text = model.Text == text1 ? text2 : text1;
			}

			await model.WaitForTestToComplete().ConfigureAwait(false);
		}

		[Test(Description = "No ObjectDisposedException should be thrown")]
		[Issue(IssueTracker.Github, 8607, "Cannot access a disposed object. Object name: 'Android.Widget.TextView'.")]
		public async Task TextCellDisposed()
		{
			var text1 = "Foo";
			var text2 = "Bar";
			var model = new _5560Model() { Text = text1 };

			for (int m = 0; m < 3; m++)
			{
				var cell = new TextCell();
				cell.SetBinding(TextCell.TextProperty, new Binding("Text"));
				cell.BindingContext = model;
				CellFactory.GetCell(cell, null, null, Context, null);

				if (m == 1)
				{
					GC.Collect(2);
				}

				model.Text = model.Text == text1 ? text2 : text1;
			}

			await model.WaitForTestToComplete().ConfigureAwait(false);
		}
	}
}
