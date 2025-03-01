using System.Windows.Input;

namespace PrismPanda.ViewModels;

public class RelayCommand(Action<object> execute, Func<object, bool> canExecute) : ICommand
{
    private readonly Action<object> _execute = execute
        ?? throw new ArgumentNullException(nameof(execute));

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
        => canExecute?.Invoke(parameter ?? throw new ArgumentNullException(nameof(parameter))) ?? true;

    public void Execute(object? parameter)
        => _execute(parameter ?? throw new ArgumentNullException(nameof(parameter)));
}
