using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

[assembly: ExportRenderer(typeof(TestShell), typeof(TestShellRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	public class ShellTests : PlatformTestFixture
	{
		[OneTimeSetUp]
		public void ShellTestSetup()
		{
			Device.SetFlags(new[] { "Shell_UWP_Experimental" });
		}

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
