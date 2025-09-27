using System.Windows.Input;

namespace WorkMood.MauiApp.Infrastructure;

/// <summary>
/// A command implementation that encapsulates a method with optional parameter and can query condition
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute != null ? _ => canExecute() : null)
    {
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}

/// <summary>
/// CommandManager compatibility for MAUI - simplified implementation
/// </summary>
public static class CommandManager
{
    public static event EventHandler? RequerySuggested;

    public static void InvalidateRequerySuggested()
    {
        RequerySuggested?.Invoke(null, EventArgs.Empty);
    }
}

/// <summary>
/// A generic command implementation that encapsulates a method with a strongly typed parameter
/// </summary>
/// <typeparam name="T">The type of the command parameter</typeparam>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Predicate<T?>? _canExecute;

    public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        if (parameter is T typedParameter)
            return _canExecute?.Invoke(typedParameter) ?? true;
        if (parameter == null && !typeof(T).IsValueType)
            return _canExecute?.Invoke(default(T)) ?? true;
        return false;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T typedParameter)
            _execute(typedParameter);
        else if (parameter == null && !typeof(T).IsValueType)
            _execute(default(T));
    }

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}