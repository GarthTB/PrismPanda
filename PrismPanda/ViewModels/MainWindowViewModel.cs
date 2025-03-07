using OpenCvSharp;
using PrismPanda.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Wacton.Unicolour;

namespace PrismPanda.ViewModels;

/// <summary> View model for MainWindow.xaml </summary>
internal class MainWindowViewModel : INotifyPropertyChanged
{
    #region Constructor

    private ImageModel? _imageModel;

    public MainWindowViewModel()
    {
        LoadCommand = new RelayCommand(
            execute: (parameter) => LoadImage(),
            canExecute: (parameter) => true
        );
        SaveCommand = new RelayCommand(
            execute: (parameter) => SaveImage(),
            canExecute: (parameter) => _imageModel is not null
                && !string.IsNullOrWhiteSpace(SavePath)
        );
        BrowseCommand = new RelayCommand(
            execute: (parameter) => BrowseSavePath(),
            canExecute: (parameter) => true
        );
    }

    #endregion

    #region Load Button

    public RelayCommand LoadCommand { get; }

    private void LoadImage()
    {
        try
        {
            var path = FileHelper.PickImage();
            if (path is null) return;
            _imageModel = new ImageModel(path);
            _originalBmp = _imageModel.GetPreview(SelectedColourSpace, 0);
            _processedBmp = null;
            SavePath = FileHelper.DistinctSavePath(path, Extension);
            UpdatePreviewAsync();
        }
        catch (Exception ex)
        {
            MsgB.OkErr($"Error loading image: {ex.Message}");
            _imageModel = null;
            _originalBmp = _processedBmp = null;
            SavePath = null;
        }
    }

    #endregion

    #region Save Button

    public RelayCommand SaveCommand { get; }

    private void SaveImage()
    {
        try
        {
            if (_imageModel is null) return;
            var success = _imageModel.ProcAndSave(
                SelectedColourSpace,
                ChromaGain,
                SavePath ?? throw new InvalidOperationException("Save path is null"),
                Is8BitFormat,
                EncodingParams);
            if (success) MsgB.OkInf("Success", "Image saved successfully");
            else MsgB.OkErr("Some error occurred while saving the image");
        }
        catch (Exception ex)
        {
            MsgB.OkErr($"Error saving image: {ex.Message}");
        }
    }

    #endregion

    #region Browse Button

    public RelayCommand BrowseCommand { get; }

    private void BrowseSavePath()
    {
        try
        {
            var path = FileHelper.PickSavePath(FileFilter);
            if (path is null) return;
            SavePath = path;
        }
        catch (Exception ex)
        {
            MsgB.OkErr($"Error browsing for save path: {ex.Message}");
        }
    }

    #endregion

    #region Chroma Gain

    private double _chromaGain;

    public double ChromaGain
    {
        get => _chromaGain;
        set
        {
            if (_chromaGain == value) return;
            _chromaGain = Math.Clamp(value, -1, 1);
            OnPropertyChanged();
            _processedBmp = null;
            UpdatePreviewAsync();
        }
    }

    #endregion

    #region Mode

    public string[] ModeNames { get; } =
    [
        "HSI",
        "TSL",
        "LCH (CIELab)",
        "LCH (CIELuv)",
        "JzCzhz",
        "OKLCH",
        "HCT"
    ];

    private int _modeIndex;

    public int ModeIndex
    {
        get => _modeIndex;
        set
        {
            if (_modeIndex == value) return;
            _modeIndex = value;
            OnPropertyChanged();
            _processedBmp = null;
            UpdatePreviewAsync();
        }
    }

    private ColourSpace SelectedColourSpace
    => _modeIndex switch
    {
        0 => ColourSpace.Hsi,
        1 => ColourSpace.Tsl,
        2 => ColourSpace.Lchab,
        3 => ColourSpace.Lchuv,
        4 => ColourSpace.Jzczhz,
        5 => ColourSpace.Oklch,
        6 => ColourSpace.Hct,
        _ => throw new ArgumentException("Unsupported colour space")
    };

    #endregion

    #region Save

    private string? _savePath;

    public string? SavePath
    {
        get => _savePath;
        set
        {
            if (_savePath is not null
                && _savePath.Equals(value, StringComparison.OrdinalIgnoreCase))
                return;
            _savePath = value;
            OnPropertyChanged();
        }
    }

    public string[] SaveFormatNames { get; } =
    [
        "TIFF",
        "JPEG, Max Q.",
        "WEBP, lossless",
        "PNG, 8-bit"
    ];

    private int _saveFormatIndex;

    public int SaveFormatIndex
    {
        get => _saveFormatIndex;
        set
        {
            if (_saveFormatIndex == value) return;
            _saveFormatIndex = value;
            OnPropertyChanged();
            if (SavePath is not null)
                SavePath = $"{SavePath[..SavePath.LastIndexOf('.')]}{Extension}";
        }
    }

    private string Extension
    => SaveFormatIndex switch
    {
        0 => ".tif",
        1 => ".jpg",
        2 => ".webp",
        3 => ".png",
        _ => throw new ArgumentException("Unsupported save format")
    };

    private bool Is8BitFormat => SaveFormatIndex != 0;

    private string FileFilter
    => SaveFormatIndex switch
    {
        0 => "TIFF files (*.tif)|*.tif",
        1 => "JPEG files (*.jpg)|*.jpg",
        2 => "WEBP files (*.webp)|*.webp",
        3 => "PNG files (*.png)|*.png",
        _ => throw new ArgumentException("Unsupported save format")
    };

    private ImageEncodingParam[] EncodingParams
    => SaveFormatIndex switch
    {
        0 => [new ImageEncodingParam(ImwriteFlags.TiffCompression, 32946)],
        1 => [new ImageEncodingParam(ImwriteFlags.JpegQuality, 100)],
        2 => [new ImageEncodingParam(ImwriteFlags.WebPQuality, 101)],
        3 => [new ImageEncodingParam(ImwriteFlags.PngCompression, 9)],
        _ => throw new ArgumentException("Unsupported save format")
    };

    #endregion

    #region Preview Image

    private BitmapSource? _originalBmp, _processedBmp, _previewBmp;

    public BitmapSource? PreviewBmp
    {
        get => _previewBmp;
        set
        {
            if (_previewBmp == value) return;
            _previewBmp = value;
            OnPropertyChanged();
        }
    }

    private bool _previewToggle = true;

    public bool PreviewToggle
    {
        get => _previewToggle;
        set
        {
            if (_previewToggle == value) return;
            _previewToggle = value;
            OnPropertyChanged();
            if (_imageModel is not null
                && ChromaGain != 0
                && PreviewToggle
                && _processedBmp is null)
                _processedBmp = _imageModel.GetPreview(SelectedColourSpace, ChromaGain);
            UpdatePreviewAsync();
        }
    }

    private CancellationTokenSource? _debounceCts;

    private async void UpdatePreviewAsync()
    {
        if (_imageModel is null) return;
        if (ChromaGain == 0 || !PreviewToggle)
            PreviewBmp = _originalBmp;
        else if (_processedBmp is not null)
            PreviewBmp = _processedBmp;
        else
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            try
            {
                await Task.Delay(150, _debounceCts.Token).ConfigureAwait(true);
                _processedBmp = _imageModel.GetPreview(SelectedColourSpace, ChromaGain);
                PreviewBmp = _processedBmp;
            }
            catch (TaskCanceledException) { } // ignore cancellation
        }
    }
    #endregion

    #region Property Changed Event

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}
