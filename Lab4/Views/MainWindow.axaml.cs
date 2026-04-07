using Avalonia.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Lab4.Views;

public partial class MainWindow : Window
{
    private Image<Rgba32>? _image;
    private Bitmap? _bitmap;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Load_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await LoadImageAsync();
    }

    private async Task LoadImageAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);

            var files = await topLevel!.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Wybierz obraz BMP",
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("Bitmap")
                        {
                            Patterns = ["*.bmp"]
                        }
                    ]
                });

            if (files.Count == 0)
                return;

            var path = files[0].TryGetLocalPath();
            if (string.IsNullOrEmpty(path))
                return;

            _image?.Dispose();
            _image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(path);

            UpdatePreview();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void Rotate_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_image == null)
            return;

        float angle = GetSelectedAngle();
        _image.Mutate(x => x.Rotate(angle));
        UpdatePreview();
    }

    private float GetSelectedAngle()
    {
        if (Rotate180Radio.IsChecked == true)
            return 180;

        if (Rotate270Radio.IsChecked == true)
            return 270;

        return 90;
    }

    private void InvertColors_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_image == null)
            return;

        for (int y = 0; y < _image.Height; y++)
        {
            for (int x = 0; x < _image.Width; x++)
            {
                var pixel = _image[x, y];
                pixel.R = (byte)(255 - pixel.R);
                pixel.G = (byte)(255 - pixel.G);
                pixel.B = (byte)(255 - pixel.B);
                _image[x, y] = pixel;
            }
        }

        UpdatePreview();
    }

    private void UpsideDown_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_image == null)
            return;

        _image.Mutate(x => x.Flip(FlipMode.Vertical));
        UpdatePreview();
    }
    private void OnlyGreen_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_image == null)
            return;

        for (int y = 0; y < _image.Height; y++)
        {
            for (int x = 0; x < _image.Width; x++)
            {
                var pixel = _image[x, y];

                bool isGreen =
                    pixel.G > 100 &&
                    pixel.G > pixel.R &&
                    pixel.G > pixel.B;

                if (!isGreen)
                {
                    _image[x, y] = new Rgba32(0, 0, 0);
                }
            }
        }

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_image == null)
            return;

        _bitmap?.Dispose();

        using var ms = new MemoryStream();
        _image.SaveAsBmp(ms);
        ms.Position = 0;

        _bitmap = new Bitmap(ms);
        PreviewImage.Source = _bitmap;
    }
}