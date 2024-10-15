﻿using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2004, "[Android] Xamarin caused by: android.runtime.JavaProxyThrowable: System.ObjectDisposedException: Cannot access a disposed object",
		PlatformAffected.Android)]
	public class Issue2004 : NavigationPage
	{
		public Issue2004() : base(new Issue2004MainPage())
		{
		}

		public class Issue2004MainPage : ContentPage
		{
			static internal NavigationPage settingsPage = new NavigationPage(new SettingsView());
			static internal NavigationPage addressesPage = new NavigationPage(new AddressListView());
			static internal NavigationPage associationsPage = new NavigationPage(new ContentPage());

			static FlyoutPage RootPage;

			public Issue2004MainPage()
			{
				FlyoutPage testPage = new FlyoutPage();
				RootPage = testPage;
				testPage.Flyout = new ContentPage
				{
					Title = "M",
				};

				testPage.Detail = new SettingsView();
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				Application.Current.MainPage = RootPage;
			}

			static void SetPage(Page page)
			{
				RootPage.Detail = page;
			}

			static async Task UI(int delay)
			{
				await Task.Delay(delay);
			}

			public static INavigation NavigationPage => RootPage.Detail.Navigation;

			public static async Task DisposedBitmapTest()
			{
				SetPage(Issue2004MainPage.associationsPage);
				await UI(999);
				SetPage(Issue2004MainPage.addressesPage);
				await UI(999);

				SetPage(Issue2004MainPage.associationsPage);

				await UI(999);
				SetPage(Issue2004MainPage.addressesPage);
				await UI(999);

				await NavigationPage.PushAsync(new ContentPage());
				await UI(999);
				await NavigationPage.PopAsync();
				await UI(999);

				SetPage(Issue2004MainPage.associationsPage);
				await UI(999);

				SetPage(Issue2004MainPage.addressesPage);
				await UI(999);
				SetPage(new ContentPage() { Content = new Label() { Text = "Success" } });
			}



			public class AddressListItemView : Grid
			{
				public AddressListItemView()
				{
					this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
					this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
					this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
					this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
					this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

					this.Add(new Button() { Text = "qwe", BackgroundColor = Colors.Transparent }, 0, 0);
					this.Add(new Button() { Text = "qwe", BackgroundColor = Colors.Transparent }, 1, 0);
					this.Add(new Button() { Text = "qwe", BackgroundColor = Colors.Transparent }, 2, 0);
					this.Add(new Button() { Text = "qwe", BackgroundColor = Colors.Transparent }, 3, 0);

					this.Add(new StackLayout()
					{
						Children =
					{
						new Label{ Text = "Address", LineBreakMode = LineBreakMode.TailTruncation},
						new Label{ Text = "Owner", LineBreakMode = LineBreakMode.TailTruncation},
						new Label{ Text = "ViolationCount"},
					}

					}, 4, 0);
				}
			}


			public class AddressListView : ContentPage
			{
				public AddressListView()
				{
					ListView listView = new ListView() { RowHeight = 75 };

					listView.SetBinding(ListView.ItemsSourceProperty, "UnitList");

					listView.ItemTemplate = new DataTemplate(() =>
					{
						ViewCell cell = new ViewCell();
						cell.View = new AddressListItemView();
						return cell;
					});

					Content = new StackLayout()
					{

						Children =
					{
						new StackLayout()
						{
							Orientation = StackOrientation.Horizontal,
							Padding = 4,
							Children =
							{
								new StackLayout()
								{
									Children =
									{
										new Label()
										{
											Text = "SortText",
											HorizontalOptions = LayoutOptions.Center
										}
									}
								}
							}
						},
						listView
					}
					};

					BindingContext = this;
				}

				protected override void OnAppearing()
				{
					base.OnAppearing();
					UnitList = null;
					NotifyPropertyChanged(() => UnitList);
					SelectedAddress = null;
					LoadAddresses();
				}

				string _selectedAddress;
				public string SelectedAddress
				{
					get => _selectedAddress;
					set
					{
						if (_selectedAddress != value)
						{
							_selectedAddress = value;
							NotifyPropertyChanged(() => SelectedAddress);
							if (SelectedAddress != null)
							{
								LoadUnitsByAddress(_selectedAddress);
								NotifyPropertyChanged(() => UnitList);
							}
						}
					}
				}

				List<string> _streeAddresses;
				private List<string> _unitList;

				public List<string> StreetAddresses
				{
					get { return _streeAddresses; }
					set
					{
						_streeAddresses = value;
						NotifyPropertyChanged();
					}
				}

				public void LoadAddresses()
				{
					StreetAddresses = Enumerable.Range(1, 10).Select(x => x.ToString()).ToList();
					SelectedAddress = StreetAddresses.First();
				}

				public void LoadUnitsByAddress(string address)
				{
					if (string.IsNullOrEmpty(address))
					{
						UnitList?.Clear();
						return;
					}
					UnitList = Enumerable.Range(1, 10).Select(x => x.ToString()).ToList();
				}

				public List<string> UnitList
				{
					get { return _unitList; }
					set { _unitList = value; }
				}

				public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
				{
					OnPropertyChanged(propertyName);
				}

				protected virtual void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpression)
				{
					string propertyName = GetPropertyName(propertyExpression);
					OnPropertyChanged(propertyName);
				}

				private string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
				{
					if (propertyExpression == null)
					{
						throw new ArgumentNullException("propertyExpression");
					}

					if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
					{
						throw new ArgumentException("Should be a member access lambda expression", "propertyExpression");
					}

					var memberExpression = (MemberExpression)propertyExpression.Body;
					return memberExpression.Member.Name;
				}
			}



			public class SettingsView : ContentPage
			{
				public Command AutoTest => new Command(async () =>
				{
					await Issue2004MainPage.DisposedBitmapTest();
				});


				protected async override void OnAppearing()
				{
					base.OnAppearing();
					await Task.Delay(1000);
					AutoTest.Execute(null);

				}
				public SettingsView()
				{
					BindingContext = this;
					Content = new ScrollView()
					{
						Content = new StackLayout()
						{
							Children =
						{
							new Label()
							{
								Text = "Auto Test",
								HorizontalOptions = LayoutOptions.Start
							}
						}
						}
					};
				}
			}
		}
	}
}