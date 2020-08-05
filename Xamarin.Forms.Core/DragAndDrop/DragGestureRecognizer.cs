using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public class DragGestureRecognizer : GestureRecognizer
	{
		public static readonly BindableProperty CanDragProperty = BindableProperty.Create(nameof(CanDrag), typeof(bool), typeof(DragGestureRecognizer), false);

		public static readonly BindableProperty DropCompletedCommandProperty = BindableProperty.Create(nameof(DropCompletedCommand), typeof(ICommand), typeof(DragGestureRecognizer), null);

		public static readonly BindableProperty DropCompletedCommandParameterProperty = BindableProperty.Create(nameof(DropCompletedCommandParameter), typeof(object), typeof(DragGestureRecognizer), null);

		public static readonly BindableProperty DragStartingCommandProperty = BindableProperty.Create(nameof(DragStartingCommand), typeof(ICommand), typeof(DragGestureRecognizer), null);

		public static readonly BindableProperty DragStartingCommandParameterProperty = BindableProperty.Create(nameof(DragStartingCommandParameter), typeof(object), typeof(DragGestureRecognizer), null);

		public DragGestureRecognizer()
		{
			ExperimentalFlags.VerifyFlagEnabled(nameof(DragGestureRecognizer), ExperimentalFlags.DragAndDropExperimental);
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
			_ = args ?? throw new ArgumentNullException(nameof(args));

			DropCompletedCommand?.Execute(DropCompletedCommandParameter);
			DropCompleted?.Invoke(this, args);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public DragStartingEventArgs SendDragStarting(VisualElement element)
		{
			var args = new DragStartingEventArgs();

			SendDragStarting(args, element);

			if (args.Cancel || args.Handled)
				return args;

			if (element is IImageElement ie)
			{
				args.Data.Image = ie.Source;
			}

			args.Data.Text = element.GetStringValue();

			return args;
		}

		void SendDragStarting(DragStartingEventArgs args, VisualElement element)
		{
			_ = args ?? throw new ArgumentNullException(nameof(args));

			DragStartingCommand?.Execute(DragStartingCommandParameter);
			DragStarting?.Invoke(this, args);

			if (!args.Handled)
			{
				args.Data.PropertiesInternal.Add("DragSource", element);
			}
		}
	}
}
