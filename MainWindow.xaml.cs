using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GFX_3_ColorSpace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowVM _model;

        public MainWindow()
        {
            InitializeComponent();
            base.DataContext = _model = new MainWindowVM() {
                    // Added this cause I forgot to implement INotifyPropertyChanged, but I think it looks neat soo it stays :>
                    UpdateBrush = (x) => this.ColorRect.Fill = x
                };
            _model.UpdateBrush(Brushes.Black);
        }

        public void RGB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _model.UpdateByRgb();
        }
    }

    public class MainWindowVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Action<Brush> UpdateBrush { get; init; }
        private void SetColor(Color color) => UpdateBrush(new SolidColorBrush(color));

        public RGB RGB { get; set; } = new();
        public CMYK CMYK { get; set; } = new();

        public int H { get; set; }
        public int S { get; set; }
        public int V { get; set; }

        public MainWindowVM UpdateByRgb()
        {
            SetColor(RGB);
            CMYK = RGB.ToCmyk();
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(CMYK)));
            //PropertyChanged(this, new PropertyChangedEventArgs(nameof(M)));
            //PropertyChanged(this, new PropertyChangedEventArgs(nameof(Y)));
            //PropertyChanged(this, new PropertyChangedEventArgs(nameof(K)));
            return this;
        }
    }

    public class RGB
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public CMYK ToCmyk()
        {
            float r = R / 255f;
            float g = G / 255f;
            float b = B / 255f;

            float K = 1f - MathF.Max(MathF.Max(r, g), b);

            return new() {
                    C= (1 - r - K) / (1 - K),
                    M= (1 - g - K) / (1 - K),
                    Y= (1 - b - K) / (1 - K),
                    K= K
                };
        }

        public static implicit operator Color(RGB self)
            => Color.FromRgb(self.R, self.G, self.B);
    }

    public class CMYK
    {
        public float C { get; set; }
        public float M { get; set; }
        public float Y { get; set; }
        public float K { get; set; }

    }
}
