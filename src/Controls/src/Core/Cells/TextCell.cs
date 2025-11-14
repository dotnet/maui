#nullable disable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.Cell"/> with primary <see cref="Microsoft.Maui.Controls.TextCell.Text"/>  and <see cref="Microsoft.Maui.Controls.TextCell.Detail"/> text.</summary>
	[Obsolete("The controls which use TextCell (ListView and TableView) are obsolete. Please use CollectionView instead.")]
	public class TextCell : Cell, ICommandElement
	{
		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty =
			BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(TextCell),
			propertyChanging: CommandElement.OnCommandChanging,
			propertyChanged: CommandElement.OnCommandChanged);

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter),
				typeof(object),
				typeof(TextCell),
				null,
				propertyChanged: CommandElement.OnCommandParameterChanged);

		/// <summary>Bindable property for <see cref="Text"/>.</summary>
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(TextCell), default(string));

		/// <summary>Bindable property for <see cref="Detail"/>.</summary>
		public static readonly BindableProperty DetailProperty = BindableProperty.Create(nameof(Detail), typeof(string), typeof(TextCell), default(string));

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(TextCell), null);

		/// <summary>Bindable property for <see cref="DetailColor"/>.</summary>
		public static readonly BindableProperty DetailColorProperty = BindableProperty.Create(nameof(DetailColor), typeof(Color), typeof(TextCell), null);

		/// <summary>Gets or sets the ICommand to be executed when the TextCell is tapped. This is a bindable property.</summary>
		/// <remarks>Setting the Command property has a side effect of changing the Enabled property depending on ICommand.CanExecute.</remarks>
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>Gets or sets the parameter passed when invoking the Command. This is a bindable property.</summary>
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <summary>Gets or sets the secondary text to be displayed in the TextCell. This is a bindable property.</summary>
		public string Detail
		{
			get { return (string)GetValue(DetailProperty); }
			set { SetValue(DetailProperty, value); }
		}

		/// <summary>Gets or sets the color to render the secondary text. This is a bindable property.</summary>
		/// <remarks>Not all platforms may support transparent text rendering. Using Color.Default will result in the system theme color being used.</remarks>
		public Color DetailColor
		{
			get { return (Color)GetValue(DetailColorProperty); }
			set { SetValue(DetailColorProperty, value); }
		}

		/// <summary>Gets or sets the primary text to be displayed. This is a bindable property.</summary>
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <summary>Gets or sets the color to render the primary text. This is a bindable property.</summary>
		/// <remarks>Not all platforms may support transparent text rendering. Using Color.Default will result in the system theme color being used. Color.Default is the default color value.</remarks>
		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		protected internal override void OnTapped()
		{
			base.OnTapped();

			if (!IsEnabled)
			{
				return;
			}

			Command?.Execute(CommandParameter);
		}

		void ICommandElement.CanExecuteChanged(object sender, EventArgs eventArgs)
		{
			if (Command is null)
				return;

			IsEnabled = Command.CanExecute(CommandParameter);
		}

		WeakCommandSubscription ICommandElement.CleanupTracker { get; set; }
	}
}