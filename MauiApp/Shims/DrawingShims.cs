using SkiaSharp;

namespace WorkMood.MauiApp.Shims;

public class PaintShimArgs
{
    public SKColor Color { get; set; }
    public float TextSize { get; set; }
    public bool IsAntialias { get; set; }
    public SKTextAlign TextAlign { get; set; }
    public SKTypeface? Typeface { get; set; }
    public SKPaintStyle? Style { get; set; }
    public int StrokeWidth { get; set; }
    public SKPathEffect? PathEffect { get; set; }
}

public interface IPaintShim : IDisposable
{
    SKPaint Raw { get; }
}

public class PaintShim : IPaintShim
{
    private readonly SKPaint _paint;
    
    public PaintShim(SKPaint paint)
    {
        _paint = paint;
    }
    
    public SKPaint Raw => _paint;

    public void Dispose()
    {
        _paint.Dispose();
    }
}

public interface IBitmapShim : IDisposable
{
    SKBitmap Raw { get; }
}

public class BitmapShim : IBitmapShim
{
    private readonly SKBitmap _bitmap;
    
    public BitmapShim(SKBitmap bitmap)
    {
        _bitmap = bitmap;
    }
    
    public SKBitmap Raw => _bitmap;

    public void Dispose()
    {
        _bitmap.Dispose();
    }
}

public interface ICanvasShim : IDisposable
{
    SKCanvas Raw { get; }

    void Clear(SKColor color);
    void Clear(IColorShim color);
    void DrawBitmap(IBitmapShim backgroundBitmap, SKRect sKRect);
    void DrawCircle(float x, float y, int radius, IPaintShim startPaint);
    void DrawLine(float left1, float top, float left2, float bottom, IPaintShim axisPaint);
    void DrawPath(SKPath path, IPaintShim paint);
    void DrawRect(SKRect area, IPaintShim backgroundPaint);
    void DrawText(string text, int x, int y, IPaintShim titlePaint);
    void DrawText(string text, float x, float y, IPaintShim titlePaint);
}

public class CanvasShim : ICanvasShim
{
    private readonly SKCanvas _canvas;
    
    public CanvasShim(SKCanvas canvas)
    {
        _canvas = canvas;
    }
    
    public SKCanvas Raw => _canvas;

    public void Dispose()
    {
        _canvas.Dispose();
    }

    public void Clear(SKColor color)
    {
        _canvas.Clear(color);
    }

    public void Clear(IColorShim color)
    {
        _canvas.Clear(color.Raw);
    }

    public void DrawBitmap(IBitmapShim backgroundBitmap, SKRect sKRect)
    {
        _canvas.DrawBitmap(backgroundBitmap.Raw, sKRect);
    }

    public void DrawPath(SKPath path, IPaintShim paint)
    {
        _canvas.DrawPath(path, paint.Raw);
    }

    public void DrawRect(SKRect area, IPaintShim backgroundPaint)
    {
        _canvas.DrawRect(area, backgroundPaint.Raw);
    }

    public void DrawText(string text, float x, float y, IPaintShim titlePaint)
    {
        _canvas.DrawText(text, x, y, titlePaint.Raw);
    }

    public void DrawText(string text, int x, int y, IPaintShim titlePaint)
    {
        _canvas.DrawText(text, x, y, titlePaint.Raw);
    }

    public void DrawLine(float left1, float top, float left2, float bottom, IPaintShim axisPaint)
    {
        _canvas.DrawLine(left1, top, left2, bottom, axisPaint.Raw);
    }

    public void DrawCircle(float x, float y, int radius, IPaintShim startPaint)
    {
        _canvas.DrawCircle(x, y, radius, startPaint.Raw);
    }
}

public interface IDrawDataShim : IDisposable
{
    SKData Raw { get; }
    byte[] ToArray();
}

public class DrawDataShim : IDrawDataShim
{
    private readonly SKData _data;
    
    public DrawDataShim(SKData data)
    {
        _data = data;
    }
    
    public SKData Raw => _data;

    public byte[] ToArray()
    {
        return _data.ToArray();
    }

