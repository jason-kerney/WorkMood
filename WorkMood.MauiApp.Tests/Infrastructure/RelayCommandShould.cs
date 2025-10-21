using System;
using System.Windows.Input;
using WorkMood.MauiApp.Infrastructure;
using Xunit;

namespace WorkMood.MauiApp.Tests.Infrastructure;

public class RelayCommandShould
{
    #region Checkpoint 1: Core ICommand Interface Tests

    [Fact]
    public void ConstructWithParameterizedAction_AndAcceptValidDelegate()
    {
        // Arrange
        var actionExecuted = false;
        Action<object?> action = _ => actionExecuted = true;

        // Act
        var command = new RelayCommand(action);

        // Assert
        Assert.NotNull(command);
        Assert.True(command is ICommand);
        
        // Verify action can be executed
        command.Execute(null);
        Assert.True(actionExecuted);
    }

    [Fact]
    public void ConstructWithParameterizedActionAndCanExecute_AndAcceptBothDelegates()
    {
        // Arrange
        var actionExecuted = false;
        Action<object?> action = _ => actionExecuted = true;
        Predicate<object?> canExecute = _ => true;

        // Act
        var command = new RelayCommand(action, canExecute);

        // Assert
        Assert.NotNull(command);
        Assert.True(command.CanExecute(null));
        
        command.Execute(null);
        Assert.True(actionExecuted);
    }

