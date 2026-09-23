using System.Linq.Expressions;
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
			readonly NavigationPage _addressesPage = new(new AddressListView());
			readonly NavigationPage _associationsPage = new(new ContentPage());
			readonly FlyoutPage _rootPage;
			int _step;

			public Issue2004MainPage()
			{
				_rootPage = new FlyoutPage();
				_rootPage.Flyout = new ContentPage
				{
					Title = "M",
				};

				_rootPage.Detail = new NavigationPage(new SettingsView());
				PrepareNextStep();
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				Application.Current.MainPage = _rootPage;
			}

			void PrepareNextStep()
			{
				var page = ((NavigationPage)_rootPage.Detail).CurrentPage;
				page.ToolbarItems.Clear();
				var next = new ToolbarItem
				{
					Text = "Next step",
					AutomationId = $"NextStep{_step + 1}"
				};
				next.Clicked += async (_, _) => await AdvanceAsync();
				page.ToolbarItems.Add(next);
			}

			async Task AdvanceAsync()
			{
				switch (++_step)
				{
					case 1:
					case 3:
					case 7:
						_rootPage.Detail = _associationsPage;
						break;
					case 2:
					case 4:
					case 8:
						_rootPage.Detail = _addressesPage;
						break;
					case 5:
						await _addressesPage.PushAsync(new ContentPage());
						break;
					case 6:
						await _addressesPage.PopAsync();
						break;
					case 9:
						_rootPage.Detail = new ContentPage
						{
							Content = new Label { Text = "Success", AutomationId = "Success" }
						};
						return;
					default:
						throw new InvalidOperationException("The page-switching regression has already completed.");
				}
				PrepareNextStep();
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
								Text = "Use Next step to switch pages, push, and pop.",
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