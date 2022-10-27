using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	static class ButtonElement
	{
		/// <summary>
		/// The backing store for the <see cref="IButtonElement.Command" /> bindable property.
		/// </summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(IButtonElement.Command), typeof(ICommand), typeof(IButtonElement), null, propertyChanging: OnCommandChanging, propertyChanged: OnCommandChanged);

		/// <summary>
		/// The backing store for the <see cref="IButtonElement.CommandParameter" /> bindable property.
		/// </summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(IButtonElement.CommandParameter), typeof(object), typeof(IButtonElement), null,
			propertyChanged: (bindable, oldvalue, newvalue) => CommandCanExecuteChanged(bindable, EventArgs.Empty));

		static void OnCommandChanged(BindableObject bo, object o, object n)
		{
			IButtonElement button = (IButtonElement)bo;
			if (n is ICommand newCommand)
				newCommand.CanExecuteChanged += button.OnCommandCanExecuteChanged;

			CommandChanged(button);
		}

		static void OnCommandChanging(BindableObject bo, object o, object n)
		{
			IButtonElement button = (IButtonElement)bo;
			if (o != null)
			{
				(o as ICommand).CanExecuteChanged -= button.OnCommandCanExecuteChanged;
			}
		}

		/// <summary>
		/// The string identifier for the pressed visual state of this control.
		/// </summary>
		public const string PressedVisualState = "Pressed";

		/// <summary>
		/// A method to signal that the <see cref="IButtonElement.Command"/> property has been changed.
		/// </summary>
		/// <param name="sender">The object initiating this event.</param>
		public static void CommandChanged(IButtonElement sender)
		{
			if (sender.Command != null)
			{
				CommandCanExecuteChanged(sender, EventArgs.Empty);
			}
			else
			{
				sender.IsEnabledCore = true;
			}
		}

		/// <summary>
		/// A method to signal that the <see cref="Command.CanExecute(object)"/> might have changed and needs to be reevaluated.
		/// </summary>
		/// <param name="sender">The object initiating this event.</param>
		/// <param name="e">Arguments associated with this event.</param>
		public static void CommandCanExecuteChanged(object sender, EventArgs e)
		{
			IButtonElement ButtonElementManager = (IButtonElement)sender;
			ICommand cmd = ButtonElementManager.Command;
			if (cmd != null)
			{
				ButtonElementManager.IsEnabledCore = cmd.CanExecute(ButtonElementManager.CommandParameter);
			}
		}

		/// <summary>
		/// A method to signal that this element was clicked/tapped.
		/// By calling this, the <see cref="IButtonElement.Command"/> and clicked events are triggered.
		/// </summary>
		/// <param name="visualElement">The element that was interacted with.</param>
		/// <param name="ButtonElementManager">The button element implementation to trigger the commands and events on.</param>
		public static void ElementClicked(VisualElement visualElement, IButtonElement ButtonElementManager)
		{
			if (visualElement.IsEnabled == true)
			{
				ButtonElementManager.Command?.Execute(ButtonElementManager.CommandParameter);
				ButtonElementManager.PropagateUpClicked();
			}
		}

		/// <summary>
		/// A method to signal that this element was pressed.
		/// By calling this, <see cref="IButtonElement.IsPressed"/> is set to <see langword="true"/>, the visual state is changed and events are triggered.
		/// </summary>
		/// <param name="visualElement">The element that was interacted with.</param>
		/// <param name="ButtonElementManager">The button element implementation to trigger the commands and events on.</param>
		public static void ElementPressed(VisualElement visualElement, IButtonElement ButtonElementManager)
		{
			if (visualElement.IsEnabled == true)
			{
				ButtonElementManager.SetIsPressed(true);
				visualElement.ChangeVisualStateInternal();
				ButtonElementManager.PropagateUpPressed();
			}
		}

		/// <summary>
		/// A method to signal that this element was released.
		/// By calling this, <see cref="IButtonElement.IsPressed"/> is set to <see langword="false"/>, the visual state is changed and events are triggered.
		/// </summary>
		/// <param name="visualElement">The element that was interacted with.</param>
		/// <param name="ButtonElementManager">The button element implementation to trigger the commands and events on.</param>
		public static void ElementReleased(VisualElement visualElement, IButtonElement ButtonElementManager)
		{
			if (visualElement.IsEnabled == true)
			{
				IButtonController buttonController = ButtonElementManager as IButtonController;
				ButtonElementManager.SetIsPressed(false);
				visualElement.ChangeVisualStateInternal();
				ButtonElementManager.PropagateUpReleased();
			}
		}
	}
}
