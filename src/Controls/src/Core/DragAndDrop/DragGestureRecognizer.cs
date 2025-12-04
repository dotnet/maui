using System;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides drag gesture recognition and defines the associated events for dragging and dropping. 
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/maui/fundamentals/gestures/drag-and-drop">Conceptual documentation on recognizing a drag and drop gesture</seealso>
	public class DragGestureRecognizer : GestureRecognizer
	{
		/// <summary>Bindable property for <see cref="CanDrag"/>.</summary>
		public static readonly BindableProperty CanDragProperty = BindableProperty.Create(nameof(CanDrag), typeof(bool), typeof(DragGestureRecognizer), true);

		/// <summary>Bindable property for <see cref="DropCompletedCommand"/>.</summary>
		public static readonly BindableProperty DropCompletedCommandProperty = BindableProperty.Create(nameof(DropCompletedCommand), typeof(ICommand), typeof(DragGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DropCompletedCommandParameter"/>.</summary>
		public static readonly BindableProperty DropCompletedCommandParameterProperty = BindableProperty.Create(nameof(DropCompletedCommandParameter), typeof(object), typeof(DragGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DragStartingCommand"/>.</summary>
		public static readonly BindableProperty DragStartingCommandProperty = BindableProperty.Create(nameof(DragStartingCommand), typeof(ICommand), typeof(DragGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DragStartingCommandParameter"/>.</summary>
		public static readonly BindableProperty DragStartingCommandParameterProperty = BindableProperty.Create(nameof(DragStartingCommandParameter), typeof(object), typeof(DragGestureRecognizer), null);

		bool _isDragActive;

		/// <summary>
		/// Initializes a new instance of the <see cref="DragGestureRecognizer"/> class.
		/// </summary>
		public DragGestureRecognizer()
		{
		}

		/// <summary>
		/// Occurs when a drop gesture is completed.
		/// </summary>
		public event EventHandler<DropCompletedEventArgs>? DropCompleted;


		/// <summary>
		/// Occurs when a drag gesture is detected.
		/// </summary>
		public event EventHandler<DragStartingEventArgs>? DragStarting;

		/// <summary>
		/// Gets or sets the value which indicates whether the element the gesture recognizer is attached to can be a drag source.
		/// </summary>
		/// <remarks>Default value is <see langword="true"/>.</remarks>
		public bool CanDrag
		{
			get { return (bool)GetValue(CanDragProperty); }
			set { SetValue(CanDragProperty, value); }
		}

		/// <summary>
		/// Gets or sets the command to be executed when a drop gesture is completed.
		/// </summary>
		public ICommand DropCompletedCommand
		{
			get { return (ICommand)GetValue(DropCompletedCommandProperty); }
			set { SetValue(DropCompletedCommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter that's to be passed to the <see cref="DropCompletedCommand"/>.
		/// </summary>
		public object DropCompletedCommandParameter
		{
			get { return (object)GetValue(DropCompletedCommandParameterProperty); }
			set { SetValue(DropCompletedCommandParameterProperty, value); }
		}

		/// <summary>
		/// Gets or sets the command to be executed when a drag gesture is first recognized.
		/// </summary>
		public ICommand DragStartingCommand
		{
			get { return (ICommand)GetValue(DragStartingCommandProperty); }
			set { SetValue(DragStartingCommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter that's to be passed to the <see cref="DragStartingCommand"/>.
		/// </summary>
		public object DragStartingCommandParameter
		{
			get { return (object)GetValue(DragStartingCommandParameterProperty); }
			set { SetValue(DragStartingCommandParameterProperty, value); }
		}

		internal void SendDropCompleted(DropCompletedEventArgs args)
		{
			if (!_isDragActive)
			{
				// this is mainly relevant for Android
				// Android fires an Ended action on every single view that has a drop handler
				// but we only need one of those DropCompleted actions to make it through
				return;
			}

			_isDragActive = false;
			_ = args ?? throw new ArgumentNullException(nameof(args));

			DropCompletedCommand?.Execute(DropCompletedCommandParameter);
			DropCompleted?.Invoke(Parent ?? this, args);
		}

		internal DragStartingEventArgs SendDragStarting(View element, Func<IElement?, Point?>? getPosition = null, PlatformDragStartingEventArgs? platformArgs = null)
		{
			var args = new DragStartingEventArgs(getPosition, platformArgs);

			DragStartingCommand?.Execute(DragStartingCommandParameter);
			DragStarting?.Invoke(element, args);

#pragma warning disable CS0618 // Type or member is obsolete
			if (!args.Handled)
				args.Data.PropertiesInternal.Add("DragSource", element);
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
			if (args.Cancel || args.Handled)
				return args;
#pragma warning restore CS0618 // Type or member is obsolete

			_isDragActive = true;

			if (args.Data.Image == null && element is IImageElement ie)
				args.Data.Image = ie.Source;

			if (String.IsNullOrWhiteSpace(args.Data.Text))
				args.Data.Text = element?.GetStringValue();

			return args;
		}
	}
}
