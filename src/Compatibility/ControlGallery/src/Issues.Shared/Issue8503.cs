using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8503, "[Bug] Text is not visible on Entry element after animation finished",
		PlatformAffected.UWP)]
	public class Issue8503 : TestContentPage
	{
		Entry isolatedEntry;

		protected override void Init()
		{
			Title = "Issue 8503 [UWP] - text in Entry not visible until receiving focus";
			ScrollView scrollView = new ScrollView();

			StackLayout layout = new StackLayout { Margin = new Thickness(20, 0) };
			layout.BindingContext = this;

			Label isolatedHeader = new Label { Text = "Isolated Bug", Margin = new Thickness(0, 20), FontSize = 18 };
			Label isolatedInstructions = new Label
			{
				Text = "This is the bug isolated. To test:\n" +
					   "Click on the button below, to show Entry for Isolated Bug.\n" +
					   "An Entry appears.\n" +
					   "Check that the entry contains text 'This text should be visible, even before acquiring focus'.\n" +
					   "If it does, the issue is fixed.\n" +
					   "If it does not, check if the text appears if the entry receives focus.\n" +
					   "If it does, the bug is not fixed."
			};
			isolatedEntry = new Entry { Text = "This text should be visible, even before acquiring focus", IsVisible = false };

			Button isolatedShowEntryButton = new Button { Text = "Click to show Entry for Isolated Bug" };
			isolatedShowEntryButton.Clicked += IsolatedShowEntryButton_Clicked;

			layout.Children.Add(isolatedHeader);
			layout.Children.Add(isolatedInstructions);
			layout.Children.Add(isolatedEntry);
			layout.Children.Add(isolatedShowEntryButton);

			Label issueHeader = new Label { Text = "Reported Bug", Margin = new Thickness(0, 20), FontSize = 18 };
			Label issueInstructions = new Label
			{
				Text = "This is the bug as reported with an example\n" +
					   "Click on the login-action button.\n" +
					   "An Entry for a UserName appears with a little translating animation.\n" +
					   "Check that the entry contains text 'user name goes here'.\n" +
					   "If it does, the issue is fixed.\n" +
					   "If it does not, check if the text appears if the entry receives focus.\n" +
					   "If it does, the bug is there."
			};

			LoginAnimateBehavior bugBehavior = new LoginAnimateBehavior
			{
				EasingType = EEasingType.SinInOut,
				AnimationType = EAnimationType.Translation
			};

			StackLayout issueStackLayout = new StackLayout();
			LoginAnimationStackLayout animatedStackLayout = new LoginAnimationStackLayout
			{
				IsVisible = false
			};
			animatedStackLayout.SetBinding(LoginAnimationStackLayout.VisibleProperty, nameof(IsUserNameEntryVisible));
			animatedStackLayout.Behaviors.Add(bugBehavior);

			Entry loginUserNameEntry = new Entry
			{
				IsEnabled = true
			};
			loginUserNameEntry.SetBinding(Entry.TextProperty, nameof(UserName));

			Button loginButton = new Button
			{
				Text = "Login_Action",
				WidthRequest = 150
			};
			loginButton.Clicked += LoginButton_Clicked;

			animatedStackLayout.Children.Add(loginUserNameEntry);

			issueStackLayout.Children.Add(animatedStackLayout);
			issueStackLayout.Children.Add(loginButton);

			layout.Children.Add(issueHeader);
			layout.Children.Add(issueInstructions);
			layout.Children.Add(issueStackLayout);

			scrollView.Content = layout;
			Content = scrollView;
		}

		private void IsolatedShowEntryButton_Clicked(object sender, EventArgs e)
		{
			isolatedEntry.IsVisible = true;
		}

		bool isUserNameEntryVisible;
		public bool IsUserNameEntryVisible
		{
			get
			{
				return isUserNameEntryVisible;
			}
			set
			{
				isUserNameEntryVisible = value;
				OnPropertyChanged(nameof(IsUserNameEntryVisible));
			}
		}

		string userName;
		public string UserName
		{
			get
			{
				return userName;
			}
			set
			{
				userName = value;
				OnPropertyChanged(nameof(UserName));
			}
		}


		private void LoginButton_Clicked(object sender, EventArgs e)
		{
			UserName = "username goes here";
			IsUserNameEntryVisible = true;
		}

		public class LoginAnimationStackLayout : StackLayout
		{
			public static readonly BindableProperty VisibleProperty = BindableProperty.Create(nameof(Visible), typeof(bool), typeof(LoginAnimationStackLayout), defaultBindingMode: BindingMode.TwoWay, defaultValue: false, propertyChanged: (b, o, n) => ((LoginAnimationStackLayout)b).OnVisibileChanged((bool)o, (bool)n));

			public bool Visible
			{
				get
				{
					var obj = GetValue(VisibleProperty);
					if (obj == null)
					{
						return false;
					}

					return (bool)obj;
				}
				set
				{
					SetValue(VisibleProperty, value);
				}
			}

			private void OnVisibileChanged(bool oldvalue, bool newvalue)
			{
				Visible = newvalue;
			}
		}

		public class LoginAnimateBehavior : BaseAnimationBehavior
		{
			private static double? yPosition;

			protected override async void AnimateItem(object sender, EventArgs e)
			{
				PropertyChangedEventArgs prop = e as PropertyChangedEventArgs;

				if (prop == null || !prop.PropertyName.Equals(nameof(LoginAnimationStackLayout.Visible), StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				await DoAnimationByType(AnimationType, associatedObject, ToEasing(EasingType), Scale);
			}

			private static async Task DoAnimationByType(EAnimationType animationType, View obj, Easing easing, double scale)
			{
				LoginAnimationStackLayout stack = obj as LoginAnimationStackLayout;
				if (stack == null)
				{
					return;
				}

				switch (animationType)
				{
					case EAnimationType.Translation:
						await DoTranslation(stack, obj, easing);
						break;

					case EAnimationType.Scale:
						await DoScale(stack, easing, scale);
						break;

					default:
						stack.IsVisible = stack.Visible;
						break;
				}
			}

			private static async Task DoTranslation(LoginAnimationStackLayout stackLayout, View obj, Easing easing)
			{
				if (stackLayout.Visible)
				{
					stackLayout.IsVisible = true;

					if (!yPosition.HasValue)
					{
						yPosition = obj.TranslationY;
					}

					await stackLayout.TranslateTo(obj.TranslationX, obj.TranslationY - 10, 10, easing);
					await stackLayout.TranslateTo(obj.TranslationX, obj.TranslationY + 10, 900, easing);
				}
				else
				{
					if (!yPosition.HasValue)
					{
						yPosition = obj.TranslationY;
					}

					await stackLayout.TranslateTo(obj.TranslationX, obj.TranslationY, 10, easing);
					await stackLayout.TranslateTo(obj.TranslationX, obj.TranslationY - 20, 900, easing);

					obj.TranslationY = yPosition.Value;

					stackLayout.IsVisible = false;
				}
			}

			private static async Task DoScale(LoginAnimationStackLayout stackLayout, Easing easing, double scale)
			{
				if (stackLayout.Visible)
				{
					stackLayout.IsVisible = true;

					await stackLayout.ScaleTo(scale, 250, easing);
					await stackLayout.ScaleTo(1.00, 250, easing);
				}
				else
				{
					await stackLayout.ScaleTo(1.00, 250, easing);
					await stackLayout.ScaleTo(scale, 250, easing);

					stackLayout.IsVisible = false;
				}
			}
		}

		public abstract class BaseAnimationBehavior : Behavior<View>
		{
			protected View associatedObject;

			public static readonly BindableProperty EasingTypeProperty = BindableProperty.Create(nameof(EasingType), typeof(EEasingType), typeof(BaseAnimationBehavior), defaultValue: EEasingType.Linear, propertyChanged: (b, o, n) => OnEasingFunctionChanged(b, (EEasingType)o, (EEasingType)n));
			public static readonly BindableProperty ScaleProperty = BindableProperty.Create(nameof(Scale), typeof(double), typeof(BaseAnimationBehavior), defaultValue: 1.25);
			public static readonly BindableProperty AnimationTypeProperty = BindableProperty.Create(nameof(AnimationType), typeof(EAnimationType), typeof(BaseAnimationBehavior), defaultValue: default(EAnimationType));

			public EEasingType EasingType
			{
				get { return (EEasingType)GetValue(EasingTypeProperty); }
				set { SetValue(EasingTypeProperty, value); }
			}

			public double Scale
			{
				get { return (double)GetValue(ScaleProperty); }
				set { SetValue(ScaleProperty, value); }
			}

			public EAnimationType AnimationType
			{
				get
				{
					EAnimationType animationType = default(EAnimationType);
					string data = GetValue(AnimationTypeProperty)?.ToString();

					Enum.TryParse(data, out animationType);

					return animationType;
				}
				set { SetValue(AnimationTypeProperty, value); }
			}

			protected override void OnAttachedTo(View bindable)
			{
				associatedObject = bindable;
				associatedObject.PropertyChanged += AnimateItem;
			}

			protected override void OnDetachingFrom(View bindable)
			{
				associatedObject.PropertyChanged -= AnimateItem;
			}

			protected static Easing ToEasing(EEasingType easingType)
			{
				switch (easingType)
				{
					case EEasingType.BounceIn:
						return Easing.BounceIn;
					case EEasingType.BounceOut:
						return Easing.BounceOut;
					case EEasingType.CubicIn:
						return Easing.CubicIn;
					case EEasingType.CubicInOut:
						return Easing.CubicInOut;
					case EEasingType.CubicOut:
						return Easing.CubicOut;
					case EEasingType.Linear:
						return Easing.Linear;
					case EEasingType.SinIn:
						return Easing.SinIn;
					case EEasingType.SinInOut:
						return Easing.SinInOut;
					case EEasingType.SinOut:
						return Easing.SinOut;
					case EEasingType.SpringIn:
						return Easing.SpringIn;
					case EEasingType.SpringOut:
						return Easing.SpringOut;
					default:
						return Easing.Linear;
				}
			}

			private static void OnEasingFunctionChanged(BindableObject bindable, EEasingType oldvalue, EEasingType newvalue)
			{
				var obj = bindable as BaseAnimationBehavior;
				if (obj == null)
				{
					return;
				}

				obj.EasingType = newvalue;
			}

			protected abstract void AnimateItem(object sender, EventArgs e);
		}

		public enum EAnimationType
		{
			None,
			Translation,
			Rotate,
			Scale
		}

		public enum EEasingType
		{
			Linear,
			SinOut,
			SinIn,
			SinInOut,
			CubicIn,
			CubicOut,
			CubicInOut,
			BounceOut,
			BounceIn,
			SpringIn,
			SpringOut,
		}
	}
}
