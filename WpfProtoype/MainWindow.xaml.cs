using AForge;
using AForge.Vision.GlyphRecognition;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfProtoype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CamCapturer _camCapturer;

        private GlyphRecognizer _glyphRecognizer;

        public MainWindow()
        {
            InitializeComponent();


            GlyphDatabase glyphDatabase = new GlyphDatabase(5);
            glyphDatabase.Add(new Glyph("Glider", new byte[,] {
                {0,0,0,0,0},
                {0,0,1,0,0},
                {0,1,1,0,0},
                {0,0,1,1,0},
                {0,0,0,0,0},
            }));
            _glyphRecognizer = new GlyphRecognizer(5);
            _glyphRecognizer.GlyphDatabase = glyphDatabase;

            InitializeCapturer();
            InitializeComboBox_Webcams();
            if (ComboBox_Webcams.Items.Count > 0)
            {
                ComboBox_Webcams.SelectedIndex = 0;
            }
        }

        private void InitializeCapturer()
        {
            _camCapturer = new CamCapturer();
            _camCapturer.OnNewFrame += camCapturer_NewFrame;
        }

        private void camCapturer_NewFrame(System.Drawing.Bitmap bitmap)
        {

            var glyphs = _glyphRecognizer.FindGlyphs(bitmap);
            if (glyphs.Count > 0)
            {
                var glyph = glyphs[0];
                List<IntPoint> glyphPoints = glyph.RecognizedGlyph == null ? glyph.Quadrilateral : glyph.RecognizedQuadrilateral;
                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Brushes.Red, 3);

                Graphics g = Graphics.FromImage(bitmap);
                g.DrawPolygon(pen, ToPointsArray(glyphPoints));
                g.DrawEllipse(new System.Drawing.Pen(System.Drawing.Brushes.Green, 3), glyphPoints[0].X, glyphPoints[0].Y, 10, 10);
            }

            BitmapImage bitmapImage = BitmapToBitmapImage(bitmap);
            UpdateImage_Frame(bitmapImage);
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        private void UpdateImage_Frame(BitmapImage bi)
        {
            Image_Frame.Dispatcher.Invoke(() => { Image_Frame.Source = bi; });
        }

        private System.Drawing.Point[] ToPointsArray(List<IntPoint> points)
        {
            int count = points.Count;
            System.Drawing.Point[] pointsArray = new System.Drawing.Point[count];

            for (int i = 0; i < count; i++)
            {
                pointsArray[i] = new System.Drawing.Point(points[i].X, points[i].Y);
            }

            return pointsArray;
        }

        private void ComboBox_Webcams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _camCapturer.ChangeCam(ComboBox_Webcams.SelectedIndex);
            _camCapturer.Start();
        }

        private void InitializeComboBox_Webcams()
        {
            var cameraNames = _camCapturer.GetCameraNames();
            foreach (var item in cameraNames)
            {
                ComboBox_Webcams.Items.Add(item);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _camCapturer.Stop();
        }
    }
}
