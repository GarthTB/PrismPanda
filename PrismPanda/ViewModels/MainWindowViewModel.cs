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
            SavePath = FileHelper.DistinctSavePath(path, Extension);
            PreviewBmp = _imageModel.GetPreview(SelectedColourSpace, ChromaGain);
        }
        catch (Exception ex)
        {
            MsgB.OkErr($"Error loading image: {ex.Message}");
            _imageModel = null;
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
                EncodingParams);
            if (success)
                MsgB.OkInf("Success", "Image saved successfully");
            else
                MsgB.OkErr("Some error occurred while saving the image");
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
            var path = FileHelper.PickSavePath();
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
            if (_imageModel is not null)
                PreviewBmp = _imageModel.GetPreview(SelectedColourSpace, ChromaGain);
            OnPropertyChanged();
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
            if (_imageModel is not null)
                PreviewBmp = _imageModel.GetPreview(SelectedColourSpace, ChromaGain);
            OnPropertyChanged();
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
        "PNG"
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
        }
    }

    private string Extension
    => SaveFormatIndex switch
    {
        0 => ".tif",
        1 => ".jpg",
        2 => ".webp",
        3 => ".png",
        _ => throw new NotImplementedException()
    };

    private ImageEncodingParam[] EncodingParams
    => SaveFormatIndex switch
    {
        0 => [new ImageEncodingParam(ImwriteFlags.TiffCompression, 32946)],
        1 => [new ImageEncodingParam(ImwriteFlags.JpegQuality, 100)],
        2 => [new ImageEncodingParam(ImwriteFlags.WebPQuality, 100)],
        3 => [new ImageEncodingParam(ImwriteFlags.PngCompression, 9)],
        _ => throw new NotImplementedException()
    };

    #endregion

    #region Preview Image

    private BitmapSource? _previewBmp;

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

    #endregion

    #region Property Changed Event

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}
