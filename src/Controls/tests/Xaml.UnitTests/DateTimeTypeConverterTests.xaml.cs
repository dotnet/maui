#if NET6_0_OR_GREATER
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
    public partial class DateTimeTypeConverterTests : ContentPage
    {
        public DateTimeTypeConverterTests()
        {
            InitializeComponent();
        }

        public DateTimeTypeConverterTests(bool useCompiledXaml)
        {
            //this stub will be replaced at compile time
        }

        [TestFixture]
        public class Tests
        {
            [Test]
            public void DateTimeTypeConverter_ConvertsDateStringInCompiledXaml()
            {
                var layout = new DateTimeTypeConverterTests(useCompiledXaml: true);

                Assert.That(layout.datePicker.Date, Is.Not.Null);
                Assert.That(layout.datePicker.Date?.Year, Is.EqualTo(2025));
                Assert.That(layout.datePicker.Date?.Month, Is.EqualTo(7));
                Assert.That(layout.datePicker.Date?.Day, Is.EqualTo(23));
            }

            [Test]
            public void TimeSpanTypeConverter_ConvertsTimeStringInCompiledXaml()
            {
                var layout = new DateTimeTypeConverterTests(useCompiledXaml: true);

                Assert.That(layout.timePicker.Time, Is.Not.Null);
                Assert.That(layout.timePicker.Time?.Hours, Is.EqualTo(14));
                Assert.That(layout.timePicker.Time?.Minutes, Is.EqualTo(30));
                Assert.That(layout.timePicker.Time?.Seconds, Is.EqualTo(0));
            }
        }
    }
}
#endif
