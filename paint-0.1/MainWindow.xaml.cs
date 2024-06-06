using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;
using System.Windows.Ink;

namespace PaintApp
{
    public partial class MainWindow : Window
    {
        private bool isEraser = false;
        private Color currentColor = Colors.Black;
        private double brushSize = 5;
        private Point lastPoint;

        public MainWindow()
        {
            InitializeComponent();
            inkCanvas1.StrokeCollected += InkCanvas1_StrokeCollected;
            inkCanvas1.PreviewMouseDown += InkCanvas1_PreviewMouseDown;
            inkCanvas1.PreviewMouseMove += InkCanvas1_PreviewMouseMove;
            inkCanvas1.PreviewMouseUp += InkCanvas1_PreviewMouseUp;
            inkCanvas1.PreviewKeyDown += InkCanvas1_PreviewKeyDown;
            inkCanvas2.StrokeCollected += InkCanvas2_StrokeCollected;
            inkCanvas2.PreviewMouseDown += InkCanvas2_PreviewMouseDown;
            inkCanvas2.PreviewMouseMove += InkCanvas2_PreviewMouseMove;
            inkCanvas2.PreviewMouseUp += InkCanvas2_PreviewMouseUp;
            inkCanvas2.PreviewKeyDown += InkCanvas2_PreviewKeyDown;
        }

