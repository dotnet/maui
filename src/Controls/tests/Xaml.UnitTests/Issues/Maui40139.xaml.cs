#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui40139 : ContentPage
{
	AViewModel _a = new();

	public AViewModel A
	{
		get => _a;
		set
		{
			_a = value;
			OnPropertyChanged();
		}
	}

	public Maui40139() => InitializeComponent();

	public class AViewModel : BindableObject
	{
		BViewModel _b = null!;

		public BViewModel B
		{
			get => _b;
			set
			{
				_b = value;
				OnPropertyChanged();
			}
		}
	}

	public class BViewModel : BindableObject
	{
		CViewModel _c = null!;

		public CViewModel C
		{
			get => _c;
			set
			{
				_c = value;
				OnPropertyChanged();
			}
		}
	}

	public class CViewModel : BindableObject
	{
		string _text = "Hello World!!";

		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				OnPropertyChanged();
			}
		}
	}

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void NullIntermediatesDoNotThrowNullReferenceExceptions(XamlInflator inflator)
		{
			var page = new Maui40139(inflator);
			var exceptions = new List<Exception>();
			var threadId = Environment.CurrentManagedThreadId;
			void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs args)
			{
				if (Environment.CurrentManagedThreadId == threadId && args.Exception is NullReferenceException)
					exceptions.Add(args.Exception);
			}

			// TypedBinding catches getter exceptions, but they still interrupt the debugger.
			AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
			try
			{
				page.BindingContext = page;
				page.A.B = new BViewModel();
				page.A.B.C = new CViewModel();
				page.A.B.C = null!;
				page.A.B = null!;
				page.A = null!;
			}
			finally
			{
				AppDomain.CurrentDomain.FirstChanceException -= OnFirstChanceException;
			}

			Assert.Empty(exceptions);
		}

		[Theory]
		[XamlInflatorData]
		internal void NullIntermediatesAreToleratedAndBindingsRecover(XamlInflator inflator)
		{
			// Non-nullable annotations do not prevent intermediate references being null at runtime.
			var page = new Maui40139(inflator);
			page.BindingContext = page;
			Assert.Null(page.label.Text);

			page.A.B = new BViewModel();
			Assert.Null(page.label.Text);

			page.A.B.C = new CViewModel();
			Assert.Equal("Hello World!!", page.label.Text);

			page.A.B.C.Text = "Updated";
			Assert.Equal("Updated", page.label.Text);

			var detached = page.A.B.C;
			page.A.B.C = null!;
			Assert.Null(page.label.Text);
			detached.Text = "Detached";
			Assert.Null(page.label.Text);

			page.A.B.C = new CViewModel { Text = "C restored" };
			Assert.Equal("C restored", page.label.Text);

			page.A.B = null!;
			Assert.Null(page.label.Text);
			page.A.B = new BViewModel { C = new CViewModel { Text = "B restored" } };
			Assert.Equal("B restored", page.label.Text);

			page.A = null!;
			Assert.Null(page.label.Text);
			page.A = new AViewModel { B = new BViewModel { C = new CViewModel { Text = "A restored" } } };
			Assert.Equal("A restored", page.label.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void TwoWayBindingToleratesNullIntermediatesAndStillWritesNull(XamlInflator inflator)
		{
			var page = new Maui40139(inflator);
			page.BindingContext = page;
			page.entry.Text = "No destination";

			page.A.B = new BViewModel { C = new CViewModel() };
			Assert.Equal("Hello World!!", page.entry.Text);

			page.entry.Text = "From target";
			Assert.Equal("From target", page.A.B.C.Text);
			Assert.Equal("From target", page.label.Text);

			page.entry.Text = null;
			Assert.Null(page.A.B.C.Text);
			Assert.Null(page.label.Text);
		}
	}
}
