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

namespace GFX_3_ColorSpace;

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

    //public void RGB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    //    => _model.UpdateByRgb();
    //public void CMYK_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)// { return; }
    //    => _model.UpdateByCmyk();
}

public class MainWindowVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public Action<Brush> UpdateBrush { get; init; }
    private void SetColor(Color color) => UpdateBrush(new SolidColorBrush(color));

    public byte R { get => _R; set { _R = value; UpdateByRgb(); } }
    byte _R;
    //public byte G { get; set; }
    public byte G { get => _G; set { _G = value; UpdateByRgb(); } }
    byte _G;
    //public byte B { get; set; }
    public byte B { get => _B; set { _B = value; UpdateByRgb(); } }
    byte _B;

    //public CMYK CMYK { get; set; } = new();
    public float C { get => _C; set { _C = value; UpdateByCmyk(); } }
    float _C;
    //public float M { get; set; }
    public float M { get => _M; set { _M = value; UpdateByCmyk(); } }
    float _M;
    //public float Y { get; set; }
    public float Y { get => _Y; set { _Y = value; UpdateByCmyk(); } }
    float _Y;
    //public float K { get; set; }
    public float K { get => _K; set { _K = value; UpdateByCmyk(); } }
    float _K;

    public HSV HSV { get; set; } = new();

    public MainWindowVM UpdateByRgb()
    {
        return this;
        (_C, _M, _Y, _K) = RGB(R, G, B).ToCmyk();
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(C)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(M)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(Y)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(K)));

        HSV = RGB(R, G, B).ToHsv();
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(HSV)));

        SetColor(RGB(R, G, B));
        return this;
    }

    internal MainWindowVM UpdateByCmyk()
    {
        (_R, _G, _B) = CMYK(C, M, Y, K).ToRgb();
        //RGB = CMYK.ToRgb();
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(R)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(G)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(B)));

        HSV = CMYK(C, M, Y, K).ToRgb().ToHsv();
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(HSV)));

        SetColor(RGB(R, G, B));
        return this;
    }

    public static RGB RGB(byte r, byte g, byte b) => new(r, g, b);
    public static CMYK CMYK(float c, float m, float y, float k) => new(c, m, y, k);
}

public record struct RGB(byte R, byte G, byte B)
{
    //public Action Updated { get; init; }

    //public byte R { get => _r; set { _r = value; Updated(); } }
    //byte _r;
    //public byte G { get => _g; set { _g = value; Updated(); } }
    //byte _g;
    //public byte B { get => _b; set { _b = value; Updated(); } }
    //byte _b;

    //public RGB Set(RGB value)
    //{
    //    _r = value.R;
    //    _g = value.G;
    //    _b = value.B;
    //    return this;
    //}

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

    public HSV ToHsv()
    {
        float r = R / 255f;
        float g = G / 255f;
        float b = B / 255f;

        float c_max = MathF.Max(MathF.Max(r, g), b);
        float c_min = MathF.Min(MathF.Min(r, g), b);
        float delta = c_max - c_min;

        float h_ = delta is 0 ? 0
            : c_max == r ? ((g - b) / delta) % 6
            : c_max == g ? ((b - g) / delta) + 2
            : c_max == b ? ((r - g) / delta) + 4
            : throw new InvalidOperationException();

        return new() {
            H= (int)(60 * h_),
            S= c_max is 0 ? 0 : delta / c_max,
            V= c_max
        };
    }

    public void Deconstruct(out byte r, out byte g, out byte b)
        => (r, g, b) = (R, G, B);

    public static implicit operator Color(RGB self)
        => Color.FromRgb(self.R, self.G, self.B);
    //public static implicit operator RGB((byte r, byte g, byte b) self)
    //    => new(self.r, self.g, self.b);
}

public record struct CMYK(float C, float M, float Y, float K)
{
    //public float C { get; set; }
    //public float M { get; set; }
    //public float Y { get; set; }
    //public float K { get; set; }

    public RGB ToRgb()
    {
        return new() {
                R= (byte)(255 * (1-C) * (1-K)),
                G= (byte)(255 * (1-M) * (1-K)),
                B= (byte)(255 * (1-Y) * (1-K))
            };
    }

    public void Deconstruct(out float c, out float m, out float y, out float k)
        => (c, m, y, k) = (C, M, Y, K);
}

public class HSV
{
    /// <summary> In range 0-360 </summary>
    public int H { get; set; }
    public float S { get; set; }
    public float V { get; set; }
}
