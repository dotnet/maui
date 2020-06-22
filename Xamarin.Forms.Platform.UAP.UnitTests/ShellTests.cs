using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UAP.UnitTests;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(TestShell), typeof(TestShellRenderer))]
namespace Xamarin.Forms.Platform.UAP.UnitTests
{
	public class ShellTests : PlatformTestFixture
	{
		[Test, Category("Shell")]
		[Description("Shell doesn't crash when Flyout Behavior Initialized to Locked")]
		public async Task FlyoutHeaderReactsToChanges()
		{
			var shell = CreateShell();
			shell.FlyoutBehavior = FlyoutBehavior.Locked;

			try
			{
				await Device.InvokeOnMainThreadAsync(() =>
				{
					var r = GetRenderer(shell);
				});
			}
			catch (Exception exc)			
			{
				Assert.Fail(exc.ToString());
			}

			Assert.Pass();
		}

		Shell CreateShell()
		{
			return new Shell()
			{
				Items =
				{
					new FlyoutItem()
					{
						Items =
						{
							new Tab()
							{
								Items =
								{
									new ShellContent()
									{
										Content = new ContentPage()
									}
								}
							}
						}
					}
				}
			};
		}
	}

	public class TestShell : Shell { }	
	
	public class TestShellRenderer : ShellRenderer
	{
	}
}
