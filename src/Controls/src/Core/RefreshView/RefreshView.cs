#nullable disable
using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RefreshView.xml" path="Type[@FullName='Microsoft.Maui.Controls.RefreshView']/Docs/*" />
	[ContentProperty(nameof(Content))]
	public partial class RefreshView : ContentView, IElementConfiguration<RefreshView>, IRefreshView
	{
		readonly Lazy<PlatformConfigurationRegistry<RefreshView>> _platformConfigurationRegistry;
		public event EventHandler Refreshing;

		/// <include file="../../docs/Microsoft.Maui.Controls/RefreshView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public RefreshView()
		{
			IsClippedToBounds = true;
#pragma warning disable CS0618 // Type or member is obsolete
			VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete

			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<RefreshView>>(() => new PlatformConfigurationRegistry<RefreshView>(this));
		}

		/// <summary>Bindable property for <see cref="IsRefreshing"/>.</summary>
		public static readonly BindableProperty IsRefreshingProperty =
			BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(RefreshView), false, BindingMode.TwoWay, coerceValue: OnIsRefreshingPropertyCoerced, propertyChanged: OnIsRefreshingPropertyChanged);

		static void OnIsRefreshingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			bool value = (bool)newValue;

			if (!value)
				return;

			var refreshView = (RefreshView)bindable;
			refreshView.Refreshing?.Invoke(bindable, EventArgs.Empty);
			refreshView.Command?.Execute(refreshView.CommandParameter);
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

			return value;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RefreshView.xml" path="//Member[@MemberName='IsRefreshing']/Docs/*" />
		public bool IsRefreshing
		{
			get { return (bool)GetValue(IsRefreshingProperty); }
			set { SetValue(IsRefreshingProperty, value); }
		}

		/// <summary>Bindable property for <see cref="Command"/>.</summary>
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

		/// <include file="../../docs/Microsoft.Maui.Controls/RefreshView.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter),
				typeof(object),
				typeof(RefreshView),
				null,
				propertyChanged: (bindable, oldvalue, newvalue) => ((RefreshView)(bindable)).RefreshCommandCanExecuteChanged(((RefreshView)(bindable)).Command, EventArgs.Empty));

		/// <include file="../../docs/Microsoft.Maui.Controls/RefreshView.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		void RefreshCommandCanExecuteChanged(object sender, EventArgs eventArgs)
		{
			if (IsRefreshing)
				return;

			if (Command != null)
			{
				SetValue(IsEnabledProperty, Command.CanExecute(CommandParameter));
			}
			else
			{
				SetValue(IsEnabledProperty, true);
			}
		}

		/// <summary>Bindable property for <see cref="RefreshColor"/>.</summary>
		public static readonly BindableProperty RefreshColorProperty =
			BindableProperty.Create(nameof(RefreshColor), typeof(Color), typeof(RefreshView), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/RefreshView.xml" path="//Member[@MemberName='RefreshColor']/Docs/*" />
		public Color RefreshColor
		{
			get { return (Color)GetValue(RefreshColorProperty); }
			set { SetValue(RefreshColorProperty, value); }
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, RefreshView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (IsEnabledProperty.PropertyName == propertyName &&
				!IsEnabled &&
				IsRefreshing)
			{
				IsRefreshing = false;
			}
		}

		Paint IRefreshView.RefreshColor => RefreshColor?.AsPaint();

		IView IRefreshView.Content => base.Content;
	}
}