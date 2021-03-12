using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2634, "Keyboard causes view to scroll incorrectly", PlatformAffected.iOS)]
	public class Issue2634 : ContentPage
	{
		public Issue2634()
		{
			Content = new AddPatientView();
		}

		public class AddPatientView : ContentView
		{
			Entry _firstNameEntry;

			public AddPatientView()
			{
				var bvBackground = new Frame
				{
					Content = new Label { Text = "" },
					BorderColor = Color.FromRgb(0x06, 0x68, 0xCF),
					BackgroundColor = Color.FromRgba(0f, 0f, 0f, 0.4f),
					HasShadow = true
				};

				var addGrid = new Grid
				{
					VerticalOptions = LayoutOptions.FillAndExpand,
					RowDefinitions = {
						new RowDefinition { Height = new GridLength (10, GridUnitType.Absolute) },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
						new RowDefinition { Height = new GridLength (10, GridUnitType.Absolute) },
					},
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength (10, GridUnitType.Absolute) },
						new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
						new ColumnDefinition { Width = new GridLength (10, GridUnitType.Absolute) },
					},
					ColumnSpacing = 1,
					RowSpacing = 1,
					Padding = 0
				};


				#region QuickAdd Data Entry
				var gridAddData = new Grid
				{
					RowDefinitions = {
						new RowDefinition { Height = new GridLength (0, GridUnitType.Absolute) },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute) },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute) },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute) },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute) },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = new GridLength (20, GridUnitType.Absolute) },
					},
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength (60, GridUnitType.Absolute)  },
						new ColumnDefinition { Width = GridLength.Auto },
						new ColumnDefinition { Width = new GridLength (10, GridUnitType.Absolute)  },
						new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star)  },
						new ColumnDefinition { Width = new GridLength (60, GridUnitType.Absolute)  }
					},
					BackgroundColor = Color.Transparent,
					ColumnSpacing = 1,
					RowSpacing = 1,
					Padding = 0
				};
				Color textColor = Color.Blue;
				Color dataColor = Color.Black;

				var slFirstName = new StackLayout { Orientation = StackOrientation.Vertical };
				var lblFirstNameLabel = new Label
				{
					Text = "First Name",
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					FontAttributes = FontAttributes.Bold,
					TextColor = textColor
				};
				_firstNameEntry = new Entry
				{
					Keyboard = Keyboard.Default,
					TextColor = dataColor,
					Placeholder = "First Name (required)",
				};
				slFirstName.Children.Add(lblFirstNameLabel);
				slFirstName.Children.Add(_firstNameEntry);
				gridAddData.Children.Add(slFirstName, 1, 4, 1, 2);

				var slMiddleName = new StackLayout { Orientation = StackOrientation.Vertical };
				var lblMiddleNameLabel = new Label
				{
					Text = "Middle Name",
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					FontAttributes = FontAttributes.Bold,
					TextColor = textColor
				};
				var entMiddleName = new Entry
				{
					Keyboard = Keyboard.Default,
					TextColor = dataColor,
					Placeholder = "Middle Name",
				};
				slMiddleName.Children.Add(lblMiddleNameLabel);
				slMiddleName.Children.Add(entMiddleName);
				gridAddData.Children.Add(slMiddleName, 1, 4, 3, 4);

				var slLastName = new StackLayout { Orientation = StackOrientation.Vertical };
				var lblLastNameLabel = new Label
				{
					Text = "Last Name",
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					FontAttributes = FontAttributes.Bold,
					TextColor = textColor
				};
				var entLastName = new Entry
				{
					Keyboard = Keyboard.Default,
					TextColor = dataColor,
					Placeholder = "Last Name (required)",
				};
				slLastName.Children.Add(lblLastNameLabel);
				slLastName.Children.Add(entLastName);
				gridAddData.Children.Add(slLastName, 1, 4, 5, 6);

				var slDob = new StackLayout { Orientation = StackOrientation.Vertical };
				var lblDobLabel = new Label
				{
					Text = "Date of Birth",
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					FontAttributes = FontAttributes.Bold,
					TextColor = textColor,
					HorizontalTextAlignment = TextAlignment.Start
				};
				var entDob = new Entry
				{
					TextColor = dataColor,
					Placeholder = "mm/dd/yyyy (required)",
					Keyboard = Keyboard.Numeric
				};
				slDob.Children.Add(lblDobLabel);
				slDob.Children.Add(entDob);
				gridAddData.Children.Add(slDob, 1, 7);

				var slGender = new StackLayout { Orientation = StackOrientation.Vertical };
				var lblGenderLabel = new Label
				{
					Text = "Gender",
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					FontAttributes = FontAttributes.Bold,
					TextColor = textColor,
					HorizontalTextAlignment = TextAlignment.Start
				};
				slGender.Children.Add(lblGenderLabel);


				gridAddData.Children.Add(slGender, 3, 7);

				var slHomePhone = new StackLayout { Orientation = StackOrientation.Vertical };
				var lblHomePhoneLabel = new Label
				{
					Text = "Home Phone",
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					FontAttributes = FontAttributes.Bold,
					TextColor = textColor,
					HorizontalTextAlignment = TextAlignment.Start
				};
				var entHomePhone = new Entry
				{
					TextColor = dataColor,
					Keyboard = Keyboard.Telephone,
					Placeholder = "888-888-8888",
					//MaxLength = 12
				};
				entHomePhone.TextChanged += (object sender, TextChangedEventArgs e) =>
				{
				};
				slHomePhone.Children.Add(lblHomePhoneLabel);
				slHomePhone.Children.Add(entHomePhone);
				gridAddData.Children.Add(slHomePhone, 1, 9);

				var slMobilePhone = new StackLayout { Orientation = StackOrientation.Vertical };
				var lblMobilePhoneLabel = new Label
				{
					Text = "Mobile Phone",
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					FontAttributes = FontAttributes.Bold,
					TextColor = textColor,
					HorizontalTextAlignment = TextAlignment.Start
				};
				var entMobilePhone = new Entry
				{
					TextColor = dataColor,
					Keyboard = Keyboard.Telephone,
					Placeholder = "888-888-8888",
				};
				entMobilePhone.TextChanged += (object sender, TextChangedEventArgs e) =>
				{
				};
				slMobilePhone.Children.Add(lblMobilePhoneLabel);
				slMobilePhone.Children.Add(entMobilePhone);
				gridAddData.Children.Add(slMobilePhone, 3, 9);
				#endregion

				string breakText = "_______________________________________________________________________________________________________________________________________________________________________________";
				var lblBreakLine = new Label { LineBreakMode = LineBreakMode.NoWrap, TextColor = Color.Red };
				lblBreakLine.Text = breakText;
				addGrid.Children.Add(lblBreakLine, 0, 3, 2, 3);

				var slFrameContent = new StackLayout { Orientation = StackOrientation.Vertical };
				slFrameContent.Children.Add(addGrid);
				var svAddData = new ScrollView { Content = gridAddData, IsClippedToBounds = true, IsVisible = true };
				slFrameContent.Children.Add(svAddData);

				var addFrame = new Frame
				{
					Content = slFrameContent,
					Padding = 5,
					HasShadow = true
				};

				var rl = new RelativeLayout();
				rl.Children.Add(bvBackground, Microsoft.Maui.Controls.Constraint.Constant(0), Microsoft.Maui.Controls.Constraint.Constant(0),
					Microsoft.Maui.Controls.Constraint.RelativeToParent((parent) =>
					   parent.Width),
					Microsoft.Maui.Controls.Constraint.RelativeToParent((parent) =>
					   parent.Height));

				rl.Children.Add(addFrame,
					Microsoft.Maui.Controls.Constraint.RelativeToParent((parent) => (parent.Width * .25) / 2),
					Microsoft.Maui.Controls.Constraint.Constant(Device.RuntimePlatform == Device.iOS ? 60 : 40),
					Microsoft.Maui.Controls.Constraint.RelativeToParent((parent) => parent.Width * .75));

				Content = rl;
			}

			void cancelButton_Clicked(object sender, EventArgs e)
			{
				_firstNameEntry.Focus();
				_firstNameEntry.Unfocus();   // done to remove focus from an entry field so keyboard will go away
			}

			void doneButton_Clicked(object sender, EventArgs e)
			{
				_firstNameEntry.Focus();
				_firstNameEntry.Unfocus();   // done to remove focus from an entry field so keyboard will go away
			}

			DataTemplate CreateDtForList()
			{
				var dt = new DataTemplate(() =>
				{
					// grid for one row definition
					var grid = new Grid
					{
						RowDefinitions = {
							new RowDefinition { Height = new GridLength (84, GridUnitType.Absolute) },
						},
						ColumnDefinitions = {
							new ColumnDefinition { Width = new GridLength (15, GridUnitType.Absolute)  },
							new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star)  },
							new ColumnDefinition { Width = new GridLength (3, GridUnitType.Absolute)  }
						},
						ColumnSpacing = 1,
						RowSpacing = 1,
						Padding = 0,
						VerticalOptions = LayoutOptions.Center
					};

					Color txtColor = Color.Blue;

					var fontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));

					var nameData = new Label
					{
						TextColor = txtColor,
						FontSize = fontSize,
						HorizontalTextAlignment = TextAlignment.Start,
						VerticalTextAlignment = TextAlignment.Center,
						VerticalOptions = LayoutOptions.Center
					};
					nameData.SetBinding(Label.TextProperty, "Name");
					var genderData = new Label
					{
						TextColor = txtColor,
						FontSize = fontSize,
						HorizontalTextAlignment = TextAlignment.Start,
						VerticalTextAlignment = TextAlignment.Center,
						VerticalOptions = LayoutOptions.Center
					};
					genderData.SetBinding(Label.TextProperty, "Gender");
					var slNameGender = new StackLayout { Orientation = StackOrientation.Horizontal };
					var lblGender1 = new Label { Text = " (", FontSize = fontSize };

					var lblGender2 = new Label { Text = ")", FontSize = fontSize, };
					slNameGender.Children.Add(nameData);
					slNameGender.Children.Add(lblGender1);
					slNameGender.Children.Add(genderData);
					slNameGender.Children.Add(lblGender2);


					var lblDob = new Label
					{
						TextColor = txtColor,
						Text = "DOB: ",
						FontSize = fontSize,
						FontAttributes = FontAttributes.Bold
					};
					var dobData = new Label
					{
						TextColor = txtColor,
						FontSize = fontSize,
						HorizontalTextAlignment = TextAlignment.Start,
						VerticalTextAlignment = TextAlignment.Center,
						VerticalOptions = LayoutOptions.Center
					};
					dobData.SetBinding(Label.TextProperty, "DateOfBirth");
					var slDobPhone = new StackLayout { Orientation = StackOrientation.Horizontal };
					slDobPhone.Children.Add(lblDob);
					slDobPhone.Children.Add(dobData);

#pragma warning disable 618
					var lblSpacer = new Label { Text = "      ", FontSize = fontSize };
#pragma warning restore 618
					slDobPhone.Children.Add(lblSpacer);

					var lblPhone = new Label
					{
						TextColor = txtColor,
						Text = "PHONE: ",
						FontSize = fontSize,
						FontAttributes = FontAttributes.Bold
					};
					var phoneData = new Label
					{
						TextColor = txtColor,
						FontSize = fontSize,
						HorizontalTextAlignment = TextAlignment.Start,
						VerticalTextAlignment = TextAlignment.Center,
						VerticalOptions = LayoutOptions.Center
					};
					phoneData.SetBinding(Label.TextProperty, "PrimaryPhone");
					slDobPhone.Children.Add(lblPhone);
					slDobPhone.Children.Add(phoneData);

					var slTotal = new StackLayout { Orientation = StackOrientation.Vertical };
					slTotal.Children.Add(slNameGender);
					slTotal.Children.Add(slDobPhone);

					grid.Children.Add(slTotal, 1, 0);
					return new ViewCell
					{
						View = grid
					};

				});
				return dt;
			}
		}
	}
}
