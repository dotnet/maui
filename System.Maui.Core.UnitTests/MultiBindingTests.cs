using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Xamarin.Forms;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class MultiBindingTests : BaseTestFixture
	{
		const string c_Fallback = "First Middle Last";
		const string c_TargetNull = "No Name Given";

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void TestChildOneWayOverridesMultiTwoWay()
		{
			// This tests a weird edge case where the MultiBinding is TwoWay but
			// one of the child bindings is OneWay. This results in a situation where
			// you actually can't change the target value to be inconsistent with the
			// the source value for the OneWay because ConvertBack causes Convert
			// to get re-evaluated (and this effectively negates the change to the
			// target). This ensures that the target and source never go out of sync,
			// as the overall binding is still TwoWay.

			var group = new GroupViewModel();
			var stack = new StackLayout
			{
				BindingContext = group.Person1
			};

			string oldName = group.Person1.FullName;
			string oldFirstName = group.Person1.FirstName;
			string oldMiddleName = group.Person1.MiddleName;
			string oldLastName = group.Person1.LastName;

			var label = new Label();
			label.SetBinding(Label.TextProperty, new MultiBinding
			{
				Bindings = new Collection<BindingBase>
				{
					new Binding(nameof(PersonViewModel.FirstName), mode: BindingMode.OneWay),
					new Binding(nameof(PersonViewModel.MiddleName)),
					new Binding(nameof(PersonViewModel.LastName)),
				},
				Converter = new StringConcatenationConverter(),
				Mode = BindingMode.TwoWay,
			});
			stack.Children.Add(label);

			Assert.AreEqual(oldName, label.Text);
			Assert.AreEqual(oldName, group.Person1.FullName);

			label.Text = $"{oldFirstName.ToUpper()} {oldMiddleName} {oldLastName.ToUpper()}";
			Assert.AreEqual($"{oldFirstName} {oldMiddleName} {oldLastName.ToUpper()}", label.Text);
			Assert.AreEqual($"{oldFirstName} {oldMiddleName} {oldLastName.ToUpper()}", group.Person1.FullName);
		}

		[Test]
		public void TestRelativeSources()
		{
			// Self
			var entry1 = new Entry()
			{
				FontFamily = "Courier New",
				FontSize = 12,
				FontAttributes = FontAttributes.Italic
			};
			entry1.SetBinding(Entry.TextProperty,
				new MultiBinding
				{
					Bindings = new Collection<BindingBase>
					{
						new Binding(nameof(Entry.FontFamily), source: RelativeBindingSource.Self),
						new Binding(nameof(Entry.FontSize), source: RelativeBindingSource.Self),
						new Binding(nameof(Entry.FontAttributes), source: RelativeBindingSource.Self),
					},
					Converter = new StringConcatenationConverter()
				});
			Assert.AreEqual("Courier New 12 Italic", entry1.Text);
			// Our unit test's ConvertBack should throw an exception below because the desired
			// return types aren't all strings
			Assert.Throws<Exception>(new TestDelegate(() => entry1.Text = "Arial 12 Italic"));

			// FindAncestor and FindAncestorBindingContext
			// are already tested in TestNestedMultiBindings
			TestNestedMultiBindings();

			// TemplatedParent
			var templ = new ControlTemplate(typeof(ExpanderControlTemplate));
			var expander = new ExpanderControl
			{
				ControlTemplate = templ,
				Content = new Label { Text = "Content" },
				IsEnabled = true,
				IsExpanded = true
			};
			var cp = expander.Children[0].LogicalChildren[1] as ContentPresenter;
			Assert.IsTrue(cp.IsVisible);
			expander.IsEnabled = false;
			Assert.IsFalse(cp.IsVisible);
			expander.IsEnabled = true;
			Assert.IsTrue(cp.IsVisible);
			expander.IsExpanded = false;
			Assert.IsFalse(cp.IsVisible);
		}

		[Test]
		public void TestNestedMultiBindings()
		{
			var group = new GroupViewModel();
			var stack = new StackLayout
			{
				BindingContext = group
			};

			var checkBox = new CheckBox();
			checkBox.SetBinding(
				CheckBox.IsCheckedProperty,
				new MultiBinding
				{
					Bindings = new Collection<BindingBase>
					{
						new MultiBinding
						{
							Bindings = new Collection<BindingBase>
							{
								new Binding(nameof(PersonViewModel.IsOver16)),
								new Binding(nameof(PersonViewModel.HasPassedTest)),
								new Binding(nameof(PersonViewModel.IsSuspended), converter: new Inverter()),
								new Binding(
									nameof(GroupViewModel.SuspendAll),
									converter: new Inverter(),
									source: new RelativeBindingSource(
										RelativeBindingSourceMode.FindAncestorBindingContext,
										ancestorType: typeof(GroupViewModel)))
							},
							Converter = new AllTrueMultiConverter()
						},
						new Binding(nameof(PersonViewModel.IsMonarch)),
						new Binding(
							$"{nameof(Element.BindingContext)}.{nameof(GroupViewModel.PardonAllSuspensions)}",
							source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(StackLayout))),
					},
					Converter = new AnyTrueMultiConverter()
				});

			// ^^
			// CanDrive = (IsOver16 && HasPassedTest && !IsSuspended && !Group.SuspendAll) || IsMonarch || Group.PardonAllSuspensions

			checkBox.BindingContext = group.Person5;
			stack.Children.Add(checkBox);

			// Monarch can do whatever she wants
			Assert.IsTrue(checkBox.IsChecked);

			// ... Until being deposed after a coup
			group.Person5.IsMonarch = false;
			Assert.IsFalse(checkBox.IsChecked);

			// After passing test she can drive again
			group.Person5.HasPassedTest = true;
			Assert.IsTrue(checkBox.IsChecked);

			// Martial law declared; no one can drive
			group.SuspendAll = true;
			Assert.IsFalse(checkBox.IsChecked);

			// Martial law is over
			group.SuspendAll = false;
			Assert.IsTrue(checkBox.IsChecked);

			// But she got in an accident and now can't drive again
			group.Person5.IsSuspended = true;
			Assert.IsFalse(checkBox.IsChecked);

			// The new PM has pardoned everyone after the end of the rebellion
			group.PardonAllSuspensions = true;
			Assert.IsTrue(checkBox.IsChecked);
		}

		[Test]
		public void TestConverterReturnValues()
		{
			var group = new GroupViewModel();
			var stack = new StackLayout
			{
				BindingContext = group
			};

			string oldName, oldFirstName, oldMiddleName, oldLastName, newLabelText;

			// "Convert" return values
			oldName = group.Person1.FullName;
			oldFirstName = group.Person1.FirstName;
			var label1 = GenerateNameLabel(nameof(group.Person1), BindingMode.TwoWay);
			stack.Children.Add(label1);
			group.Person1.FirstName = "DoNothing";
			Assert.AreEqual(oldName, label1.Text);
			Assert.AreEqual("DoNothing", group.Person1.FirstName);

			group.Person1.FirstName = "UnsetValue";
			Assert.AreEqual(c_Fallback, label1.Text);
			Assert.AreEqual("UnsetValue", group.Person1.FirstName);

			group.Person1.FirstName = "null";
			Assert.AreEqual(c_TargetNull, label1.Text);
			Assert.AreEqual("null", group.Person1.FirstName);

			// "ConvertBack" return values
			oldName = group.Person2.FullName;
			oldFirstName = group.Person2.FirstName;
			oldMiddleName = group.Person2.MiddleName;
			oldLastName = group.Person2.LastName;

			var label2 = GenerateNameLabel(nameof(group.Person2), BindingMode.TwoWay);
			stack.Children.Add(label2);
			label2.Text = $"DoNothing {oldMiddleName} {oldLastName.ToUpper()}";
			Assert.AreEqual($"{oldFirstName} {oldMiddleName} {oldLastName.ToUpper()}", group.Person2.FullName);
			Assert.AreEqual(group.Person2.FullName, label2.Text);

			label2.Text = oldName;
			Assert.AreEqual(oldName, group.Person2.FullName);
			Assert.AreEqual(oldName, label2.Text);
			// Any UnsetValue prevents any changes to source but target accepts value
			label2.Text = $"{oldFirstName.ToUpper()} UnsetValue {oldLastName}";
			Assert.AreEqual(oldName, group.Person2.FullName);
			Assert.AreEqual($"{oldFirstName.ToUpper()} UnsetValue {oldLastName}", label2.Text);

			label2.Text = oldName;
			Assert.AreEqual(oldName, group.Person2.FullName);
			Assert.AreEqual(oldName, label2.Text);
			label2.Text = "null";
			// Returning null prevents changes to source but target accepts value
			Assert.AreEqual(oldName, group.Person2.FullName);
			Assert.AreEqual("null", label2.Text);

			// Insufficient memebrs in ConvertBack array don't affect remaining
			label2.Text = oldName;
			Assert.AreEqual(oldName, group.Person2.FullName);
			Assert.AreEqual(oldName, label2.Text);
			label2.Text = $"Duck Duck";
			Assert.AreEqual($"Duck Duck {oldLastName}", group.Person2.FullName);
			// Target can't go out of sync with source
			Assert.AreEqual($"Duck Duck {oldLastName}", label2.Text);
			
			// Too many members are no problem either 
			label2.Text = oldName;
			Assert.AreEqual(oldName, group.Person2.FullName);
			label2.Text = oldName + " Extra";			
			Assert.AreEqual(oldName, group.Person2.FullName);
			// Target still won't go out of sync with source
			Assert.AreEqual(oldName, label2.Text);
		}

		[Test]
		public void TestEfficiency()
		{
			var group = new GroupViewModel();
			var stack = new StackLayout
			{
				BindingContext = group.Person1
			};

			string oldName = group.Person1.FullName;

			var converter = new StringConcatenationConverter();

			var label = new Label();
			label.SetBinding(Label.TextProperty, new MultiBinding
			{
				Bindings = new Collection<BindingBase>
				{
					new Binding(nameof(PersonViewModel.FirstName)),
					new Binding(nameof(PersonViewModel.MiddleName)),
					new Binding(nameof(PersonViewModel.LastName)),
				},
				Converter = converter,
				Mode = BindingMode.TwoWay,
			});

			// Initial binding should result in 1 Convert, no ConvertBack's
			Assert.AreEqual(1, converter.Converts);
			Assert.AreEqual(0, converter.ConvertBacks);

			// Parenting results in bctx change; should be 1 additional Convert, no ConvertBack's
			stack.Children.Add(label);
			Assert.AreEqual(group.Person1.FullName, label.Text);
			Assert.AreEqual(2, converter.Converts);
			Assert.AreEqual(0, converter.ConvertBacks);

			// Source change results in 1 additional Convert, no ConvertBack's
			group.Person1.FirstName = group.Person1.FullName.ToUpper();
			Assert.AreEqual(3, converter.Converts);
			Assert.AreEqual(0, converter.ConvertBacks);

			// Target change results in 1 ConvertBack, one additional Convert
			label.Text = oldName;
			Assert.AreEqual(oldName, group.Person1.FullName);
			Assert.AreEqual(4, converter.Converts);
			Assert.AreEqual(1, converter.ConvertBacks);
		}

		[Test]
		public void TestBindingModes()
		{
			var group = new GroupViewModel();
			var stack = new StackLayout
			{
				BindingContext = group
			};

			string oldName = group.Person1.FullName;
			var label1W = GenerateNameLabel(nameof(group.Person1), BindingMode.OneWay);
			stack.Children.Add(label1W);
			Assert.AreEqual(group.Person1.FullName, label1W.Text);
			label1W.SetValueCore(Label.TextProperty, "don't change source", Internals.SetValueFlags.None);
			Assert.AreEqual("don't change source", label1W.Text);
			Assert.AreEqual(oldName, group.Person1.FullName);

			var label2W = GenerateNameLabel(nameof(group.Person2), BindingMode.TwoWay);
			stack.Children.Add(label2W);
			Assert.AreEqual(group.Person2.FullName, label2W.Text);
			label2W.Text = group.Person2.FullName.ToUpper();
			Assert.AreEqual(group.Person2.FullName.ToUpper(), label2W.Text);

			oldName = group.Person3.FullName;
			var label1WTS = GenerateNameLabel(nameof(group.Person3), BindingMode.OneWayToSource);
			stack.Children.Add(label1WTS);
			// Initial value is target fallback
			Assert.AreEqual(c_Fallback, label1WTS.Text);
			Assert.AreEqual(c_Fallback, group.Person3.FullName);
			label1WTS.Text = oldName;
			Assert.AreEqual(oldName, label1WTS.Text);
			Assert.AreEqual(oldName, group.Person3.FullName);

			oldName = group.Person4.FullName;
			var label1T = GenerateNameLabel(nameof(group.Person4), BindingMode.OneTime);
			stack.Children.Add(label1T);
			Assert.AreEqual(group.Person4.FullName, label1T.Text);
			group.Person4.FirstName = "Do";
			group.Person4.MiddleName = "Not";
			group.Person4.LastName = "Update";
			// changing source values should not trigger update
			Assert.AreEqual(oldName, label1T.Text);
			Assert.AreEqual("Do Not Update", group.Person4.FullName);
			group.Person4 = group.Person1;
			// changing the bctx should trigger update
			Assert.AreEqual(group.Person1.FullName, label1T.Text);
		}

		private Label GenerateNameLabel(string person, BindingMode mode)
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, new MultiBinding
			{
				Bindings = new Collection<BindingBase>
				{
					new Binding(nameof(PersonViewModel.FirstName)),
					new Binding(nameof(PersonViewModel.MiddleName)),
					new Binding(nameof(PersonViewModel.LastName)),
				},
				Converter = new StringConcatenationConverter(),
				Mode = mode,
				FallbackValue = c_Fallback,
				TargetNullValue = c_TargetNull
			});
			label.SetBinding(Label.BindingContextProperty, new Binding(person));
			return label;
		}

		public class Inverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				bool? b = value as bool?;
				if (b == null)
					return false;
				return !b.Value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return Convert(value, targetType, parameter, culture);
			}
		}

		public class AllTrueMultiConverter : IMultiValueConverter
		{
			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
			{
				if (values == null || !targetType.IsAssignableFrom(typeof(bool)))
					// Return UnsetValue to use the binding FallbackValue
					return BindableProperty.UnsetValue;
				foreach (var value in values)
				{
					if (!(value is bool b))
						return BindableProperty.UnsetValue;
					else if (!b)
						return false;
				}
				return true;
			}

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
			{
				if (!(value is bool b) || targetTypes.Any(t => !t.IsAssignableFrom(typeof(bool))))
					// Return null to indicate conversion back is not possible
					return null;

				if (b)
					return targetTypes.Select(t => (object)true).ToArray();
				else
					// Can't convert back from false because of ambiguity
					return null;
			}
		}

		public class AnyTrueMultiConverter : IMultiValueConverter
		{
			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
			{
				if (values == null || !targetType.IsAssignableFrom(typeof(bool)))
					// Return UnsetValue to use the binding FallbackValue
					return BindableProperty.UnsetValue;
				foreach (var value in values)
				{
					if (!(value is bool b))
						return BindableProperty.UnsetValue;
					else if (b)
						return true;
				}
				return false;
			}

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
			{
				if (!(value is bool b) || targetTypes.Any(t => !t.IsAssignableFrom(typeof(bool))))
					// Return null to indicate conversion back is not possible
					return null;

				if (!b)
					return targetTypes.Select(t => (object)false).ToArray();
				else
					// Can't convert back from true because of ambiguity
					return null;
			}
		}

		public class StringConcatenationConverter : IMultiValueConverter
		{
			public int Converts { get; private set; }

			public int ConvertBacks { get; private set; }

			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
			{
				Converts++;

				if (values is null)
					return null;
				string concatenator = parameter as string ?? " ";
				StringBuilder sb = new StringBuilder();
				int i = 0;

				if (values.All(v => string.IsNullOrEmpty(v as string)))
					return BindableProperty.UnsetValue;

				foreach (var value in values)
				{
					if (value as string == "DoNothing")
						return Binding.DoNothing;
					if (value as string == "UnsetValue")
						return BindableProperty.UnsetValue;
					if (value as string == "null")
						return null;

					if (i != 0 && concatenator != null)
						sb.Append(concatenator);
					sb.Append(value?.ToString());
					i++;
				}
				return sb.ToString();
			}

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
			{
				ConvertBacks++;

				string s = value as string;
				if (s == "null" || string.IsNullOrEmpty(s))
					return null;

				string concatenator = parameter as string ?? " ";

				if (!targetTypes.All(t => t == typeof(string)))
					// Normally we'd return null but throw exception just for unit test to catch
					throw new Exception("Invalid targetTypes");

				var array = s.Split(new string[] { concatenator }, StringSplitOptions.RemoveEmptyEntries).Cast<object>().ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					var str = array[i] as string;
					if (str == "null")
						array[i] = null;
					if (str == "UnsetValue")
						array[i] = BindableProperty.UnsetValue;
					if (str == "DoNothing")
						array[i] = Binding.DoNothing;
				}
				return array;
			}
		}

		public class GroupViewModel : INotifyPropertyChanged
		{
			PersonViewModel _person1 = new PersonViewModel
			{
				FirstName = "Gaius",
				MiddleName = "Julius",
				LastName = "Caesar",
				IsOver16 = true,
				HasPassedTest = false,
				IsSuspended = false
			};
			PersonViewModel _person2 = new PersonViewModel
			{
				FirstName = "William",
				MiddleName = "Henry",
				LastName = "Gates",
				IsOver16 = true,
				HasPassedTest = true,
				IsSuspended = false
			};
			PersonViewModel _person3 = new PersonViewModel
			{
				FirstName = "John",
				MiddleName = "Fitzgerald",
				LastName = "Kennedy",
				IsOver16 = true,
				HasPassedTest = true,
				IsSuspended = true
			};
			PersonViewModel _person4 = new PersonViewModel
			{
				FirstName = "Harry",
				MiddleName = "James",
				LastName = "Potter",
				HasPassedTest = true,
				IsOver16 = false,
				IsSuspended = false
			};
			PersonViewModel _person5 = new PersonViewModel
			{
				FirstName = "Queen",
				MiddleName = "Elizabeth",
				LastName = "II",
				HasPassedTest = false,
				IsOver16 = true,
				IsSuspended = false,
				IsMonarch = true,
			};

			public PersonViewModel Person1
			{
				get => _person1;
				set
				{
					_person1 = value;
					OnPropertyChanged();
				}
			}

			public PersonViewModel Person2
			{
				get => _person2;
				set
				{
					_person2 = value;
					OnPropertyChanged();
				}
			}

			public PersonViewModel Person3
			{
				get => _person3;
				set
				{
					_person3 = value;
					OnPropertyChanged();
				}
			}

			public PersonViewModel Person4
			{
				get => _person4;
				set
				{
					_person4 = value;
					OnPropertyChanged();
				}
			}

			public PersonViewModel Person5
			{
				get => _person5;
				set
				{
					_person5 = value;
					OnPropertyChanged();
				}
			}

			bool _PardonAllSuspensions;
			public bool PardonAllSuspensions
			{
				get => _PardonAllSuspensions;
				set
				{
					_PardonAllSuspensions = value;
					OnPropertyChanged();
				}
			}

			bool _SuspendAll;
			public bool SuspendAll
			{
				get => _SuspendAll;
				set
				{
					_SuspendAll = value;
					OnPropertyChanged();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			void OnPropertyChanged([CallerMemberName] string name = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
			}
		}

		public class PersonViewModel : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			string _FirstName;
			public string FirstName
			{
				get
				{
					return _FirstName;
				}
				set
				{
					if (_FirstName != value)
					{
						_FirstName = value;
						OnPropertyChanged();
					}
				}
			}

			string _MiddleName;
			public string MiddleName
			{
				get
				{
					return _MiddleName;
				}
				set
				{
					if (_MiddleName != value)
					{
						_MiddleName = value;
						OnPropertyChanged();
					}
				}
			}

			string _LastName;
			public string LastName
			{
				get
				{
					return _LastName;
				}
				set
				{
					if (_LastName != value)
					{
						_LastName = value;
						OnPropertyChanged();
					}
				}
			}

			public string FullName => FirstName + " " + MiddleName + " " + LastName;

			public bool CanDrive => IsOver16 && HasPassedTest && !IsSuspended;

			bool _IsOver16;
			public bool IsOver16
			{
				get
				{
					return _IsOver16;
				}
				set
				{
					if (_IsOver16 != value)
					{
						_IsOver16 = value;
						OnPropertyChanged();
					}
				}
			}

			bool _HasPassedTest;
			public bool HasPassedTest
			{
				get
				{
					return _HasPassedTest;
				}
				set
				{
					if (_HasPassedTest != value)
					{
						_HasPassedTest = value;
						OnPropertyChanged();
					}
				}
			}

			bool _IsSuspended;
			public bool IsSuspended
			{
				get
				{
					return _IsSuspended;
				}
				set
				{
					if (_IsSuspended != value)
					{
						_IsSuspended = value;
						OnPropertyChanged();
					}
				}
			}

			bool _IsMonarch;
			public bool IsMonarch
			{
				get
				{
					return _IsMonarch;
				}
				set
				{
					if (_IsMonarch != value)
					{
						_IsMonarch = value;
						OnPropertyChanged();
					}
				}
			}

			void OnPropertyChanged([CallerMemberName] string name = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
			}
		}

		[ContentProperty(nameof(Content))]
		public class ExpanderControl : TemplatedView
		{
			#region bool IsExpanded dependency property
			public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
				"IsExpanded",
				typeof(bool),
				typeof(ExpanderControl),
				true);
			public bool IsExpanded
			{
				get
				{
					return (bool)GetValue(IsExpandedProperty);
				}
				set
				{

					SetValue(IsExpandedProperty, value);
				}
			}
			#endregion

			#region object Content dependency property
			public static BindableProperty ContentProperty = BindableProperty.Create(
				"Content",
				typeof(View),
				typeof(ExpanderControl),
				null);
			public View Content
			{
				get
				{
					return (View)GetValue(ContentProperty);
				}
				set
				{
					SetValue(ContentProperty, value);
				}
			}
			#endregion
		}

		public class ExpanderControlTemplate : Grid
		{
			public ExpanderControlTemplate()
			{
				this.RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = new GridLength(0, GridUnitType.Auto)},
					new RowDefinition { Height = new GridLength(1, GridUnitType.Star)}
				};

				var expander = new Button { Text = "^" };
				Grid.SetRow(expander, 0);
				this.Children.Add(expander);

				var cp = new ContentPresenter();
				Grid.SetRow(cp, 1);
				cp.SetBinding(ContentPresenter.ContentProperty, new Binding(
					nameof(ExpanderControl.Content),
					source: new RelativeBindingSource(RelativeBindingSourceMode.TemplatedParent)));
				cp.SetBinding(ContentPresenter.IsVisibleProperty, new MultiBinding
				{
					Bindings = new Collection<BindingBase>
					{
						new Binding(nameof(ExpanderControl.IsEnabled), source: RelativeBindingSource.TemplatedParent),
						new Binding(nameof(ExpanderControl.IsExpanded), source: RelativeBindingSource.TemplatedParent)
					},
					Converter = new AllTrueMultiConverter()
				});
				this.Children.Add(cp);
			}
		}
	}
}
