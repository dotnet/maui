using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	[ContentProperty("Content")]
	public class RefreshView : ContentView, IElementConfiguration<RefreshView>
	{
		readonly Lazy<PlatformConfigurationRegistry<RefreshView>> _platformConfigurationRegistry;
		public event EventHandler Refreshing;

		public RefreshView()
		{
			IsClippedToBounds = true;
			VerticalOptions = LayoutOptions.FillAndExpand;
			HorizontalOptions = LayoutOptions.FillAndExpand;

			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<RefreshView>>(() => new PlatformConfigurationRegistry<RefreshView>(this));
		}

		public static readonly BindableProperty IsRefreshingProperty =
			BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(RefreshView), false, BindingMode.TwoWay, coerceValue: OnIsRefreshingPropertyCoerced, propertyChanged: OnIsRefreshingPropertyChanged);

		static void OnIsRefreshingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			bool value = (bool)newValue;

			if (!value)
				return;

			var refreshView = ((RefreshView)bindable);
			refreshView.Refreshing?.Invoke(bindable, EventArgs.Empty);
			if (refreshView.Command != null)
				refreshView.Command.Execute(refreshView.CommandParameter);
		}

		static object OnIsRefreshingPropertyCoerced(BindableObject bindable, object value)
		{
			RefreshView view = (RefreshView)bindable;
			bool newValue = (bool)value;

			// IsRefreshing can always be toggled to false
			if (!newValue)
				return value;

			if (!view.IsEnabled)
				return false;

			if (view.Command == null)
				return value;

			if (!view.Command.CanExecute(view.CommandParameter))
				return false;

			return value;
		}

		public bool IsRefreshing
		{
			get { return (bool)GetValue(IsRefreshingProperty); }
			set { SetValue(IsRefreshingProperty, value); }
		}

		public static readonly BindableProperty CommandProperty =
			BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(RefreshView), propertyChanged: OnCommandChanged);

		static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
		{
			RefreshView refreshView = (RefreshView)bindable;
			if (oldValue is ICommand oldCommand)
				oldCommand.CanExecuteChanged -= refreshView.RefreshCommandCanExecuteChanged;

			if (newValue is ICommand newCommand)
				newCommand.CanExecuteChanged += refreshView.RefreshCommandCanExecuteChanged;

			refreshView.RefreshCommandCanExecuteChanged(bindable, EventArgs.Empty);
		}

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter),
				typeof(object),
				typeof(RefreshView),
				null,
				propertyChanged: (bindable, oldvalue, newvalue) => ((RefreshView)(bindable)).RefreshCommandCanExecuteChanged(((RefreshView)(bindable)).Command, EventArgs.Empty));

		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		void RefreshCommandCanExecuteChanged(object sender, EventArgs eventArgs)
		{
			if (Command != null)
				SetValueCore(IsEnabledProperty, Command.CanExecute(CommandParameter));
			else
				SetValueCore(IsEnabledProperty, true);
		}

		public static readonly BindableProperty RefreshColorProperty =
			BindableProperty.Create(nameof(RefreshColor), typeof(Color), typeof(RefreshView), Color.Default);

		public Color RefreshColor
		{
			get { return (Color)GetValue(RefreshColorProperty); }
			set { SetValue(RefreshColorProperty, value); }
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			if (Content == null)
				return new SizeRequest(new Size(100, 100));

			return base.OnMeasure(widthConstraint, heightConstraint);
		}

		public IPlatformElementConfiguration<T, RefreshView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (IsEnabledProperty.PropertyName == propertyName)
				if (!IsEnabled && IsRefreshing)
					IsRefreshing = false;
		}
	}
}