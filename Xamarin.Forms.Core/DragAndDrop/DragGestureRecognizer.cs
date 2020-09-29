using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms
{
	public class DragGestureRecognizer : GestureRecognizer
	{
		public static readonly BindableProperty CanDragProperty = BindableProperty.Create(nameof(CanDrag), typeof(bool), typeof(DragGestureRecognizer), true);

		public static readonly BindableProperty DropCompletedCommandProperty = BindableProperty.Create(nameof(DropCompletedCommand), typeof(ICommand), typeof(DragGestureRecognizer), null);

		public static readonly BindableProperty DropCompletedCommandParameterProperty = BindableProperty.Create(nameof(DropCompletedCommandParameter), typeof(object), typeof(DragGestureRecognizer), null);

		public static readonly BindableProperty DragStartingCommandProperty = BindableProperty.Create(nameof(DragStartingCommand), typeof(ICommand), typeof(DragGestureRecognizer), null);

		public static readonly BindableProperty DragStartingCommandParameterProperty = BindableProperty.Create(nameof(DragStartingCommandParameter), typeof(object), typeof(DragGestureRecognizer), null);

		bool _isDragActive;

		public DragGestureRecognizer()
		{
		}

		public event EventHandler<DropCompletedEventArgs> DropCompleted;
		public event EventHandler<DragStartingEventArgs> DragStarting;

		public bool CanDrag
		{
			get { return (bool)GetValue(CanDragProperty); }
			set { SetValue(CanDragProperty, value); }
		}

		public ICommand DropCompletedCommand
		{
			get { return (ICommand)GetValue(DropCompletedCommandProperty); }
			set { SetValue(DropCompletedCommandProperty, value); }
		}

		public object DropCompletedCommandParameter
		{
			get { return (object)GetValue(DropCompletedCommandParameterProperty); }
			set { SetValue(DropCompletedCommandParameterProperty, value); }
		}

		public ICommand DragStartingCommand
		{
			get { return (ICommand)GetValue(DragStartingCommandProperty); }
			set { SetValue(DragStartingCommandProperty, value); }
		}

		public object DragStartingCommandParameter
		{
			get { return (object)GetValue(DragStartingCommandParameterProperty); }
			set { SetValue(DragStartingCommandParameterProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendDropCompleted(DropCompletedEventArgs args)
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
			DropCompleted?.Invoke(this, args);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public DragStartingEventArgs SendDragStarting(VisualElement element)
		{
			var args = new DragStartingEventArgs();

			DragStartingCommand?.Execute(DragStartingCommandParameter);
			DragStarting?.Invoke(this, args);

			if (!args.Handled)
			{
				args.Data.PropertiesInternal.Add("DragSource", element);
			}

			if (args.Cancel || args.Handled)
				return args;

			_isDragActive = true;

			if (args.Data.Image == null && element is IImageElement ie)
			{
				args.Data.Image = ie.Source;
			}

			if (String.IsNullOrWhiteSpace(args.Data.Text))
				args.Data.Text = element.GetStringValue();

			return args;
		}
	}
}