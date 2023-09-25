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

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 13126, "[Bug] Regression: 5.0.0-pre5 often fails to draw dynamically loaded collection view content", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
#endif
	public class Issue13126 : TestContentPage
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

			_vm.Data.Add(Success);

			_vm.IsBusy = false;
		}

		internal static CollectionView BindingWithConverter()
		{
			var cv = new CollectionView
			{
				IsVisible = true,

				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("."));
					return label;
				})
			};

			cv.EmptyView = new Label { Text = "Should not see me" };

			cv.SetBinding(CollectionView.ItemsSourceProperty, new Binding("Data"));
			cv.SetBinding(VisualElement.IsVisibleProperty, new Binding("IsBusy", converter: new BoolInverter()));

			return cv;
		}

		class BoolInverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return !((bool)value);
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}

		internal class _13126VM : INotifyPropertyChanged
		{
			private bool _isBusy;

			public bool IsBusy
			{
				get
				{
					return _isBusy;
				}

				set
				{
					_isBusy = value;
					OnPropertyChanged(nameof(IsBusy));
				}
			}

			public OptimizedObservableCollection<string> Data { get; } = new OptimizedObservableCollection<string>();

			public event PropertyChangedEventHandler PropertyChanged;

			void OnPropertyChanged(string name)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
			}
		}

		internal class OptimizedObservableCollection<T> : ObservableCollection<T>
		{
			bool _shouldRaiseNotifications = true;

			public OptimizedObservableCollection()
			{
			}

			public OptimizedObservableCollection(IEnumerable<T> collection)
				: base(collection)
			{
			}

			public IDisposable BeginMassUpdate()
			{
				return new MassUpdater(this);
			}

			protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
			{
				if (_shouldRaiseNotifications)
					base.OnCollectionChanged(e);
			}

			protected override void OnPropertyChanged(PropertyChangedEventArgs e)
			{
				if (_shouldRaiseNotifications)
					base.OnPropertyChanged(e);
			}

			class MassUpdater : IDisposable
			{
				readonly OptimizedObservableCollection<T> parent;
				public MassUpdater(OptimizedObservableCollection<T> parent)
				{
					this.parent = parent;
					parent._shouldRaiseNotifications = false;
				}

				public void Dispose()
				{
					parent._shouldRaiseNotifications = true;
					parent.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
					parent.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
					parent.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				}
			}
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void CollectionViewShouldSourceShouldUpdateWhileInvisible()
		{
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
