using System;
using System.Windows.Input;
using WorkMood.MauiApp.Infrastructure;
using Xunit;
using RelayCommand = WorkMood.MauiApp.Infrastructure.RelayCommand;

namespace WorkMood.MauiApp.Tests.Infrastructure;

public class RelayCommandOfTShould
{
    #region Test Types for Generic Testing

    public class TestClass
    {
        public string Value { get; set; } = string.Empty;
        public override string ToString() => Value;
    }

    public struct TestStruct
    {
        public int Value { get; set; }
        public TestStruct(int value) => Value = value;
    }

    public enum TestEnum
    {
        None = 0,
        First = 1,
        Second = 2
    }

    #endregion

    #region Checkpoint 1: Core ICommand Interface Tests

    [Fact]
    public void ConstructWithValidTypedAction_AndAcceptDelegate()
    {
        // Arrange
        var actionExecuted = false;
        Action<string?> action = _ => actionExecuted = true;

        // Act
        var command = new WorkMood.MauiApp.Infrastructure.RelayCommand<string>(action);

        // Assert
        Assert.NotNull(command);
        Assert.True(command is ICommand);
        
        // Verify action can be executed
        command.Execute("test");
        Assert.True(actionExecuted);
    }

    [Fact]
    public void ConstructWithValidTypedActionAndCanExecute_AndAcceptBothDelegates()
    {
        // Arrange
        var actionExecuted = false;
        Action<int> action = _ => actionExecuted = true;
        Predicate<int> canExecute = x => x > 0;

        // Act
        var command = new WorkMood.MauiApp.Infrastructure.RelayCommand<int>(action, canExecute);

        // Assert
        Assert.NotNull(command);
        Assert.True(command.CanExecute(5));
        Assert.False(command.CanExecute(-1));
        
        command.Execute(5);
        Assert.True(actionExecuted);
    }

