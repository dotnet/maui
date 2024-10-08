using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui24949 : ContentPage
{
    public Maui24949()
    {
        InitializeComponent();
    }

    public Maui24949(bool useCompiledXaml)
    {
        //this stub will be replaced at compile time
    }



    [TestFixture]
    class Test
    {
        [SetUp]
        public void Setup()
        {
            Application.SetCurrentApplication(new MockApplication());
            DispatcherProvider.SetCurrent(new DispatcherProviderStub());
        }


        [TearDown] public void TearDown()
        {
            AppInfo.SetCurrent(null);
            DeviceInfo.SetCurrent(null);
        }

        [Test]
        public void TemplateBinding([Values(false, true)] bool useCompiledXaml)
        {
            var page = new Maui24949(useCompiledXaml);
            Assert.That(page.button.Content, Is.TypeOf<Label>());
        }
    }
}

[DefaultProperty( "Content" )]
[ContentProperty( "Content" )]
public class Maui24949Button : TemplatedView
{
    public static readonly BindableProperty ContentProperty = BindableProperty.Create( nameof( Content ), typeof( View ), typeof( Maui24949Button ) );

    public View Content
    {
        get => (View)GetValue( ContentProperty );
        set => SetValue( ContentProperty, value );
    }
}

[DefaultProperty( "Content" )]
[ContentProperty( "Content" )]
public class Maui24949Border : TemplatedView
{
    private ContentPresenter m_contentPresenter;

    public Maui24949Border()
    {
        m_contentPresenter = new ContentPresenter();
        this.ControlTemplate = new ControlTemplate( () => m_contentPresenter );
    }

    public static readonly BindableProperty ContentProperty = BindableProperty.Create( nameof( Content ), typeof( View ), typeof( Maui24949Border ) );

    public View Content
    {
        get => (View)GetValue( ContentProperty );
        set => SetValue( ContentProperty, value );
    }
}