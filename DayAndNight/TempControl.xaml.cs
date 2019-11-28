using DayAndNight.Themes;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DayAndNight
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TempControl : ContentView
    {
        public TempControl()
        {
            InitializeComponent();
            percent = 50;
            MessagingCenter.Subscribe<ThemeMessage>(this, ThemeMessage.ThemeChanged, (tm) => UpdateTheme(tm));
        }

        private void UpdateTheme(ThemeMessage tm)
        {
            TempGuageCanvas.InvalidateSurface();
        }

        public double Percent
        {
            get => percent;
            set
            {
                percent = value;
                TempGuageCanvas.InvalidateSurface();
            }
        }

        private double percent;
        private const float bottomPadding = 50f;

        private SKPath clipPath = SKPath.ParseSvgPathData("M.021 28.481a25.933 25.933 0 0 0 8.824-2.112 27.72 27.72 0 0 0 7.391-5.581l19.08-17.045S39.879.5 44.516.5s9.352 3.243 9.352 3.243l20.74 18.628a30.266 30.266 0 0 0 4.525 3.545c3.318 2.263 11.011 2.564 11.011 2.564z");

        private SKPaint redBrush = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = Color.Red.ToSKColor(),
            StrokeWidth = 3
        };

        //bg brush
        private SKPaint backgroundBrush = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = Color.Red.ToSKColor()
        };

        private void TempGuageCanvas_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // objetosc
            float density = info.Size.Width / (float)this.Width;

            var scaledClipPath = new SKPath(clipPath);
            scaledClipPath.Transform(SKMatrix.MakeScale(density, density));
            scaledClipPath.GetTightBounds(out var tightBounds);

            // pozycjonowanie
            var xPos = info.Width * ((float)percent / 100);

            // blokowanie trojkacika

            xPos = Math.Min(Math.Max(xPos, 150), info.Width - 150);

            var translateX = (xPos - tightBounds.MidX);
            var translateY = (info.Height - (tightBounds.Height + bottomPadding));

            using (new SKAutoCanvasRestore(canvas))
            {
                // pozyjonowanie wciecia
                canvas.Translate(translateX, translateY);
                canvas.ClipPath(scaledClipPath, SKClipOperation.Difference, true);
                canvas.Translate(-translateX, -translateY);

                SKColor gradientStart = ((Color)Application.Current.Resources["GaugeGradientStartColor"]).ToSKColor();
                SKColor gradientEnd = ((Color)Application.Current.Resources["GaugeGradientEndColor"]).ToSKColor();

                //gradient
                backgroundBrush.Shader = SKShader.CreateLinearGradient(
                                            new SKPoint(0, 0),
                                            new SKPoint(info.Width, info.Height),
                                            new SKColor[] { gradientStart, gradientEnd },
                                            new float[] { 0, 1 },
                                            SKShaderTileMode.Clamp);
                SKRect backgroundBounds = new SKRect(0, 0, info.Width, info.Height - bottomPadding);
                canvas.DrawRoundRect(backgroundBounds, 20, 20, backgroundBrush);

                //kreski na dole
                var numTicks = 15;
                var distance = info.Width / numTicks;
                var tickHeight = 50;

                for (int i = 1; i < numTicks; i++)
                {
                    var start = new SKPoint(i * distance, info.Height - bottomPadding);
                    var end = new SKPoint(i * distance, info.Height - (tickHeight + bottomPadding));

                    SKPaint tickBruch = new SKPaint()
                    {
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 2,
                    };
                    tickBruch.Shader = SKShader.CreateLinearGradient(
                                            start,
                                            end,
                                            new SKColor[] { new SKColor(255, 255, 255, 100), new SKColor(255, 255, 255, 0) },
                                            new float[] { 0, 1 },
                                            SKShaderTileMode.Clamp);
                    canvas.DrawLine(start, end, tickBruch);
                }

                DrawSvgAtPoint(canvas, new SKPoint(100, info.Height - (bottomPadding + 100)), (float)100, "DayAndNight.Images.Snowflake.svg");
                DrawSvgAtPoint(canvas, new SKPoint(info.Width - 100, info.Height - (bottomPadding + 100)), (float)100, "DayAndNight.Images.Heat.svg");
            }
            // rysowanie małego trójkącika
            using (Stream stream = GetType().Assembly.GetManifestResourceStream("DayAndNight.Images.Thumb.png"))
            {
                SKBitmap bitmap = SKBitmap.Decode(stream);
                var imageHeight = bitmap.Height * .75;
                var imageWidth = bitmap.Width * .75;
                SKRect thumbRect = new SKRect(0, 0, (float)imageWidth, (float)imageHeight);
                thumbRect.Location = new SKPoint(xPos - ((float)imageWidth / 2), info.Height - (bottomPadding + (float)imageHeight / 2));

                //SKPoint location = new SKPoint(xPos - (bitmap.Width / 2), info.Height - bitmap.Height);
                canvas.DrawBitmap(bitmap, thumbRect);
            }
        }

        // funkcja renderujaca obrazki
        private void DrawSvgAtPoint(SKCanvas canvas, SKPoint location, float Size, string svgName)
        {
            using (Stream stream = GetType().Assembly.GetManifestResourceStream(svgName))
            {
                SkiaSharp.Extended.Svg.SKSvg svg = new SkiaSharp.Extended.Svg.SKSvg();
                svg.Load(stream);

                using (new SKAutoCanvasRestore(canvas))
                {
                    SKRect bounds = svg.ViewBox;

                    //ustalanie wielkosci
                    float xRatio = Size / bounds.Width;
                    float yRatio = Size / bounds.Height;
                    float ratio = Math.Min(xRatio, yRatio);

                    canvas.Translate(location.X - bounds.MidX * ratio, location.Y - bounds.MidY * ratio);
                    var matrix = SKMatrix.MakeScale(ratio, ratio);

                    //renderowanie obazka
                    canvas.DrawPicture(svg.Picture, ref matrix);
                }
            }
        }

        // poruszanie sie slidera
        private void TouchEffect_TouchAction(object sender, TouchEffect.TouchActionEventArgs args)
        {
            Percent = (args.Location.X / TempGuageCanvas.Width) * 100;
        }
    }
}