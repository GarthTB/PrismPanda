using System.Windows;

namespace PrismPanda.ViewModels;

/// <summary> To simplify the code. </summary>
internal static class MsgB
{
    internal static void OkErr(string message)
        => MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

    internal static void OkInf(string title, string message)
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
}
