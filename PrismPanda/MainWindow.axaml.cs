using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using PrismPanda.Core;
using Window = Avalonia.Controls.Window;

namespace PrismPanda;

public partial class MainWindow : Window
{
    #region Initialize and About

    public MainWindow()
    {
        InitializeComponent();
        ImgBox.Source = _processedThumbnail;
    }

    private Bitmap? _processedThumbnail = null;

    private async void Mw_OnKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.F1) await ShowAbout();
        }
        catch (Exception)
        { // ignored
        }
    }

    private static async Task ShowAbout()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        _ = await MessageBoxManager.GetMessageBoxStandard(
                "About",
                "Welcome to PrismPanda!\n"
              + "Split an image into different channels and easily adjust them\n"
              + "using sliders or numerical values! When the value is 1, it reaches\n"
              + "the maximum value of the channel, and when it's -1, it reaches\n"
              + "the minimum value of the channel. 0 does nothing.\n"
              + "See README for details.\n"
              + $"Version: {version}\n"
              + "Copyright \u00a9 GarthTB 2025. All rights reserved.")
            .ShowAsync();
    }

    #endregion

    #region UI-Only Logic

    private void ColorSpaceCombo_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Ch1TxB is null || Ch2TxB is null || Ch3TxB is null || Ch1Name is null || Ch2Name is null || Ch3Name is null)
            return;
        Ch1TxB.Text = Ch2TxB.Text = Ch3TxB.Text = "0.000";
        (Ch1Name.Content, Ch2Name.Content, Ch3Name.Content) = ColorSpaceCombo.SelectedIndex switch
        {
            0 => ("Hue", "Saturation", "Lightness"),
            1 => ("Hue", "Saturation", "Value"),
            2 => ("Hue", "Saturation", "Intensity"),
            3 => ("Tint", "Saturation", "Lightness"),
            4 => ("Lightness", "Chroma", "Hue (Lab)"),
            5 => ("Lightness", "Chroma", "Hue (Luv)"),
            6 => ("Hue", "Saturation", "Lightness"),
            7 => ("Lightness", "Chroma", "Hue"),
            8 => ("Lightness", "Chroma", "Hue"),
            9 => ("Hue", "Saturation", "Value"),
            10 => ("Hue", "Saturation", "Lightness"),
            _ => ("Hue", "Chroma", "Tone")
        };
    }

    private void Ch1Reset_OnClick(object? sender, RoutedEventArgs e) => Ch1TxB.Text = "0.000";

    private void Ch2Reset_OnClick(object? sender, RoutedEventArgs e) => Ch2TxB.Text = "0.000";

    private void Ch3Reset_OnClick(object? sender, RoutedEventArgs e) => Ch3TxB.Text = "0.000";

    private void Ch1Sli_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        Ch1TxB.Text = e.NewValue.ToString("0.000");
        ProcessThumbnail();
    }

    private void Ch2Sli_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        Ch2TxB.Text = e.NewValue.ToString("0.000");
        ProcessThumbnail();
    }

    private void Ch3Sli_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        Ch3TxB.Text = e.NewValue.ToString("0.000");
        ProcessThumbnail();
    }

    private void Ch1TxB_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!double.TryParse(Ch1TxB.Text, out var value)) Ch1TxB.Text = "0.000";
        switch (value)
        {
            case < -1: Ch1TxB.Text = "-1.000"; break;
            case > 1: Ch1TxB.Text = "1.000"; break;
            default: Ch1Sli.Value = value; break;
        }
    }

    private void Ch2TxB_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!double.TryParse(Ch2TxB.Text, out var value)) Ch2TxB.Text = "0.000";
        switch (value)
        {
            case < -1: Ch2TxB.Text = "-1.000"; break;
            case > 1: Ch2TxB.Text = "1.000"; break;
            default: Ch2Sli.Value = value; break;
        }
    }

    private void Ch3TxB_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!double.TryParse(Ch3TxB.Text, out var value)) Ch3TxB.Text = "0.000";
        switch (value)
        {
            case < -1: Ch3TxB.Text = "-1.000"; break;
            case > 1: Ch3TxB.Text = "1.000"; break;
            default: Ch3Sli.Value = value; break;
        }
    }

    #endregion

    private async void OpenBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Open One Image",
                    AllowMultiple = false,
                    FileTypeFilter = [FilePickerFileTypes.ImageAll, FilePickerFileTypes.All]
                });
            if (files.Count == 0) return;
            if (await ImageManager.SetImage(files[0].Path.AbsolutePath))
                Ch1TxB.Text = Ch2TxB.Text = Ch3TxB.Text = "0.000";
        }
        catch (Exception)
        { // ignored
        }
    }

    private void ProcessThumbnail() { }
}