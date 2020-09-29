using System;
using static Xamarin.Forms.Core.Markup.Markup;

namespace Xamarin.Forms.Markup
{
	public static class ElementGesturesExtensions
	{
		const string bindingContextPath = Binding.SelfPath;

		/// <summary>Add a <see cref="ClickGestureRecognizer"/>,
		/// and bind to its Command and (optionally) CommandParameter properties</summary>
		/// <param name="parameterPath">If not specified or null, no binding is created for the CommandParameter property</param>
		public static TGestureElement BindClickGesture<TGestureElement>(
			this TGestureElement gestureElement,
			string commandPath = bindingContextPath,
			object commandSource = null,
			string parameterPath = null,
			object parameterSource = null
		) where TGestureElement : Element, IGestureRecognizers
			=> ClickGesture(gestureElement, g => g.BindCommand(commandPath, commandSource, parameterPath, parameterSource));

		/// <summary>Add a <see cref="SwipeGestureRecognizer"/>,
		/// and bind to its Command and (optionally) CommandParameter properties</summary>
		/// <param name="parameterPath">If not specified or null, no binding is created for the CommandParameter property</param>
		public static TGestureElement BindSwipeGesture<TGestureElement>(
			this TGestureElement gestureElement,
			string commandPath = bindingContextPath,
			object commandSource = null,
			string parameterPath = null,
			object parameterSource = null
		) where TGestureElement : Element, IGestureRecognizers
			=> SwipeGesture(gestureElement, g => g.BindCommand(commandPath, commandSource, parameterPath, parameterSource));

		/// <summary>Add a <see cref="TapGestureRecognizer"/>,
		/// and bind to its Command and (optionally) CommandParameter properties</summary>
		/// <param name="parameterPath">If not specified or null, no binding is created for the CommandParameter property</param>
		public static TGestureElement BindTapGesture<TGestureElement>(
			this TGestureElement gestureElement,
			string commandPath = bindingContextPath,
			object commandSource = null,
			string parameterPath = null,
			object parameterSource = null
		) where TGestureElement : Element, IGestureRecognizers
			=> TapGesture(gestureElement, g => g.BindCommand(commandPath, commandSource, parameterPath, parameterSource));

		/// <summary>Add a <see cref="ClickGestureRecognizer"/>,
		/// and pass it to the supplied <paramref name="init"/> Action</summary>
		public static TGestureElement ClickGesture<TGestureElement>(this TGestureElement gestureElement, Action<ClickGestureRecognizer> init)
			where TGestureElement : Element, IGestureRecognizers
			=> Gesture(gestureElement, init);

		/// <summary>Add a <see cref="PanGestureRecognizer"/>,
		/// and pass it to the supplied <paramref name="init"/> Action</summary>
		public static TGestureElement PanGesture<TGestureElement>(this TGestureElement gestureElement, Action<PanGestureRecognizer> init)
			where TGestureElement : Element, IGestureRecognizers
			=> Gesture(gestureElement, init);

		/// <summary>Add a <see cref="PinchGestureRecognizer"/>,
		/// and pass it to the supplied <paramref name="init"/> Action</summary>
		public static TGestureElement PinchGesture<TGestureElement>(this TGestureElement gestureElement, Action<PinchGestureRecognizer> init)
			where TGestureElement : Element, IGestureRecognizers
			=> Gesture(gestureElement, init);

		/// <summary>Add a <see cref="SwipeGestureRecognizer"/>,
		/// and pass it to the supplied <paramref name="init"/> Action</summary>
		public static TGestureElement SwipeGesture<TGestureElement>(this TGestureElement gestureElement, Action<SwipeGestureRecognizer> init)
			where TGestureElement : Element, IGestureRecognizers
			=> Gesture(gestureElement, init);

		/// <summary>Add a <see cref="TapGestureRecognizer"/>,
		/// and pass it to the supplied <paramref name="init"/> Action</summary>
		public static TGestureElement TapGesture<TGestureElement>(this TGestureElement gestureElement, Action<TapGestureRecognizer> init)
			where TGestureElement : Element, IGestureRecognizers
			=> Gesture(gestureElement, init);

		/// <summary>Add a <typeparamref name="TGestureRecognizer"/>,
		/// and pass it to the supplied <paramref name="init"/> Action</summary>
		public static TGestureElement Gesture<TGestureElement, TGestureRecognizer>(
			this TGestureElement gestureElement,
			Action<TGestureRecognizer> init
		) where TGestureElement : Element, IGestureRecognizers
		  where TGestureRecognizer : GestureRecognizer, new()
		{
			VerifyExperimental();
			var gestureRecognizer = new TGestureRecognizer();
			init.Invoke(gestureRecognizer);

			gestureElement.GestureRecognizers.Add(gestureRecognizer);

			return gestureElement;
		}
	}
}