    [Fact]
    public void ThrowArgumentNullException_WhenActionIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new RelayCommand((Action<object?>)null!));
        Assert.Equal("execute", exception.ParamName);
    }

    [Fact]
    public void AcceptNullCanExecute_GracefullyDefaultingToAlwaysTrue()
    {
        // Arrange
        Action<object?> action = _ => { };

        // Act
        var command = new RelayCommand(action, null);

        // Assert
        Assert.NotNull(command);
        Assert.True(command.CanExecute(null));
        Assert.True(command.CanExecute("test"));
    }

    [Fact]
    public void ConstructWithParameterlessAction_AndConvertToParameterizedPattern()
    {
        // Arrange
        var actionExecuted = false;
        Action action = () => actionExecuted = true;

        // Act
        var command = new RelayCommand(action);

        // Assert
        Assert.NotNull(command);
        
        // Should execute regardless of parameter
        command.Execute("ignored parameter");
        Assert.True(actionExecuted);
    }

    [Fact]
    public void ConstructWithParameterlessActionAndCanExecute_AndConvertBothToParameterizedPattern()
    {
        // Arrange
        var actionExecuted = false;
        Action action = () => actionExecuted = true;
        Func<bool> canExecute = () => true;

        // Act
        var command = new RelayCommand(action, canExecute);

        // Assert
        Assert.NotNull(command);
        Assert.True(command.CanExecute("ignored parameter"));
        
        command.Execute("ignored parameter");
        Assert.True(actionExecuted);
    }

    [Fact]
    public void CreateSuccessfully_WhenParameterlessActionIsNull_ButFailAtExecution()
    {
        // Arrange - The constructor will succeed because it wraps the null action in a lambda
        var command = new RelayCommand((Action)null!);

        // Act & Assert - The exception occurs when trying to execute
        Assert.Throws<NullReferenceException>(() => command.Execute(null));
    }

    [Fact]
    public void AcceptNullParameterlessCanExecute_GracefullyDefaultingToAlwaysTrue()
    {
        // Arrange
        Action action = () => { };

        // Act
        var command = new RelayCommand(action, null);

        // Assert
        Assert.NotNull(command);
        Assert.True(command.CanExecute(null));
        Assert.True(command.CanExecute("test"));
    }

    #endregion

    #region Checkpoint 2: CanExecute and Execute Behavior Tests

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void ReturnCorrectCanExecuteValue_WhenPredicateSpecified(bool predicateResult, bool expected)
    {
        // Arrange
        var command = new RelayCommand(_ => { }, _ => predicateResult);

        // Act
        var result = command.CanExecute(null);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ReturnTrue_WhenNoCanExecutePredicateSpecified()
    {
        // Arrange
        var command = new RelayCommand(_ => { });

        // Act & Assert
        Assert.True(command.CanExecute(null));
        Assert.True(command.CanExecute("test"));
        Assert.True(command.CanExecute(42));
    }

    [Fact]
    public void PassParameterToCanExecutePredicate_WhenEvaluatingCanExecute()
    {
        // Arrange
        object? receivedParameter = null;
        var command = new RelayCommand(_ => { }, param => { receivedParameter = param; return true; });
        var testParameter = "test parameter";

        // Act
        command.CanExecute(testParameter);

        // Assert
        Assert.Equal(testParameter, receivedParameter);
    }

    [Fact]
    public void HandleNullParameter_InCanExecutePredicate()
    {
        // Arrange
        object? receivedParameter = "not null";
        var command = new RelayCommand(_ => { }, param => { receivedParameter = param; return true; });

        // Act
        command.CanExecute(null);

        // Assert
        Assert.Null(receivedParameter);
    }

    [Fact]
    public void InvokeActionWithCorrectParameter_WhenExecuted()
    {
        // Arrange
        object? receivedParameter = null;
        var command = new RelayCommand(param => receivedParameter = param);
        var testParameter = "test parameter";

        // Act
        command.Execute(testParameter);

        // Assert
        Assert.Equal(testParameter, receivedParameter);
    }

    [Fact]
    public void HandleNullParameter_InExecuteAction()
    {
        // Arrange
        object? receivedParameter = "not null";
        var command = new RelayCommand(param => receivedParameter = param);

        // Act
        command.Execute(null);

        // Assert
        Assert.Null(receivedParameter);
    }

    [Fact]
    public void ExecuteParameterlessAction_IgnoringProvidedParameter()
    {
        // Arrange
        var actionExecuted = false;
        var command = new RelayCommand(() => actionExecuted = true);

        // Act
        command.Execute("ignored parameter");

        // Assert
        Assert.True(actionExecuted);
    }

    [Fact]
    public void ExecuteRegardlessOfCanExecuteState_WhenExecuteCalled()
    {
        // Arrange
        var actionExecuted = false;
        var command = new RelayCommand(_ => actionExecuted = true, _ => false);

        // Act
        Assert.False(command.CanExecute(null)); // Verify CanExecute is false
        command.Execute(null); // Execute anyway

        // Assert
        Assert.True(actionExecuted);
    }

    [Fact]
    public void AllowMultipleExecutions_OnSameCommand()
    {
        // Arrange
        var executionCount = 0;
        var command = new RelayCommand(_ => executionCount++);

        // Act
        command.Execute(null);
        command.Execute(null);
        command.Execute(null);

        // Assert
        Assert.Equal(3, executionCount);
    }

    #endregion

    #region Checkpoint 3: CanExecuteChanged Event and CommandManager Integration Tests

    [Fact]
    public void SubscribeToCommandManagerEvent_WhenCanExecuteChangedEventAdded()
    {
        // Arrange
        var command = new RelayCommand(_ => { });
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
        var command = new RelayCommand(_ => { });
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
        var command = new RelayCommand(_ => { });
        var eventRaised = false;
        command.CanExecuteChanged += (_, _) => eventRaised = true;

        // Act
        command.RaiseCanExecuteChanged();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void CallCommandManagerInvalidate_WhenRaiseCanExecuteChangedInvoked()
    {
        // Arrange
        var command = new RelayCommand(_ => { });
        var managerEventRaised = false;
        EventHandler managerHandler = (_, _) => managerEventRaised = true;
        CommandManager.RequerySuggested += managerHandler;

        // Act
        command.RaiseCanExecuteChanged();

        // Assert
        Assert.True(managerEventRaised);

        // Cleanup
        CommandManager.RequerySuggested -= managerHandler;
    }

    [Fact]
    public void HandleMultipleEventSubscriptions_Correctly()
    {
        // Arrange
        var command = new RelayCommand(_ => { });
        var event1Raised = false;
        var event2Raised = false;
        EventHandler handler1 = (_, _) => event1Raised = true;
        EventHandler handler2 = (_, _) => event2Raised = true;

        // Act
        command.CanExecuteChanged += handler1;
        command.CanExecuteChanged += handler2;
        command.RaiseCanExecuteChanged();

        // Assert
        Assert.True(event1Raised);
        Assert.True(event2Raised);

        // Cleanup
        command.CanExecuteChanged -= handler1;
        command.CanExecuteChanged -= handler2;
    }

    [Fact]
    public void HandleNullEventHandler_GracefullyInEventOperations()
    {
        // Arrange
        var command = new RelayCommand(_ => { });

        // Act & Assert - Should not throw
        command.CanExecuteChanged += null;
        command.CanExecuteChanged -= null;
        command.RaiseCanExecuteChanged(); // Should work even with null handlers
    }

    [Fact]
    public void NotReceiveNotification_AfterUnsubscribingFromEvent()
    {
        // Arrange
        var command = new RelayCommand(_ => { });
        var eventCount = 0;
        EventHandler handler = (_, _) => eventCount++;

        // Act
        command.CanExecuteChanged += handler;
        command.RaiseCanExecuteChanged();
        command.CanExecuteChanged -= handler;
        command.RaiseCanExecuteChanged();

        // Assert
        Assert.Equal(1, eventCount); // Only first raise should count
    }

    [Fact]
    public void ShareEventNotifications_WithOtherRelayCommands()
    {
        // Arrange
        var command1 = new RelayCommand(_ => { });
        var command2 = new RelayCommand(_ => { });
        var command1EventRaised = false;
        var command2EventRaised = false;
        
        command1.CanExecuteChanged += (_, _) => command1EventRaised = true;
        command2.CanExecuteChanged += (_, _) => command2EventRaised = true;

        // Act - Invalidate globally through CommandManager
        CommandManager.InvalidateRequerySuggested();

        // Assert - Both commands should receive notification
        Assert.True(command1EventRaised);
        Assert.True(command2EventRaised);
    }

    #endregion
}