        private void btnColorPicker_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                currentColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                btnColorPicker.Background = new SolidColorBrush(currentColor);
                isEraser = false; // Отключение ластика
                btnEraser.Background = Brushes.Transparent;
            }
        }

        private void btnEraser_Click(object sender, RoutedEventArgs e)
        {
            isEraser = !isEraser;
            if (isEraser)
            {
                btnEraser.Background = Brushes.LightGray;
            }
            else
            {
                btnEraser.Background = Brushes.Transparent;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg";
            if (saveFileDialog.ShowDialog() == true)
            {
                RenderTargetBitmap bmp = new RenderTargetBitmap((int)inkCanvas1.ActualWidth, (int)inkCanvas1.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(inkCanvas1);

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                inkCanvas1.Background = new ImageBrush(bitmapImage);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas1.Strokes.Clear();
            inkCanvas1.Background = Brushes.White;
        }

        private void btnBrushSize_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.NumericUpDown numericUpDown = new System.Windows.Forms.NumericUpDown();
            numericUpDown.Value = (decimal)brushSize;
            if (numericUpDown.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                brushSize = (double)numericUpDown.Value;
            }
        }

        private void InkCanvas1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            lastPoint = e.GetPosition(inkCanvas1);
        }

        private void InkCanvas1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(inkCanvas1);
                if (isEraser)
                {
                    // Рисование ластиком
                    inkCanvas1.Strokes.Clear();
                    inkCanvas1.StrokeCollected -= InkCanvas1_StrokeCollected;
                    inkCanvas1.StrokeCollected += InkCanvas1_StrokeCollected;
                    inkCanvas1.StrokeThickness = brushSize;
                    inkCanvas1.StrokeColor = Brushes.White;
                    inkCanvas1.StrokeEndCap = System.Windows.Media.PenLineCap.Round;
                    inkCanvas1.StrokeLineJoin = System.Windows.Media.PenLineJoin.Round;
                    inkCanvas1.StrokeDashCap = System.Windows.Media.PenLineCap.Round;
                    inkCanvas1.StrokeDashArray = new DoubleCollection();
                    inkCanvas1.Strokes.Add(new Stroke(new StylusPointCollection(new[] { lastPoint, currentPoint })));
                }
                else
                {
                    // Рисование кистью
                    inkCanvas1.StrokeThickness = brushSize;
                    inkCanvas1.StrokeColor = new SolidColorBrush(currentColor);
                    inkCanvas1.StrokeEndCap = System.Windows.Media.PenLineCap.Round;
                    inkCanvas1.StrokeLineJoin = System.Windows.Media.PenLineJoin.Round;
                    inkCanvas1.StrokeDashCap = System.Windows.Media.PenLineCap.Round;
                    inkCanvas1.StrokeDashArray = new DoubleCollection();
                    inkCanvas1.Strokes.Add(new Stroke(new StylusPointCollection(new[] { lastPoint, currentPoint })));
                }
                lastPoint = currentPoint;
            }
        }

        private void InkCanvas1_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Сброс состояния ластика
            if (isEraser)
            {
                isEraser = false;
                btnEraser.Background = Brushes.Transparent;
            }
        }

        private void InkCanvas1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                isEraser = !isEraser;
                if (isEraser)
                {
                    btnEraser.Background = Brushes.LightGray;
                }
                else
                {
                    btnEraser.Background = Brushes.Transparent;
                }
            }
        }

        private void InkCanvas1_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Обработка собранных штрихов
        }

        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            inkCanvas1.Width = inkCanvas1.ActualWidth * e.NewValue;
            inkCanvas1.Height = inkCanvas1.ActualHeight * e.NewValue;
        }

        private void sliderHorizontal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            inkCanvas1.Margin = new Thickness(e.NewValue, inkCanvas1.Margin.Top, inkCanvas1.Margin.Right, inkCanvas1.Margin.Bottom);
        }

        private void sliderVertical_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            inkCanvas1.Margin = new Thickness(inkCanvas1.Margin.Left, e.NewValue, inkCanvas1.Margin.Right, inkCanvas1.Margin.Bottom);
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            sliderZoom.Value += 0.1;
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            sliderZoom.Value -= 0.1;
        }

        private void InkCanvas2_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            lastPoint = e.GetPosition(inkCanvas2);
        }

        private void InkCanvas2_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(inkCanvas2);
                if (isEraser)
                {
                    // Рисование ластиком
                    inkCanvas2.Strokes.Clear();
                    inkCanvas2.StrokeCollected -= InkCanvas2_StrokeCollected;
                    inkCanvas2.StrokeCollected += InkCanvas2_StrokeCollected;
                    inkCanvas2.StrokeThickness = brushSize;
                    inkCanvas2.StrokeColor = Brushes.White;
                    inkCanvas2.StrokeEndCap = System.Windows.Media.PenLineCap.Round;
                    inkCanvas2.StrokeLineJoin = System.Windows.Media.PenLineJoin.Round;
                    inkCanvas2.StrokeDashCap = System.Windows.Media.PenLineCap.Round;
                    inkCanvas2.StrokeDashArray = new DoubleCollection();
                    inkCanvas2.Strokes.Add(new Stroke(new StylusPointCollection(new[] { lastPoint, currentPoint })));
                }
                else
                {
                    // Рисование кистью
                    inkCanvas2.StrokeThickness = brushSize;
                    inkCanvas2.StrokeColor = new SolidColorBrush(currentColor);
                    inkCanvas2.StrokeEndCap = System.Windows.Media.PenLineCap.Round;
                    inkCanvas2.StrokeLineJoin = System.Windows.Media.PenLineJoin.Round;
                    inkCanvas2.StrokeDashCap = System.Windows.Media.PenLineCap.Round;
                    inkCanvas2.StrokeDashArray = new DoubleCollection();
                    inkCanvas2.Strokes.Add(new Stroke(new StylusPointCollection(new[] { lastPoint, currentPoint })));
                }
                lastPoint = currentPoint;
            }
        }

        private void InkCanvas2_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Сброс состояния ластика
            if (isEraser)
            {
                isEraser = false;
                btnEraser.Background = Brushes.Transparent;
            }
        }

        private void InkCanvas2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                isEraser = !isEraser;
                if (isEraser)
                {
                    btnEraser.Background = Brushes.LightGray;
                }
                else
                {
                    btnEraser.Background = Brushes.Transparent;
                }
            }
        }

        private void InkCanvas2_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Обработка собранных штрихов
        }
    }
}