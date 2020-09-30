using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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
																BackgroundColor = Color.White,
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