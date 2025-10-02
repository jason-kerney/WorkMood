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
}

public interface IPaintShim : IDisposable
{
    SKPaint Raw { get; }
}

public class PaintShim(SKPaint paint) : IPaintShim
{
    public SKPaint Raw { get; } = paint;

    public void Dispose()
    {
        paint.Dispose();
    }
}

public interface IBitmapShim : IDisposable
{
    SKBitmap Raw { get; }
}

public class BitmapShim(SKBitmap bitmap) : IBitmapShim
{
    public SKBitmap Raw { get; } = bitmap;

    public void Dispose()
    {
        bitmap.Dispose();
    }
}

public interface ICanvasShim : IDisposable
{
    SKCanvas Raw { get; }

    void Clear(SKColor color);
    void DrawBitmap(IBitmapShim backgroundBitmap, SKRect sKRect);
    void DrawCircle(float x, float y, int radius, IPaintShim startPaint);
    void DrawLine(float left1, float top, float left2, float bottom, IPaintShim axisPaint);
    void DrawPath(SKPath path, IPaintShim paint);
    void DrawText(string text, int x, int y, IPaintShim titlePaint);
    void DrawText(string text, float x, float y, IPaintShim titlePaint);
}

public class CanvasShim(SKCanvas canvas) : ICanvasShim
{
    public SKCanvas Raw { get; } = canvas;

    public void Dispose()
    {
        canvas.Dispose();
    }

    public void Clear(SKColor color)
    {
        canvas.Clear(color);
    }

    public void DrawBitmap(IBitmapShim backgroundBitmap, SKRect sKRect)
    {
        canvas.DrawBitmap(backgroundBitmap.Raw, sKRect);
    }

    public void DrawPath(SKPath path, IPaintShim paint)
    {
        canvas.DrawPath(path, paint.Raw);
    }

    public void DrawText(string text, float x, float y, IPaintShim titlePaint)
    {
        canvas.DrawText(text, x, y, titlePaint.Raw);
    }

    public void DrawText(string text, int x, int y, IPaintShim titlePaint)
    {
        canvas.DrawText(text, x, y, titlePaint.Raw);
    }

    public void DrawLine(float left1, float top, float left2, float bottom, IPaintShim axisPaint)
    {
        canvas.DrawLine(left1, top, left2, bottom, axisPaint.Raw);
    }

    public void DrawCircle(float x, float y, int radius, IPaintShim startPaint)
    {
        canvas.DrawCircle(x, y, radius, startPaint.Raw);
    }
}

public interface IDrawDataShim : IDisposable
{
    SKData Raw { get; }
    byte[] ToArray();
}

public class DrawDataShim(SKData data) : IDrawDataShim
{
    public SKData Raw { get; } = data;

    public byte[] ToArray()
    {
        return data.ToArray();
    }

    public void Dispose()
    {
        data.Dispose();
    }
}

public interface IImageShim : IDisposable
{
    SKImage? Raw { get; }

    IDrawDataShim Encode(SKEncodedImageFormat format, int quality);
}

public class ImageShim(SKImage image) : IImageShim
{
    public SKImage? Raw { get; } = image;

    public IDrawDataShim Encode(SKEncodedImageFormat format, int quality)
    {
        var data = image.Encode(format, quality);
        return new DrawDataShim(data);
    }

    public void Dispose()
    {
        image.Dispose();
    }
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
        };
        return new PaintShim(paint);
    }
}
