#nullable disable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class CommandTests : BaseTestFixture
    {
        [Fact]
        public void Constructor()
        {
            var cmd = new Command(() => { });
            Assert.True(cmd.CanExecute(null));
        }

        [Fact]
        public void ThrowsWithNullConstructor()
        {
            Assert.Throws<ArgumentNullException>(() => new Command((Action)null));
        }

        [Fact]
        public void ThrowsWithNullParameterizedConstructor()
        {
            Assert.Throws<ArgumentNullException>(() => new Command((Action<object>)null));
        }

        [Fact]
        public void ThrowsWithNullCanExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new Command(() => { }, null));
        }

        [Fact]
        public void ThrowsWithNullParameterizedCanExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new Command(o => { }, null));
        }

        [Fact]
        public void ThrowsWithNullExecuteValidCanExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new Command(null, () => true));
        }

        [Fact]
        public void Execute()
        {
            bool executed = false;
            var cmd = new Command(() => executed = true);

            cmd.Execute(null);
            Assert.True(executed);
        }

        [Fact]
        public void ExecuteParameterized()
        {
            object executed = null;
            var cmd = new Command(o => executed = o);

            var expected = new object();
            cmd.Execute(expected);

            Assert.Equal(expected, executed);
        }

        [Fact]
        public void ExecuteWithCanExecute()
        {
            bool executed = false;
            var cmd = new Command(() => executed = true, () => true);

            cmd.Execute(null);
            Assert.True(executed);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void CanExecute(bool expected)
        {
            bool canExecuteRan = false;
            var cmd = new Command(() => { }, () =>
            {
                canExecuteRan = true;
                return expected;
            });

            Assert.Equal(expected, cmd.CanExecute(null));
            Assert.True(canExecuteRan);
        }

        [Fact]
        public void ChangeCanExecute()
        {
            bool signaled = false;
            var cmd = new Command(() => { });

            cmd.CanExecuteChanged += (sender, args) => signaled = true;

            cmd.ChangeCanExecute();
            Assert.True(signaled);
        }

        [Fact]
        public void GenericThrowsWithNullExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new Command<string>(null));
        }

        [Fact]
        public void GenericThrowsWithNullExecuteAndCanExecuteValid()
        {
            Assert.Throws<ArgumentNullException>(() => new Command<string>(null, s => true));
        }

        [Fact]
        public void GenericThrowsWithValidExecuteAndCanExecuteNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Command<string>(s => { }, null));
        }

        [Fact]
        public void GenericExecute()
        {
            string result = null;
            var cmd = new Command<string>(s => result = s);

            cmd.Execute("Foo");
            Assert.Equal("Foo", result);
        }

        [Fact]
        public void GenericExecuteWithCanExecute()
        {
            string result = null;
            var cmd = new Command<string>(s => result = s, s => true);

            cmd.Execute("Foo");
            Assert.Equal("Foo", result);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void GenericCanExecute(bool expected)
        {
            string result = null;
            var cmd = new Command<string>(s => { }, s =>
            {
                result = s;
                return expected;
            });

            Assert.Equal(expected, cmd.CanExecute("Foo"));
            Assert.Equal("Foo", result);
        }

        class FakeParentContext
        {
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        class FakeChildContext
        {
        }

        [Fact]
        public void CanExecuteReturnsFalseIfParameterIsWrongReferenceType()
        {
            var command = new Command<FakeChildContext>(context => { }, context => true);

            Assert.False(command.CanExecute(new FakeParentContext()), "the parameter is of the wrong type");
        }

        [Fact]
        public void CanExecuteReturnsFalseIfParameterIsWrongValueType()
        {
            var command = new Command<int>(context => { }, context => true);

            Assert.False(command.CanExecute(10.5), "the parameter is of the wrong type");
        }

        [Fact]
        public void CanExecuteUsesParameterIfReferenceTypeAndSetToNull()
        {
            var command = new Command<FakeChildContext>(context => { }, context => true);

            Assert.True(command.CanExecute(null), "null is a valid value for a reference type");
        }

        [Fact]
        public void CanExecuteUsesParameterIfNullableAndSetToNull()
        {
            var command = new Command<int?>(context => { }, context => true);

            Assert.True(command.CanExecute(null), "null is a valid value for a Nullable<int> type");
        }

        [Fact]
        public void CanExecuteIgnoresParameterIfValueTypeAndSetToNull()
        {
            var command = new Command<int>(context => { }, context => true);

            Assert.False(command.CanExecute(null), "null is not a valid valid for int");
        }

        [Fact]
        public void ExecuteDoesNotRunIfParameterIsWrongReferenceType()
        {
            int executions = 0;
            var command = new Command<FakeChildContext>(context => executions += 1);

            command.Execute(new FakeParentContext()); // "the command should not execute, so no exception should be thrown"
            Assert.True(executions == 0, "the command should not have executed");
        }

        [Fact]
        public void ExecuteDoesNotRunIfParameterIsWrongValueType()
        {
            int executions = 0;
            var command = new Command<int>(context => executions += 1);

            command.Execute(10.5); // "the command should not execute, so no exception should be thrown"
            Assert.True(executions == 0, "the command should not have executed");
        }

        [Fact]
        public void ExecuteRunsIfReferenceTypeAndSetToNull()
        {
            int executions = 0;
            var command = new Command<FakeChildContext>(context => executions += 1);

            command.Execute(null); // "null is a valid value for a reference type"
            Assert.True(executions == 1, "the command should have executed");
        }

        [Fact]
        public void ExecuteRunsIfNullableAndSetToNull()
        {
            int executions = 0;
            var command = new Command<int?>(context => executions += 1);

            command.Execute(null); // "null is a valid value for a Nullable<int> type"
            Assert.True(executions == 1, "the command should have executed");
        }

        [Fact]
        public void ExecuteDoesNotRunIfValueTypeAndSetToNull()
        {
            int executions = 0;
            var command = new Command<int>(context => executions += 1);

            command.Execute(null); // "null is not a valid value for int"
            Assert.True(executions == 0, "the command should not have executed");
        }

        [Theory]
        [InlineData(typeof(Button), true)]
        [InlineData(typeof(Button), false)]
        [InlineData(typeof(RefreshView), true)]
        [InlineData(typeof(RefreshView), false)]
        [InlineData(typeof(TextCell), true)]
        [InlineData(typeof(TextCell), false)]
        [InlineData(typeof(ImageButton), true)]
        [InlineData(typeof(ImageButton), false)]
        [InlineData(typeof(MenuItem), true)]
        [InlineData(typeof(MenuItem), false)]
        [InlineData(typeof(SearchBar), true)]
        [InlineData(typeof(SearchBar), false)]
        [InlineData(typeof(SearchHandler), true)]
        [InlineData(typeof(SearchHandler), false)]
        public async Task CommandsSubscribedToCanExecuteCollect(Type controlType, bool useWeakEventHandler)
        {
            // Create a view model with a Command
            ICommand command;

            if (!useWeakEventHandler)
                command = new CommandWithoutWeakEventHandler();
            else
                command = new Command(() => { });

            List<WeakReference> weakReferences = new List<WeakReference>();

            // Create a button in a separate scope to ensure no references remain
            {
                var control = (BindableObject)Activator.CreateInstance(controlType);
                switch (control)
                {
                    case Button b:
                        b.Command = command;
                        break;
                    case RefreshView r:
                        r.Command = command;
                        break;
                    case TextCell t:
                        t.Command = command;
                        break;
                    case ImageButton i:
                        i.Command = command;
                        break;
                    case MenuItem m:
                        m.Command = command;
                        break;
                    case SearchBar s:
                        s.SearchCommand = command;
                        break;
                    case SearchHandler sh:
                        sh.Command = command;
                        sh.ClearPlaceholderCommand = command;
                        break;
                }

                // Create a weak reference to the button
                weakReferences.Add(new WeakReference(control));

                if (control is ICommandElement commandElement)
                {
                    // Add weak references to the command and its cleanup tracker
                    weakReferences.Add(new WeakReference(commandElement.CleanupTracker));
                    weakReferences.Add(new WeakReference(commandElement.CleanupTracker.Proxy));
                }
                else if (control is SearchHandler searchHandler)
                {
                    // Add weak references to the command and its cleanup tracker
                    weakReferences.Add(new WeakReference(searchHandler.CommandSubscription));
                    weakReferences.Add(new WeakReference(searchHandler.CommandSubscription.Proxy));
                    weakReferences.Add(new WeakReference(searchHandler.ClearPlaceholderCommandSubscription));
                    weakReferences.Add(new WeakReference(searchHandler.ClearPlaceholderCommandSubscription.Proxy));
                }

                await TestHelpers.Collect();
                await TestHelpers.Collect();

                // Make sure everything is still alive if the button is still in scope
                // We need to reference the button here again to keep it alive 
                // awaiting a Task appears to move us to a new scope and causes the button to be collected
                Assert.NotNull(control);

                foreach (var weakRef in weakReferences)
                {
                    Assert.True(weakRef.IsAlive);
                }
            }

            foreach (var weakRef in weakReferences)
            {
                Assert.False(await weakRef.WaitForCollect());
            }
        }

        class CommandWithoutWeakEventHandler : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter) { }

            public void ChangeCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Tests that the Command constructor with Action&lt;object&gt; parameter throws ArgumentNullException when execute is null.
        /// This ensures proper parameter validation for the execute delegate.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullExecute_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Command((Action<object>)null));
        }

        /// <summary>
        /// Tests that the Command constructor with Action&lt;object&gt; parameter successfully creates a Command when execute is valid.
        /// This verifies that a valid delegate is properly stored and the Command can be used.
        /// Expected result: Command is created successfully and can execute.
        /// </summary>
        [Fact]
        public void Constructor_ValidExecute_CreatesCommandSuccessfully()
        {
            // Arrange
            bool executed = false;
            Action<object> execute = obj => executed = true;

            // Act
            var command = new Command(execute);

            // Assert
            Assert.NotNull(command);
            Assert.True(command.CanExecute(null));

            command.Execute(null);
            Assert.True(executed);
        }

        /// <summary>
        /// Tests that the Command constructor with Action&lt;object&gt; parameter works with various parameter types.
        /// This verifies that the execute action receives the correct parameter value.
        /// Expected result: Execute action receives the passed parameter correctly.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("test")]
        [InlineData(42)]
        public void Constructor_ValidExecuteWithParameter_ExecutesWithCorrectParameter(object expectedParameter)
        {
            // Arrange
            object actualParameter = new object(); // Different from expected
            Action<object> execute = obj => actualParameter = obj;

            // Act
            var command = new Command(execute);
            command.Execute(expectedParameter);

            // Assert
            Assert.Equal(expectedParameter, actualParameter);
        }
    }

    public partial class CommandTTests
    {
        /// <summary>
        /// Tests that the Command<T> constructor throws ArgumentNullException when execute parameter is null.
        /// Validates that null execute actions are properly rejected during construction.
        /// </summary>
        [Fact]
        public void Constructor_NullExecuteParameter_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Command<string>(null, s => true));
        }

        /// <summary>
        /// Tests that the Command<T> constructor throws ArgumentNullException when canExecute parameter is null.
        /// Validates that null canExecute predicates are properly rejected during construction.
        /// </summary>
        [Fact]
        public void Constructor_NullCanExecuteParameter_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Command<string>(s => { }, null));
        }

        /// <summary>
        /// Tests that the Command<T> constructor throws ArgumentNullException when both parameters are null.
        /// Validates that the execute parameter is checked first in the validation order.
        /// </summary>
        [Fact]
        public void Constructor_BothParametersNull_ThrowsArgumentNullExceptionForExecute()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new Command<string>(null, null));
            Assert.Equal("execute", ex.ParamName);
        }

        /// <summary>
        /// Tests that the Command<T> constructor succeeds with valid non-null parameters.
        /// Validates that proper construction occurs with valid execute and canExecute delegates.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_SucceedsConstruction()
        {
            // Arrange
            bool executeInvoked = false;
            bool canExecuteInvoked = false;

            // Act
            var command = new Command<string>(
                s => executeInvoked = true,
                s => { canExecuteInvoked = true; return true; });

            // Assert
            Assert.NotNull(command);
            Assert.False(executeInvoked);
            Assert.False(canExecuteInvoked);
        }

        /// <summary>
        /// Tests Command<T> execute behavior with valid parameter for reference type.
        /// Validates that execute action is called when parameter type matches T and IsValidParameter returns true.
        /// </summary>
        [Fact]
        public void Constructor_ExecuteWithValidReferenceType_ExecutesAction()
        {
            // Arrange
            string receivedParameter = null;
            var command = new Command<string>(s => receivedParameter = s, s => true);
            const string testValue = "test";

            // Act
            command.Execute(testValue);

            // Assert
            Assert.Equal(testValue, receivedParameter);
        }

        /// <summary>
        /// Tests Command<T> execute behavior with null parameter for reference type.
        /// Validates that execute action is called with null when T is a reference type (null is valid).
        /// </summary>
        [Fact]
        public void Constructor_ExecuteWithNullForReferenceType_ExecutesAction()
        {
            // Arrange
            string receivedParameter = "initial";
            var command = new Command<string>(s => receivedParameter = s, s => true);

            // Act
            command.Execute(null);

            // Assert
            Assert.Null(receivedParameter);
        }

        /// <summary>
        /// Tests Command<T> execute behavior with valid parameter for value type.
        /// Validates that execute action is called when parameter matches value type T.
        /// </summary>
        [Fact]
        public void Constructor_ExecuteWithValidValueType_ExecutesAction()
        {
            // Arrange
            int receivedParameter = 0;
            var command = new Command<int>(i => receivedParameter = i, i => true);
            const int testValue = 42;

            // Act
            command.Execute(testValue);

            // Assert
            Assert.Equal(testValue, receivedParameter);
        }

        /// <summary>
        /// Tests Command<T> execute behavior with null parameter for value type.
        /// Validates that execute action is NOT called when parameter is null and T is a non-nullable value type.
        /// </summary>
        [Fact]
        public void Constructor_ExecuteWithNullForValueType_DoesNotExecuteAction()
        {
            // Arrange
            bool executeInvoked = false;
            var command = new Command<int>(i => executeInvoked = true, i => true);

            // Act
            command.Execute(null);

            // Assert
            Assert.False(executeInvoked);
        }

        /// <summary>
        /// Tests Command<T> execute behavior with null parameter for nullable value type.
        /// Validates that execute action is called with null when T is a nullable value type.
        /// </summary>
        [Fact]
        public void Constructor_ExecuteWithNullForNullableValueType_ExecutesAction()
        {
            // Arrange
            int? receivedParameter = 99;
            var command = new Command<int?>(i => receivedParameter = i, i => true);

            // Act
            command.Execute(null);

            // Assert
            Assert.Null(receivedParameter);
        }

        /// <summary>
        /// Tests Command<T> execute behavior with wrong parameter type.
        /// Validates that execute action is NOT called when parameter type doesn't match T.
        /// </summary>
        [Fact]
        public void Constructor_ExecuteWithWrongParameterType_DoesNotExecuteAction()
        {
            // Arrange
            bool executeInvoked = false;
            var command = new Command<string>(s => executeInvoked = true, s => true);

            // Act
            command.Execute(123); // int instead of string

            // Assert
            Assert.False(executeInvoked);
        }

        /// <summary>
        /// Tests Command<T> CanExecute behavior with valid parameter for reference type.
        /// Validates that canExecute predicate is called and its result is returned when parameter is valid.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_CanExecuteWithValidReferenceType_ReturnsPredicateResult(bool expectedResult)
        {
            // Arrange
            var command = new Command<string>(s => { }, s => expectedResult);
            const string testValue = "test";

            // Act
            bool result = command.CanExecute(testValue);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests Command<T> CanExecute behavior with null parameter for reference type.
        /// Validates that canExecute predicate is called with null when T is a reference type.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_CanExecuteWithNullForReferenceType_ReturnsPredicateResult(bool expectedResult)
        {
            // Arrange
            var command = new Command<string>(s => { }, s => expectedResult);

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests Command<T> CanExecute behavior with null parameter for value type.
        /// Validates that CanExecute returns false when parameter is null and T is a non-nullable value type.
        /// </summary>
        [Fact]
        public void Constructor_CanExecuteWithNullForValueType_ReturnsFalse()
        {
            // Arrange
            var command = new Command<int>(i => { }, i => true);

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Command<T> CanExecute behavior with null parameter for nullable value type.
        /// Validates that canExecute predicate is called with null when T is a nullable value type.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_CanExecuteWithNullForNullableValueType_ReturnsPredicateResult(bool expectedResult)
        {
            // Arrange
            var command = new Command<int?>(i => { }, i => expectedResult);

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests Command<T> CanExecute behavior with wrong parameter type.
        /// Validates that CanExecute returns false when parameter type doesn't match T.
        /// </summary>
        [Fact]
        public void Constructor_CanExecuteWithWrongParameterType_ReturnsFalse()
        {
            // Arrange
            var command = new Command<string>(s => { }, s => true);

            // Act
            bool result = command.CanExecute(123); // int instead of string

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the Command&lt;T&gt; constructor throws ArgumentNullException when execute parameter is null.
        /// This validates the null check validation in the constructor.
        /// Expected result: ArgumentNullException with parameter name "execute".
        /// </summary>
        [Fact]
        public void Constructor_NullExecute_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new Command<string>(null));
            Assert.Equal("execute", exception.ParamName);
        }

        /// <summary>
        /// Tests that the Command&lt;T&gt; constructor successfully creates a command with a valid execute action.
        /// This validates the basic constructor functionality with a reference type.
        /// Expected result: Command is created and can execute successfully.
        /// </summary>
        [Fact]
        public void Constructor_ValidExecuteReferenceType_CreatesCommand()
        {
            // Arrange
            bool executed = false;
            string receivedParameter = null;
            Action<string> execute = param =>
            {
                executed = true;
                receivedParameter = param;
            };

            // Act
            var command = new Command<string>(execute);

            // Assert
            Assert.NotNull(command);
            Assert.True(command.CanExecute("test"));

            command.Execute("test");
            Assert.True(executed);
            Assert.Equal("test", receivedParameter);
        }

        /// <summary>
        /// Tests that the Command&lt;T&gt; constructor successfully creates a command with a valid execute action for value types.
        /// This validates the basic constructor functionality with a value type.
        /// Expected result: Command is created and can execute successfully.
        /// </summary>
        [Fact]
        public void Constructor_ValidExecuteValueType_CreatesCommand()
        {
            // Arrange
            bool executed = false;
            int receivedParameter = 0;
            Action<int> execute = param =>
            {
                executed = true;
                receivedParameter = param;
            };

            // Act
            var command = new Command<int>(execute);

            // Assert
            Assert.NotNull(command);
            Assert.True(command.CanExecute(42));

            command.Execute(42);
            Assert.True(executed);
            Assert.Equal(42, receivedParameter);
        }

        /// <summary>
        /// Tests that the Command&lt;T&gt; constructor successfully creates a command with a valid execute action for nullable value types.
        /// This validates the basic constructor functionality with a nullable value type.
        /// Expected result: Command is created and can execute successfully with both null and non-null values.
        /// </summary>
        [Fact]
        public void Constructor_ValidExecuteNullableValueType_CreatesCommand()
        {
            // Arrange
            bool executed = false;
            int? receivedParameter = null;
            Action<int?> execute = param =>
            {
                executed = true;
                receivedParameter = param;
            };

            // Act
            var command = new Command<int?>(execute);

            // Assert
            Assert.NotNull(command);
            Assert.True(command.CanExecute(42));
            Assert.True(command.CanExecute(null));

            command.Execute(42);
            Assert.True(executed);
            Assert.Equal(42, receivedParameter);

            executed = false;
            command.Execute(null);
            Assert.True(executed);
            Assert.Null(receivedParameter);
        }

        /// <summary>
        /// Tests that the Command&lt;T&gt; does not execute when passed an invalid parameter type.
        /// This validates the IsValidParameter logic for reference types.
        /// Expected result: Command does not execute the action when parameter is wrong type.
        /// </summary>
        [Fact]
        public void Constructor_InvalidParameterType_DoesNotExecute()
        {
            // Arrange
            bool executed = false;
            Action<string> execute = param => executed = true;
            var command = new Command<string>(execute);

            // Act
            command.Execute(42); // int instead of string

            // Assert
            Assert.False(executed);
        }

        /// <summary>
        /// Tests that the Command&lt;T&gt; executes when passed null for a reference type.
        /// This validates the IsValidParameter logic allows null for reference types.
        /// Expected result: Command executes the action when parameter is null for reference type.
        /// </summary>
        [Fact]
        public void Constructor_NullParameterReferenceType_Executes()
        {
            // Arrange
            bool executed = false;
            string receivedParameter = "initial";
            Action<string> execute = param =>
            {
                executed = true;
                receivedParameter = param;
            };
            var command = new Command<string>(execute);

            // Act
            command.Execute(null);

            // Assert
            Assert.True(executed);
            Assert.Null(receivedParameter);
        }

        /// <summary>
        /// Tests that the Command&lt;T&gt; does not execute when passed null for a value type.
        /// This validates the IsValidParameter logic rejects null for value types.
        /// Expected result: Command does not execute the action when parameter is null for value type.
        /// </summary>
        [Fact]
        public void Constructor_NullParameterValueType_DoesNotExecute()
        {
            // Arrange
            bool executed = false;
            Action<int> execute = param => executed = true;
            var command = new Command<int>(execute);

            // Act
            command.Execute(null);

            // Assert
            Assert.False(executed);
        }

        /// <summary>
        /// Tests that the Command&lt;T&gt; executes when passed null for a nullable value type.
        /// This validates the IsValidParameter logic allows null for nullable value types.
        /// Expected result: Command executes the action when parameter is null for nullable value type.
        /// </summary>
        [Fact]
        public void Constructor_NullParameterNullableValueType_Executes()
        {
            // Arrange
            bool executed = false;
            int? receivedParameter = 42;
            Action<int?> execute = param =>
            {
                executed = true;
                receivedParameter = param;
            };
            var command = new Command<int?>(execute);

            // Act
            command.Execute(null);

            // Assert
            Assert.True(executed);
            Assert.Null(receivedParameter);
        }

        /// <summary>
        /// Tests that CanExecute returns false for invalid parameter types.
        /// This validates the IsValidParameter logic in CanExecute for reference types.
        /// Expected result: CanExecute returns false when parameter type is invalid.
        /// </summary>
        [Fact]
        public void Constructor_CanExecuteInvalidParameterType_ReturnsFalse()
        {
            // Arrange
            Action<string> execute = param => { };
            var command = new Command<string>(execute);

            // Act & Assert
            Assert.False(command.CanExecute(42)); // int instead of string
        }

        /// <summary>
        /// Tests that CanExecute returns true for valid parameter types.
        /// This validates the IsValidParameter logic in CanExecute for reference types.
        /// Expected result: CanExecute returns true when parameter type is valid.
        /// </summary>
        [Fact]
        public void Constructor_CanExecuteValidParameterType_ReturnsTrue()
        {
            // Arrange
            Action<string> execute = param => { };
            var command = new Command<string>(execute);

            // Act & Assert
            Assert.True(command.CanExecute("test"));
            Assert.True(command.CanExecute(null)); // null is valid for reference type
        }

        /// <summary>
        /// Tests that CanExecute returns false for null with value types.
        /// This validates the IsValidParameter logic in CanExecute for value types.
        /// Expected result: CanExecute returns false when parameter is null for value type.
        /// </summary>
        [Fact]
        public void Constructor_CanExecuteNullValueType_ReturnsFalse()
        {
            // Arrange
            Action<int> execute = param => { };
            var command = new Command<int>(execute);

            // Act & Assert
            Assert.False(command.CanExecute(null));
        }

        /// <summary>
        /// Tests that CanExecute returns true for null with nullable value types.
        /// This validates the IsValidParameter logic in CanExecute for nullable value types.
        /// Expected result: CanExecute returns true when parameter is null for nullable value type.
        /// </summary>
        [Fact]
        public void Constructor_CanExecuteNullNullableValueType_ReturnsTrue()
        {
            // Arrange
            Action<int?> execute = param => { };
            var command = new Command<int?>(execute);

            // Act & Assert
            Assert.True(command.CanExecute(null));
            Assert.True(command.CanExecute(42));
        }
    }
}