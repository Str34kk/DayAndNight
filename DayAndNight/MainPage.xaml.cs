using DayAndNight.Themes;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DayAndNight
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<ThemeMessage>(this, ThemeMessage.ThemeChanged, (tm) => UpdateTheme(tm));
        }

        private void UpdateTheme(ThemeMessage tm)
        {
            BackgroundGradien.InvalidateSurface();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<ThemeMessage>(this, ThemeMessage.ThemeChanged);
        }

        private string themeName = "light";

        private void ProfileImage_Tapped(object sender, EventArgs e)
        {
            if (themeName == "light") themeName = "dark";
            else themeName = "light";

            ThemeHelper.ChangeTheme(themeName);
        }

        private SKPaint backgroundBrush = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = Color.Red.ToSKColor()
        };

        private void BackgroundGradien_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKColor gradientStart = ((Color)Application.Current.Resources["BackgroundGradientStartColor"]).ToSKColor();
            SKColor gradientMid = ((Color)Application.Current.Resources["BackgroundGradientMidColor"]).ToSKColor();
            SKColor gradientEnd = ((Color)Application.Current.Resources["BackgroundGradientEndColor"]).ToSKColor();

            backgroundBrush.Shader = SKShader.CreateRadialGradient(
                new SKPoint(0, info.Height * .75f),
                info.Height * .75f,
                new SKColor[]
                {
                    gradientStart, gradientMid,  gradientEnd
                },
                new float[] { 0, 0.3f, 1 },
                SKShaderTileMode.Clamp);

            //backgroundBrush.Shader = SKShader.CreateLinearGradient(
            //                               new SKPoint(0, 0),
            //                               new SKPoint(info.Width, info.Height),
            //                               new SKColor[]
            //                               {
            //                                   gradientStart, gradientEnd
            //                               },
            //                               new float[] { 0, 1 },
            //                               SKShaderTileMode.Clamp);
            SKRect backgroundBounds = new SKRect(0, 0, info.Width, info.Height);
            canvas.DrawRect(backgroundBounds, backgroundBrush);
        }
    }
}