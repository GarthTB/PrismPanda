using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace PrismPanda;

/// <summary> View model for MainWindow.xaml </summary>
internal class MainWindowViewModel : INotifyPropertyChanged
{
    #region Chroma

    private double _chroma = 1;

    public double Chroma
    {
        get => _chroma;
        set
        {
            if (_chroma == value) return;
            _chroma = Math.Clamp(value, 0, 2);
            OnPropertyChanged();
        }
    }

    #endregion

    #region Mode

    public string[] ModeNames { get; } = ["HSI", "TSL", "LCH (CIELab)", "LCH (CIELuv)", "JzCzhz", "OKLCH", "HCT"];

    private int _modeIndex;

    public int ModeIndex
    {
        get => _modeIndex;
        set
        {
            if (_modeIndex == value) return;
            _modeIndex = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Save Format

    public string[] SaveFormatNames { get; } = ["TIFF, 16 bit", "JPEG, Max Q.", "WEBP, lossless", "PNG, 16 bit"];

    private int _saveFormatIndex;

    public int SaveFormatIndex
    {
        get => _saveFormatIndex;
        set
        {
            if (_saveFormatIndex == value) return;
            _saveFormatIndex = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Preview Image

    private WriteableBitmap? _previewBmp;

    public WriteableBitmap? PreviewBmp
    {
        get => _previewBmp;
        set
        {
            if (_previewBmp == value) return;
            _previewBmp = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