    [Fact]
    public void ThrowArgumentNullException_WhenTypedActionIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new WorkMood.MauiApp.Infrastructure.RelayCommand<string>((Action<string?>)null!));
        Assert.Equal("execute", exception.ParamName);
    }

    [Fact]
    public void AcceptNullCanExecute_GracefullyDefaultingToAlwaysTrue()
    {
        // Arrange
        Action<string?> action = _ => { };

        // Act
        var command = new RelayCommand<string>(action, null);

        // Assert
        Assert.NotNull(command);
        Assert.True(command.CanExecute("test"));
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void ImplementICommandInterface_Correctly()
    {
        // Arrange
        var command = new RelayCommand<string>(_ => { });
        var canExecuteChangedAssigned = false;

        // Act & Assert
        Assert.IsAssignableFrom<ICommand>(command);
        
        // Test that CanExecuteChanged can be subscribed to
        EventHandler handler = (_, _) => { };
        command.CanExecuteChanged += handler;
        canExecuteChangedAssigned = true;
        command.CanExecuteChanged -= handler;
        
        Assert.True(canExecuteChangedAssigned);
    }

    [Fact]
    public void HandleGenericTypeParameter_ForReferenceTypes()
    {
        // Arrange
        TestClass? receivedParameter = null;
        var command = new RelayCommand<TestClass>(param => receivedParameter = param);
        var testObject = new TestClass { Value = "test" };

        // Act
        command.Execute(testObject);

        // Assert
        Assert.Same(testObject, receivedParameter);
    }

    [Fact]
    public void HandleGenericTypeParameter_ForValueTypes()
    {
        // Arrange
        TestStruct? receivedParameter = null;
        var command = new RelayCommand<TestStruct>(param => receivedParameter = param);
        var testStruct = new TestStruct(42);

        // Act
        command.Execute(testStruct);

        // Assert
        Assert.Equal(testStruct, receivedParameter);
    }

    [Fact]
    public void HandleGenericTypeParameter_ForEnumTypes()
    {
        // Arrange
        TestEnum? receivedParameter = null;
        var command = new RelayCommand<TestEnum>(param => receivedParameter = param);

        // Act
        command.Execute(TestEnum.Second);

        // Assert
        Assert.Equal(TestEnum.Second, receivedParameter);
    }

    #endregion

    #region Checkpoint 2: Type Safety and Parameter Validation Tests

    [Theory]
    [InlineData("test string", true)]
    [InlineData(null, true)]
    public void ValidateParameterType_ForStringParameters(object? parameter, bool expectedCanExecute)
    {
        // Arrange
        var command = new RelayCommand<string>(_ => { });

        // Act
        var canExecute = command.CanExecute(parameter);

        // Assert
        Assert.Equal(expectedCanExecute, canExecute);
    }

    [Theory]
    [InlineData(42, true)]
    [InlineData("not an int", false)]
    [InlineData(null, false)] // int is value type
    public void ValidateParameterType_ForValueTypeParameters(object? parameter, bool expectedCanExecute)
    {
        // Arrange
        var command = new RelayCommand<int>(_ => { });

        // Act
        var canExecute = command.CanExecute(parameter);

        // Assert
        Assert.Equal(expectedCanExecute, canExecute);
    }

    [Theory]
    [InlineData(null, false)] // nullable int does NOT allow null in this implementation
    [InlineData(42, true)]
    [InlineData("not an int", false)]
    public void ValidateParameterType_ForNullableValueTypeParameters(object? parameter, bool expectedCanExecute)
    {
        // Arrange
        var command = new WorkMood.MauiApp.Infrastructure.RelayCommand<int?>(_ => { });

        // Act
        var canExecute = command.CanExecute(parameter);

        // Assert
        Assert.Equal(expectedCanExecute, canExecute);
    }

    [Fact]
    public void ExecuteWithCorrectType_PassesParameterToAction()
    {
        // Arrange
        string? receivedParameter = null;
        var command = new RelayCommand<string>(param => receivedParameter = param);
        var testParameter = "test value";

        // Act
        command.Execute(testParameter);

        // Assert
        Assert.Equal(testParameter, receivedParameter);
    }

    [Fact]
    public void ExecuteWithIncorrectType_DoesNotCallAction()
    {
        // Arrange
        var actionCalled = false;
        var command = new RelayCommand<string>(_ => actionCalled = true);

        // Act
        command.Execute(42); // Wrong type

        // Assert
        Assert.False(actionCalled);
    }

    [Fact]
    public void ExecuteWithNull_CallsActionForReferenceTypes()
    {
        // Arrange
        string? receivedParameter = "not null";
        var command = new RelayCommand<string>(param => receivedParameter = param);

        // Act
        command.Execute(null);

        // Assert
        Assert.Null(receivedParameter);
    }

    [Fact]
    public void ExecuteWithNull_DoesNotCallActionForValueTypes()
    {
        // Arrange
        var actionCalled = false;
        var command = new RelayCommand<int>(_ => actionCalled = true);

        // Act
        command.Execute(null);

        // Assert
        Assert.False(actionCalled);
    }

    [Fact]
    public void ExecuteWithNull_DoesNotCallActionForNullableValueTypes()
    {
        // Arrange
        var actionCalled = false;
        var command = new WorkMood.MauiApp.Infrastructure.RelayCommand<int?>(_ => actionCalled = true);

        // Act
        command.Execute(null);

        // Assert  
        Assert.False(actionCalled); // Null is not passed to nullable value type commands
    }

    [Fact]
    public void CanExecuteWithTypedPredicate_PassesCorrectParameter()
    {
        // Arrange
        string? receivedParameter = null;
        var command = new RelayCommand<string>(_ => { }, param => { receivedParameter = param; return true; });
        var testParameter = "test value";

        // Act
        command.CanExecute(testParameter);

        // Assert
        Assert.Equal(testParameter, receivedParameter);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void CanExecuteWithTypedPredicate_ReturnsPredicateResult(bool predicateResult, bool expected)
    {
        // Arrange
        var command = new RelayCommand<string>(_ => { }, _ => predicateResult);

        // Act
        var result = command.CanExecute("test");

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Checkpoint 3: CanExecuteChanged Event and Advanced Scenarios Tests

    [Fact]
    public void SubscribeToCommandManagerEvent_WhenCanExecuteChangedEventAdded()
    {
        // Arrange
        var command = new RelayCommand<string>(_ => { });
        var eventRaised = false;
        EventHandler handler = (_, _) => eventRaised = true;

        // Act
        command.CanExecuteChanged += handler;
        CommandManager.InvalidateRequerySuggested();

        // Assert
        Assert.True(eventRaised);

        // Cleanup
        command.CanExecuteChanged -= handler;
    }

    [Fact]
    public void UnsubscribeFromCommandManagerEvent_WhenCanExecuteChangedEventRemoved()
    {
        // Arrange
        var command = new RelayCommand<string>(_ => { });
        var eventRaised = false;
        EventHandler handler = (_, _) => eventRaised = true;

        // Act
        command.CanExecuteChanged += handler;
        command.CanExecuteChanged -= handler;
        CommandManager.InvalidateRequerySuggested();

        // Assert
        Assert.False(eventRaised);
    }

    [Fact]
    public void TriggerCanExecuteChangedEvent_WhenRaiseCanExecuteChangedCalled()
    {
        // Arrange
        var command = new RelayCommand<string>(_ => { });
        var eventRaised = false;
        command.CanExecuteChanged += (_, _) => eventRaised = true;

        // Act
        command.RaiseCanExecuteChanged();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void WorkWithComplexReferenceType_IncludingNullHandling()
    {
        // Arrange
        TestClass? receivedParameter = null;
        bool canExecuteResult = false;
        var command = new RelayCommand<TestClass>(
            param => receivedParameter = param,
            param => { canExecuteResult = param?.Value == "valid"; return canExecuteResult; });

        var validObject = new TestClass { Value = "valid" };
        var invalidObject = new TestClass { Value = "invalid" };

        // Act & Assert - Valid object
        Assert.True(command.CanExecute(validObject));
        command.Execute(validObject);
        Assert.Same(validObject, receivedParameter);

        // Act & Assert - Invalid object  
        Assert.False(command.CanExecute(invalidObject));

        // Act & Assert - Null handling
        Assert.False(command.CanExecute(null));
    }

    [Fact]
    public void WorkWithValueTypeConstraints_IncludingStructs()
    {
        // Arrange
        TestStruct receivedParameter = new TestStruct(0);
        var command = new RelayCommand<TestStruct>(
            param => receivedParameter = param,
            param => param.Value > 10);

        var validStruct = new TestStruct(20);
        var invalidStruct = new TestStruct(5);

        // Act & Assert - Valid struct
        Assert.True(command.CanExecute(validStruct));
        command.Execute(validStruct);
        Assert.Equal(validStruct, receivedParameter);

        // Act & Assert - Invalid struct
        Assert.False(command.CanExecute(invalidStruct));

        // Act & Assert - Null doesn't work for value types
        Assert.False(command.CanExecute(null));
    }

    [Fact]
    public void HandleMultipleGenericCommands_WithDifferentTypes()
    {
        // Arrange
        var stringCommand = new RelayCommand<string>(s => { });
        var intCommand = new RelayCommand<int>(i => { });
        var objectCommand = new RelayCommand<TestClass>(o => { });

        // Act & Assert - Type isolation
        Assert.True(stringCommand.CanExecute("test"));
        Assert.False(stringCommand.CanExecute(42));

        Assert.True(intCommand.CanExecute(42));
        Assert.False(intCommand.CanExecute("test"));

        Assert.True(objectCommand.CanExecute(new TestClass()));
        Assert.False(objectCommand.CanExecute("test"));
    }

    [Fact]
    public void MaintainTypeSpecificBehavior_AcrossMultipleExecutions()
    {
        // Arrange
        var executionCount = 0;
        var lastParameter = string.Empty;
        var command = new RelayCommand<string>(param => 
        {
            executionCount++;
            lastParameter = param ?? "null";
        });

        // Act
        command.Execute("first");
        command.Execute("second");
        command.Execute(null);
        command.Execute(42); // Wrong type - should not execute

        // Assert
        Assert.Equal(3, executionCount); // Only 3 valid executions
        Assert.Equal("null", lastParameter); // Last valid execution was null
    }

    [Fact]
    public void IntegrateWithCommandManager_ForGenericTypeCommands()
    {
        // Arrange
        var stringCommand = new RelayCommand<string>(_ => { });
        var intCommand = new RelayCommand<int>(_ => { });
        var stringEventRaised = false;
        var intEventRaised = false;

        stringCommand.CanExecuteChanged += (_, _) => stringEventRaised = true;
        intCommand.CanExecuteChanged += (_, _) => intEventRaised = true;

        // Act - Global CommandManager invalidation
        CommandManager.InvalidateRequerySuggested();

        // Assert - Both typed commands receive notification
        Assert.True(stringEventRaised);
        Assert.True(intEventRaised);
    }

    #endregion
}