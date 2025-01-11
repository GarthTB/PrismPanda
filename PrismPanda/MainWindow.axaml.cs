using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using PrismPanda.Core;
using Window = Avalonia.Controls.Window;

namespace PrismPanda;

public partial class MainWindow : Window
{
    #region Initialize and About

    public MainWindow() => InitializeComponent();

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
              + "Split the image into three channels in different color spaces,\n"
              + "and easily adjust the values of each channel with sliders or numbers.\n"
              + "When the gain is 0, the pixel values remain unchanged.\n"
              + "When the gain is 1, the pixel values are doubled.\n"
              + "When the gain is -1, the pixel value is 0.\n"
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

    private void FormatCombo_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (FormatCombo is not null) ImageManager.FormatIndex = FormatCombo.SelectedIndex;
    }

    private void Ch1Reset_OnClick(object? sender, RoutedEventArgs e) => Ch1TxB.Text = "0.000";

    private void Ch2Reset_OnClick(object? sender, RoutedEventArgs e) => Ch2TxB.Text = "0.000";

    private void Ch3Reset_OnClick(object? sender, RoutedEventArgs e) => Ch3TxB.Text = "0.000";

    private void Ch1Sli_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        Ch1TxB.Text = e.NewValue.ToString("0.000");
        ProcessPreview();
    }

    private void Ch2Sli_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        Ch2TxB.Text = e.NewValue.ToString("0.000");
        ProcessPreview();
    }

    private void Ch3Sli_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        Ch3TxB.Text = e.NewValue.ToString("0.000");
        ProcessPreview();
    }

    private async void ProcessPreview()
    {
        try
        {
            ImgBox.Source = await ImageManager.GeneratePreview(
                ColorSpaceCombo.SelectedIndex, Ch1Sli.Value, Ch2Sli.Value, Ch3Sli.Value);
        }
        catch (Exception)
        { // ignored
        }
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

    #region File-Related Logic

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
            if (files.Count <= 0 || !await ImageManager.SetImage(files[0])) return;
            Ch1TxB.Text = Ch2TxB.Text = Ch3TxB.Text = "0.000";
            ImgBox.Source = await ImageManager.GeneratePreview(-1);
        }
        catch (Exception)
        { // ignored
        }
    }

    private async void SaveBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (ImageManager.File is null)
                _ = await MessageBoxManager.GetMessageBoxStandard("Warning", "No image is currently loaded.")
                    .ShowAsync();
            else if (Ch1Sli.Value == 0 && Ch2Sli.Value == 0 && Ch3Sli.Value == 0)
                _ = await MessageBoxManager.GetMessageBoxStandard("Warning", "No changes made to the image.")
                    .ShowAsync();
            else
            {
                var file = await StorageProvider.SaveFilePickerAsync(
                    new FilePickerSaveOptions
                    {
                        Title = "Save the Image as...",
                        ShowOverwritePrompt = true,
                        DefaultExtension = ImageManager.Extension,
                        SuggestedFileName = $"PrismPanda_{ImageManager.BareFilename}",
                        SuggestedStartLocation = await ImageManager.File.GetParentAsync()
                    });
                if (file is null) return;
                _ = await ImageManager.AdjustAndSaveImage(
                    file, ColorSpaceCombo.SelectedIndex, Ch1Sli.Value, Ch2Sli.Value, Ch3Sli.Value,
                    FormatCombo.SelectedIndex)
                    ? await MessageBoxManager.GetMessageBoxStandard("Success", "Image saved successfully.").ShowAsync()
                    : await MessageBoxManager.GetMessageBoxStandard("Error", "Failed to save the image.").ShowAsync();
            }
        }
        catch (Exception)
        { // ignored
        }
    }

    #endregion
}