    public void Dispose()
    {
        _data.Dispose();
    }
}

public interface IImageShim : IDisposable
{
    SKImage? Raw { get; }

    IDrawDataShim Encode(SKEncodedImageFormat format, int quality);
}

public class ImageShim : IImageShim
{
    private readonly SKImage _image;

    public ImageShim(SKImage image)
    {
        _image = image;
    }

    public SKImage? Raw => _image;

    public IDrawDataShim Encode(SKEncodedImageFormat format, int quality)
    {
        var data = _image.Encode(format, quality);
        return new DrawDataShim(data);
    }

    public void Dispose()
    {
        _image.Dispose();
    }
}

public interface IColorShim
{
    SKColor Raw { get; }
}

public class ColorShim : IColorShim
{
    private readonly SKColor _color;

    public ColorShim(SKColor color)
    {
        _color = color;
    }

    public SKColor Raw => _color;
}

public interface IDrawShimFactory
{
    IImageShim FromRaw(SKImage image);
    IImageShim ImageFromBitmap(SKBitmap bitmap);
    IImageShim ImageFromBitmap(IBitmapShim bitmap);

    ICanvasShim FromRaw(SKCanvas canvas);
    ICanvasShim CanvasFromBitmap(SKBitmap bitmap);
    ICanvasShim CanvasFromBitmap(IBitmapShim bitmap);

    IBitmapShim BitmapFromDimensions(int width, int height);
    IBitmapShim DecodeBitmapFromFile(string filePath);
    IBitmapShim FromRaw(SKBitmap bitmap);

    IPaintShim PaintFromArgs(PaintShimArgs paintShimArgs);

    IColorShim WhiteColor();
    IColorShim LightGrayColor();
    SKColor BlackColor();
    SKColor DarkGrayColor();
    SKColor GrayColor();
}

public class DrawShimFactory : IDrawShimFactory
{
    public IImageShim FromRaw(SKImage image) => new ImageShim(image);
    public IImageShim ImageFromBitmap(SKBitmap bitmap)
    {
        var image = SKImage.FromBitmap(bitmap);
        return FromRaw(image);
    }

    public IImageShim ImageFromBitmap(IBitmapShim bitmap) => ImageFromBitmap(bitmap.Raw);

    public ICanvasShim FromRaw(SKCanvas canvas) => new CanvasShim(canvas);

    public ICanvasShim CanvasFromBitmap(SKBitmap bitmap)
    {
        var canvas = new SKCanvas(bitmap);
        return FromRaw(canvas);
    }

    public ICanvasShim CanvasFromBitmap(IBitmapShim bitmap) => CanvasFromBitmap(bitmap.Raw);

    public IBitmapShim FromRaw(SKBitmap bitmap) => new BitmapShim(bitmap);

    public IBitmapShim BitmapFromDimensions(int width, int height)
    {
        var bitmap = new SKBitmap(width, height);
        return FromRaw(bitmap);
    }
    public IBitmapShim DecodeBitmapFromFile(string filePath)
    {
        var bitmap = SKBitmap.Decode(filePath);
        return FromRaw(bitmap);
    }

    public IPaintShim PaintFromArgs(PaintShimArgs paintShimArgs)
    {
        var paint = new SKPaint
        {
            Color = paintShimArgs.Color,
            TextSize = paintShimArgs.TextSize,
            IsAntialias = paintShimArgs.IsAntialias,
            TextAlign = paintShimArgs.TextAlign,
            Typeface = paintShimArgs.Typeface,
            Style = paintShimArgs.Style ?? SKPaintStyle.Fill,
            StrokeWidth = paintShimArgs.StrokeWidth,
            PathEffect = paintShimArgs.PathEffect,
        };
        return new PaintShim(paint);
    }
    
    public IColorShim WhiteColor() => new ColorShim(SKColors.White);
    public IColorShim LightGrayColor() => new ColorShim(SKColors.LightGray);
    public SKColor BlackColor() => SKColors.Black;
    public SKColor DarkGrayColor() => SKColors.DarkGray;
    public SKColor GrayColor() => SKColors.Gray;
}
