#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.UnitTests;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public abstract class BindingBaseUnitTests : BaseTestFixture
    {
        protected abstract BindingBase CreateBinding(BindingMode mode = BindingMode.Default, string stringFormat = null);

        internal class ComplexMockViewModel : MockViewModel
        {
            public ComplexMockViewModel Model
            {
                get { return model; }
                set
                {
                    if (model == value)
                        return;

                    model = value;
                    OnPropertyChanged("Model");
                }
            }

            internal int count;
            public int QueryCount
            {
                get { return count++; }
            }

            [IndexerName("Indexer")]
            public string this[int v]
            {
                get { return values[v]; }
                set
                {
                    if (values[v] == value)
                        return;

                    values[v] = value;
                    OnPropertyChanged("Indexer[" + v + "]");
                }
            }

            public string[] Array
            {
                get;
                set;
            }

            public object DoStuff()
            {
                return null;
            }

            public object DoStuff(object argument)
            {
                return null;
            }

            string[] values = new string[5];
            ComplexMockViewModel model;
        }

        public BindingBaseUnitTests()
        {
            ApplicationExtensions.CreateAndSetMockApplication();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Application.Current = null;
            }

            base.Dispose(disposing);
        }

        [Fact]
        public void CloneMode()
        {
            var binding = CreateBinding(BindingMode.Default);
            var clone = binding.Clone();

            Assert.Equal(binding.Mode, clone.Mode);
        }

        [Fact]
        public void StringFormat()
        {
            var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
            var binding = CreateBinding(BindingMode.Default, "Foo {0}");

            var vm = new MockViewModel { Text = "Bar" };
            var bo = new MockBindable { BindingContext = vm };
            bo.SetBinding(property, binding);

            Assert.Equal("Foo Bar", bo.GetValue(property));
        }

        [Fact]
        public void StringFormatOnUpdate()
        {
            var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
            var binding = CreateBinding(BindingMode.Default, "Foo {0}");

            var vm = new MockViewModel { Text = "Bar" };
            var bo = new MockBindable { BindingContext = vm };
            bo.SetBinding(property, binding);

            vm.Text = "Baz";

            Assert.Equal("Foo Baz", bo.GetValue(property));
        }

        [Fact("StringFormat should not be applied to OneWayToSource bindings")]
        public void StringFormatOneWayToSource()
        {
            var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
            var binding = CreateBinding(BindingMode.OneWayToSource, "Foo {0}");

            var vm = new MockViewModel { Text = "Bar" };
            var bo = new MockBindable { BindingContext = vm };
            bo.SetBinding(property, binding);

            bo.SetValue(property, "Bar");

            Assert.Equal("Bar", vm.Text);
        }

        [Fact("StringFormat should only be applied from from source in TwoWay bindings")]
        public void StringFormatTwoWay()
        {
            var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
            var binding = CreateBinding(BindingMode.TwoWay, "Foo {0}");

            var vm = new MockViewModel { Text = "Bar" };
            var bo = new MockBindable { BindingContext = vm };
            bo.SetBinding(property, binding);

            bo.SetValue(property, "Baz", SetterSpecificity.FromHandler);

            Assert.Equal("Baz", vm.Text);
            Assert.Equal("Foo Baz", bo.GetValue(property));
        }

        [Fact("You should get an exception when trying to change a binding after it's been applied")]
        public void ChangeAfterApply()
        {
            var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
            var binding = CreateBinding(BindingMode.OneWay);

            var vm = new MockViewModel { Text = "Bar" };
            var bo = new MockBindable { BindingContext = vm };
            bo.SetBinding(property, binding);

            Assert.Throws<InvalidOperationException>(() => binding.Mode = BindingMode.OneWayToSource);
            Assert.Throws<InvalidOperationException>(() => binding.StringFormat = "{0}");
        }

        [Theory, InlineData("en-US"), InlineData("tr-TR")]
        public void StringFormatNonStringType(string culture)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

            var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
            var binding = new Binding("Value", stringFormat: "{0:P2}");

            var vm = new { Value = 0.95d };
            var bo = new MockBindable { BindingContext = vm };
            bo.SetBinding(property, binding);

            Assert.Equal(bo.GetValue(property), string.Format(new System.Globalization.CultureInfo(culture), "{0:P2}", .95d)); //%95,00 or 95.00%
        }

        [Fact]
        public void ReuseBindingInstance()
        {
            var vm = new MockViewModel();

            var bindable = new MockBindable();
            bindable.BindingContext = vm;

            var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
            var binding = new Binding("Text");
            bindable.SetBinding(property, binding);

            var bindable2 = new MockBindable();
            bindable2.BindingContext = new MockViewModel();
            Assert.Throws<InvalidOperationException>(() => bindable2.SetBinding(property, binding));

            GC.KeepAlive(bindable);
        }

        [MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
        [Theory, Category("[Binding] Set Value")]
        public void ValueSetOnOneWay(
            bool setContextFirst,
            bool isDefault)
        {
            const string value = "Foo";
            var viewmodel = new MockViewModel
            {
                Text = value
            };

            BindingMode propertyDefault = BindingMode.OneWay;
            BindingMode bindingMode = BindingMode.OneWay;
            if (isDefault)
            {
                propertyDefault = BindingMode.OneWay;
                bindingMode = BindingMode.Default;
            }

            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), null, propertyDefault);
            var binding = CreateBinding(bindingMode);

            var bindable = new MockBindable();
            if (setContextFirst)
            {
                bindable.BindingContext = viewmodel;
                bindable.SetBinding(property, binding);
            }
            else
            {
                bindable.SetBinding(property, binding);
                bindable.BindingContext = viewmodel;
            }

            Assert.True(value == viewmodel.Text,
                "BindingContext property changed");
            Assert.True(value == (string)bindable.GetValue(property),
                "Target property did not change");
            var messages = MockApplication.MockLogger.Messages;
            Assert.True(messages.Count == 0, "An error was logged: " + messages.FirstOrDefault());
        }

        [Theory, Category("[Binding] Set Value")]
        [MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
        public void ValueSetOnOneWayToSource(
            bool setContextFirst,
            bool isDefault)
        {
            const string value = "Foo";
            var viewmodel = new MockViewModel();

            BindingMode propertyDefault = BindingMode.OneWay;
            BindingMode bindingMode = BindingMode.OneWayToSource;
            if (isDefault)
            {
                propertyDefault = BindingMode.OneWayToSource;
                bindingMode = BindingMode.Default;
            }

            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), defaultValue: value, defaultBindingMode: propertyDefault);
            var binding = CreateBinding(bindingMode);

            var bindable = new MockBindable();
            if (setContextFirst)
            {
                bindable.BindingContext = viewmodel;
                bindable.SetBinding(property, binding);
            }
            else
            {
                bindable.SetBinding(property, binding);
                bindable.BindingContext = viewmodel;
            }

            Assert.True(value == (string)bindable.GetValue(property),
                "Target property changed");
            Assert.True(value == viewmodel.Text,
                "BindingContext property did not change");
            var messages = MockApplication.MockLogger.Messages;
            Assert.True(messages.Count == 0,
                "An error was logged: " + messages.FirstOrDefault());
        }

        [Theory, Category("[Binding] Set Value")]
        [MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
        public void ValueSetOnTwoWay(
            bool setContextFirst,
            bool isDefault)
        {
            const string value = "Foo";
            var viewmodel = new MockViewModel
            {
                Text = value
            };

            BindingMode propertyDefault = BindingMode.OneWay;
            BindingMode bindingMode = BindingMode.TwoWay;
            if (isDefault)
            {
                propertyDefault = BindingMode.TwoWay;
                bindingMode = BindingMode.Default;
            }

            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), defaultValue: "default value", defaultBindingMode: propertyDefault);
            var binding = CreateBinding(bindingMode);

            var bindable = new MockBindable();
            if (setContextFirst)
            {
                bindable.BindingContext = viewmodel;
                bindable.SetBinding(property, binding);
            }
            else
            {
                bindable.SetBinding(property, binding);
                bindable.BindingContext = viewmodel;
            }

            Assert.True(value == viewmodel.Text,
                "BindingContext property changed");
            Assert.True(value == (string)bindable.GetValue(property),
                "Target property did not change");
            var messages = MockApplication.MockLogger.Messages;
            Assert.True(messages.Count == 0,
                "An error was logged: " + messages.FirstOrDefault());
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory, Category("[Binding] Update Value")]
        public void ValueUpdatedWithSimplePathOnOneWayBinding(
            bool isDefault)
        {
            const string newvalue = "New Value";
            var viewmodel = new MockViewModel
            {
                Text = "Foo"
            };

            BindingMode propertyDefault = BindingMode.OneWay;
            BindingMode bindingMode = BindingMode.OneWay;
            if (isDefault)
            {
                propertyDefault = BindingMode.OneWay;
                bindingMode = BindingMode.Default;
            }

            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
            var binding = CreateBinding(bindingMode);

            var bindable = new MockBindable();
            bindable.BindingContext = viewmodel;
            bindable.SetBinding(property, binding);

            viewmodel.Text = newvalue;
            Assert.True(newvalue == (string)bindable.GetValue(property),
                "Bindable did not update on binding context property change");
            Assert.True(newvalue == viewmodel.Text,
                "Source property changed when it shouldn't");
            var messages = MockApplication.MockLogger.Messages;
            Assert.True(messages.Count == 0,
                "An error was logged: " + messages.FirstOrDefault());
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory, Category("[Binding] Update Value")]
        public void ValueUpdatedWithSimplePathOnOneWayToSourceBinding(
            bool isDefault)

        {
            const string newvalue = "New Value";
            var viewmodel = new MockViewModel
            {
                Text = "Foo"
            };

            BindingMode propertyDefault = BindingMode.OneWay;
            BindingMode bindingMode = BindingMode.OneWayToSource;
            if (isDefault)
            {
                propertyDefault = BindingMode.OneWayToSource;
                bindingMode = BindingMode.Default;
            }

            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
            var binding = CreateBinding(bindingMode);

            var bindable = new MockBindable();
            bindable.BindingContext = viewmodel;
            bindable.SetBinding(property, binding);

            string original = (string)bindable.GetValue(property);
            const string value = "value";
            viewmodel.Text = value;
            Assert.True(original == (string)bindable.GetValue(property),
                "Target updated from Source on OneWayToSource");

            bindable.SetValue(property, newvalue);
            Assert.True(newvalue == (string)bindable.GetValue(property),
                "Bindable did not update on binding context property change");
            Assert.True(newvalue == viewmodel.Text,
                "Source property changed when it shouldn't");

            var messages = MockApplication.MockLogger.Messages;
            Assert.True(messages.Count == 0,
                "An error was logged: " + messages.FirstOrDefault());
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory, Category("[Binding] Update Value")]
        public void ValueUpdatedWithSimplePathOnTwoWayBinding(
            bool isDefault)
        {
            const string newvalue = "New Value";
            var viewmodel = new MockViewModel
            {
                Text = "Foo"
            };

            BindingMode propertyDefault = BindingMode.OneWay;
            BindingMode bindingMode = BindingMode.TwoWay;
            if (isDefault)
            {
                propertyDefault = BindingMode.TwoWay;
                bindingMode = BindingMode.Default;
            }

            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
            var binding = CreateBinding(bindingMode);

            var bindable = new MockBindable();
            bindable.BindingContext = viewmodel;
            bindable.SetBinding(property, binding);

            viewmodel.Text = newvalue;
            Assert.True(newvalue == (string)bindable.GetValue(property),
                "Target property did not update change");
            Assert.True(newvalue == viewmodel.Text,
                "Source property changed from what it was set to");

            const string newvalue2 = "New Value in the other direction";

            bindable.SetValue(property, newvalue2);
            Assert.True(newvalue2 == viewmodel.Text,
                "Source property did not update with Target's change");
            Assert.True(newvalue2 == (string)bindable.GetValue(property),
                "Target property changed from what it was set to");
            var messages = MockApplication.MockLogger.Messages;
            Assert.True(messages.Count == 0,
                "An error was logged: " + messages.FirstOrDefault());
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void ValueUpdatedWithOldContextDoesNotUpdateWithOneWayBinding(bool isDefault)
        {
            const string newvalue = "New Value";
            var viewmodel = new MockViewModel
            {
                Text = "Foo"
            };

            BindingMode propertyDefault = BindingMode.OneWay;
            BindingMode bindingMode = BindingMode.OneWay;
            if (isDefault)
            {
                propertyDefault = BindingMode.OneWay;
                bindingMode = BindingMode.Default;
            }

            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
            var binding = CreateBinding(bindingMode);

            var bindable = new MockBindable();
            bindable.BindingContext = viewmodel;
            bindable.SetBinding(property, binding);

            bindable.BindingContext = new MockViewModel();
            Assert.Null(bindable.GetValue(property));

            viewmodel.Text = newvalue;
            Assert.True(null == bindable.GetValue(property),
                "Target updated from old Source property change");
            var messages = MockApplication.MockLogger.Messages;
            Assert.True(messages.Count == 0,
                "An error was logged: " + messages.FirstOrDefault());
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void ValueUpdatedWithOldContextDoesNotUpdateWithTwoWayBinding(bool isDefault)
        {
            const string newvalue = "New Value";
            var viewmodel = new MockViewModel
            {
                Text = "Foo"
            };

            BindingMode propertyDefault = BindingMode.OneWay;
            BindingMode bindingMode = BindingMode.TwoWay;
            if (isDefault)
            {
                propertyDefault = BindingMode.TwoWay;
                bindingMode = BindingMode.Default;
            }

            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
            var binding = CreateBinding(bindingMode);

            var bindable = new MockBindable();
            bindable.BindingContext = viewmodel;
            bindable.SetBinding(property, binding);

            bindable.BindingContext = new MockViewModel();
            Assert.Null(bindable.GetValue(property));

            viewmodel.Text = newvalue;
            Assert.True(null == bindable.GetValue(property),
                "Target updated from old Source property change");

            string original = viewmodel.Text;

            bindable.SetValue(property, newvalue);
            Assert.True(original == viewmodel.Text,
                "Source updated from old Target property change");
            var messages = MockApplication.MockLogger.Messages;
            Assert.True(messages.Count == 0,
                "An error was logged: " + messages.FirstOrDefault());
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void ValueUpdatedWithOldContextDoesNotUpdateWithOneWayToSourceBinding(bool isDefault)
        {
            const string newvalue = "New Value";
            var viewmodel = new MockViewModel
            {
                Text = "Foo"
            };

            BindingMode propertyDefault = BindingMode.OneWay;
            BindingMode bindingMode = BindingMode.OneWayToSource;
            if (isDefault)
            {
                propertyDefault = BindingMode.OneWayToSource;
                bindingMode = BindingMode.Default;
            }

            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
            var binding = CreateBinding(bindingMode);

            var bindable = new MockBindable();
            bindable.BindingContext = viewmodel;
            bindable.SetBinding(property, binding);

            bindable.BindingContext = new MockViewModel();
            Assert.Equal(property.DefaultValue, bindable.GetValue(property));

            viewmodel.Text = newvalue;
            Assert.True(property.DefaultValue == bindable.GetValue(property),
                "Target updated from old Source property change");
            var messages = MockApplication.MockLogger.Messages;
            Assert.True(messages.Count == 0,
                "An error was logged: " + messages.FirstOrDefault());
        }

        [Fact]
        public void BindingStaysOnUpdateValueFromBinding()
        {
            const string newvalue = "New Value";
            var viewmodel = new MockViewModel
            {
                Text = "Foo"
            };

            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), null);
            var binding = CreateBinding(BindingMode.Default);

            var bindable = new MockBindable();
            bindable.BindingContext = viewmodel;
            bindable.SetBinding(property, binding);

            viewmodel.Text = newvalue;
            Assert.Equal(newvalue, bindable.GetValue(property));

            const string newValue2 = "new value 2";
            viewmodel.Text = newValue2;
            Assert.Equal(newValue2, bindable.GetValue(property));
            var messages = MockApplication.MockLogger.Messages;
            Assert.True(messages.Count == 0,
                "An error was logged: " + messages.FirstOrDefault());
        }

        [Fact]
        public void OneWayToSourceContextSetToNull()
        {
            var binding = new Binding("Text", BindingMode.OneWayToSource);

            MockBindable bindable = new MockBindable
            {
                BindingContext = new MockViewModel()
            };
            bindable.SetBinding(MockBindable.TextProperty, binding);

            bindable.BindingContext = null;
        }

        [Theory, Category("[Binding] Simple paths")]
        [InlineData(BindingMode.OneWay)]
        [InlineData(BindingMode.OneWayToSource)]
        [InlineData(BindingMode.TwoWay)]
        public void SourceAndTargetAreWeakWeakSimplePath(BindingMode mode)
        {
            var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", BindingMode.OneWay);
            var binding = CreateBinding(mode);

            WeakReference weakViewModel = null, weakBindable = null;

            int i = 0;
            Action create = null;
            create = () =>
            {
                if (i++ < 1024)
                {
                    create();
                    return;
                }

                MockBindable bindable = new MockBindable();
                weakBindable = new WeakReference(bindable);

                MockViewModel viewmodel = new MockViewModel();
                weakViewModel = new WeakReference(viewmodel);

                bindable.BindingContext = viewmodel;
                bindable.SetBinding(property, binding);

                bindable.BindingContext = null;
            };

            create();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (mode == BindingMode.TwoWay || mode == BindingMode.OneWay)
                Assert.False(weakViewModel.IsAlive, "ViewModel wasn't collected");

            if (mode == BindingMode.TwoWay || mode == BindingMode.OneWayToSource)
                Assert.False(weakBindable.IsAlive, "Bindable wasn't collected");
        }

        [Fact]
        public Task PropertyChangeBindingsOccurThroughMainThread() => DispatcherTest.Run(async () =>
        {
            var isOnBackgroundThread = false;
            var invokeOnMainThreadWasCalled = false;

            DispatcherProviderStubOptions.InvokeOnMainThread = action =>
            {
                invokeOnMainThreadWasCalled = true;
                action();
            };
            DispatcherProviderStubOptions.IsInvokeRequired =
                () => isOnBackgroundThread;

            var vm = new MockViewModel { Text = "text" };

            var bindable = new MockBindable();
            var binding = CreateBinding();
            bindable.BindingContext = vm;
            bindable.SetBinding(MockBindable.TextProperty, binding);

            Assert.False(invokeOnMainThreadWasCalled);

            isOnBackgroundThread = true;

            vm.Text = "updated";

            Assert.True(invokeOnMainThreadWasCalled);
        });
    }



    public partial class BindingBaseTryFormatTests
    {
        /// <summary>
        /// Tests TryFormat with valid format string and matching arguments.
        /// Expects successful formatting and return value of true.
        /// </summary>
        [Theory]
        [InlineData("Hello {0}", new object[] { "World" }, "Hello World")]
        [InlineData("Number: {0}", new object[] { 42 }, "Number: 42")]
        [InlineData("Value: {0:C}", new object[] { 123.45 }, "Value: $123.45")]
        [InlineData("Multiple: {0} {1}", new object[] { "A", "B" }, "Multiple: A B")]
        [InlineData("No placeholders", new object[] { }, "No placeholders")]
        [InlineData("", new object[] { }, "")]
        public void TryFormat_ValidFormatAndArgs_ReturnsTrue(string format, object[] args, string expected)
        {
            // Arrange & Act
            bool result = BindingBase.TryFormat(format, args, out string value);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, value);
        }

        /// <summary>
        /// Tests TryFormat with null format string.
        /// Expects FormatException to be caught, value set to null, and return false.
        /// </summary>
        [Fact]
        public void TryFormat_NullFormat_ReturnsFalse()
        {
            // Arrange & Act
            bool result = BindingBase.TryFormat(null, new object[] { "test" }, out string value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryFormat with null args array.
        /// Expects FormatException to be caught, value set to null, and return false.
        /// </summary>
        [Fact]
        public void TryFormat_NullArgs_ReturnsFalse()
        {
            // Arrange & Act
            bool result = BindingBase.TryFormat("{0}", null, out string value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryFormat with format string referencing more arguments than provided.
        /// Expects FormatException to be caught, value set to null, and return false.
        /// </summary>
        [Theory]
        [InlineData("{0} {1}", new object[] { "only_one" })]
        [InlineData("{0} {1} {2}", new object[] { "one", "two" })]
        [InlineData("{5}", new object[] { "zero", "one", "two" })]
        public void TryFormat_InsufficientArgs_ReturnsFalse(string format, object[] args)
        {
            // Arrange & Act
            bool result = BindingBase.TryFormat(format, args, out string value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryFormat with invalid format specifiers.
        /// Expects FormatException to be caught, value set to null, and return false.
        /// </summary>
        [Theory]
        [InlineData("Invalid {", new object[] { "test" })]
        [InlineData("Invalid }", new object[] { "test" })]
        [InlineData("Invalid {0", new object[] { "test" })]
        [InlineData("Invalid 0}", new object[] { "test" })]
        [InlineData("Invalid {-1}", new object[] { "test" })]
        [InlineData("Invalid {abc}", new object[] { "test" })]
        public void TryFormat_InvalidFormatSpecifiers_ReturnsFalse(string format, object[] args)
        {
            // Arrange & Act
            bool result = BindingBase.TryFormat(format, args, out string value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryFormat with args containing null values.
        /// Expects successful formatting with null values converted to empty strings.
        /// </summary>
        [Theory]
        [InlineData("{0}", new object[] { null }, "")]
        [InlineData("{0} {1}", new object[] { "test", null }, "test ")]
        [InlineData("{0} {1} {2}", new object[] { null, "middle", null }, " middle ")]
        public void TryFormat_ArgsWithNullValues_ReturnsTrue(string format, object[] args, string expected)
        {
            // Arrange & Act
            bool result = BindingBase.TryFormat(format, args, out string value);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, value);
        }

        /// <summary>
        /// Tests TryFormat with empty args array but format requiring arguments.
        /// Expects FormatException to be caught, value set to null, and return false.
        /// </summary>
        [Theory]
        [InlineData("{0}")]
        [InlineData("Hello {0}")]
        [InlineData("{0} and {1}")]
        public void TryFormat_EmptyArgsWithPlaceholders_ReturnsFalse(string format)
        {
            // Arrange & Act
            bool result = BindingBase.TryFormat(format, new object[] { }, out string value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryFormat with extremely large array to test boundary conditions.
        /// Expects successful formatting and return value of true.
        /// </summary>
        [Fact]
        public void TryFormat_LargeArgsArray_ReturnsTrue()
        {
            // Arrange
            var args = new object[100];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = i.ToString();
            }
            string format = "{0} {50} {99}";

            // Act
            bool result = BindingBase.TryFormat(format, args, out string value);

            // Assert
            Assert.True(result);
            Assert.Equal("0 50 99", value);
        }

        /// <summary>
        /// Tests TryFormat with various special characters and escape sequences.
        /// Expects successful formatting and return value of true.
        /// </summary>
        [Theory]
        [InlineData("{{0}}", new object[] { "test" }, "{0}")]
        [InlineData("Escaped: {{{0}}}", new object[] { "value" }, "Escaped: {value}")]
        [InlineData("Unicode: {0}", new object[] { "🌟" }, "Unicode: 🌟")]
        [InlineData("Newline: {0}\n{1}", new object[] { "line1", "line2" }, "Newline: line1\nline2")]
        public void TryFormat_SpecialCharacters_ReturnsTrue(string format, object[] args, string expected)
        {
            // Arrange & Act
            bool result = BindingBase.TryFormat(format, args, out string value);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, value);
        }

        /// <summary>
        /// Tests TryFormat with edge case numeric values including extremes.
        /// Expects successful formatting and return value of true.
        /// </summary>
        [Theory]
        [InlineData("{0}", new object[] { int.MaxValue }, "2147483647")]
        [InlineData("{0}", new object[] { int.MinValue }, "-2147483648")]
        [InlineData("{0}", new object[] { double.NaN }, "NaN")]
        [InlineData("{0}", new object[] { double.PositiveInfinity }, "∞")]
        [InlineData("{0}", new object[] { double.NegativeInfinity }, "-∞")]
        [InlineData("{0:F2}", new object[] { 0.0 }, "0.00")]
        public void TryFormat_EdgeCaseNumericValues_ReturnsTrue(string format, object[] args, string expected)
        {
            // Arrange & Act
            bool result = BindingBase.TryFormat(format, args, out string value);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, value);
        }
    }
}