//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6640, "[Android] Crash when app go background", PlatformAffected.Android)]
	public class Issue6640 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						AutomationId = "GoToShell",
						Text = "Click the button",
						Command = new Command(() =>
						{
							Application.Current.MainPage = new MasterPageShell();
						})
					}
				}
			};
		}

		[Preserve(AllMembers = true)]
		public class MasterPageShell : TestShell
		{
			protected override void Init()
			{
				FlyoutHeader = new FlyoutHeader();
				Items.Add(new FlyoutItem
				{
					Title = "Issue 6640",
					Items =
					{
						new ShellSection
						{
							Items =
							{
								new ShellContent
								{
									Content = new ContentPage
									{
										Content = new StackLayout
										{
											Children =
											{
												new Button
												{
													AutomationId = "Logout",
													Text = "Click the button",
													Command = new Command(() =>
													{
														Application.Current.MainPage = new ContentPage
														{
															Content = new StackLayout
															{
																BackgroundColor = Colors.White,
																Children =
																{
																	new Label
																	{
																		Text = $"Press Back Arrow to send application to background and wait few seconds.{Environment.NewLine}Application must not crash."
																	}
																}
															}
														};
													})
												}
											}
										}
									}
								}
							}
						}
					}
				});
			}
		}

		[Preserve(AllMembers = true)]
		public class FlyoutHeader : TestContentPage
		{
			protected override void Init()
			{
				Content = new Grid();
			}
		}
	}
}