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

using static System.MathF;

namespace GFX_3_ColorSpace.MainWindow_Tabs;

/// <summary>
/// Interaction logic for Color.xaml
/// </summary>
public partial class ColorTab : Page
{
    public ColorTabVM _model;

    public ColorTab()
    {
        InitializeComponent();

        base.DataContext = _model = new ColorTabVM()
        {
            // Added this cause I forgot to implement INotifyPropertyChanged, but I think it looks neat soo it stays :>
            UpdateBrush = (x) => this.ColorRect.Fill = x
        };
        _model.UpdateBrush(Brushes.Black);
    }
}


public class ColorTabVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public Action<Brush> UpdateBrush { get; init; }
    private void SetColor(System.Windows.Media.Color color) => UpdateBrush(new SolidColorBrush(color));

    public byte R { get => _R; set { _R = value; UpdateByRgb(); } }
    byte _R;
    public byte G { get => _G; set { _G = value; UpdateByRgb(); } }
    byte _G;
    public byte B { get => _B; set { _B = value; UpdateByRgb(); } }
    byte _B;

    public float C { get => _C; set { _C = value; UpdateByCmyk(); } }
    float _C;
    public float M { get => _M; set { _M = value; UpdateByCmyk(); } }
    float _M;
    public float Y { get => _Y; set { _Y = value; UpdateByCmyk(); } }
    float _Y;
    public float K { get => _K; set { _K = value; UpdateByCmyk(); } }
    float _K;

    public int H { get => _H; set { _H = value; UpdateByHsv(); } }
    int _H;
    public float S { get => _S; set { _S = value; UpdateByHsv(); } }
    float _S;
    public float V { get => _V; set { _V = value; UpdateByHsv(); } }
    float _V;

    ColorTabVM UpdateByRgb()
    {
        (_C, _M, _Y, _K) = RGB(R, G, B).ToCmyk();
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(C)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(M)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(Y)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(K)));

        (_H, _S, _V) = RGB(R, G, B).ToHsv();
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(H)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(S)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(V)));

        SetColor(RGB(R, G, B));
        return this;
    }

    ColorTabVM UpdateByCmyk()
    {
        (_R, _G, _B) = CMYK(C, M, Y, K).ToRgb();
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(R)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(G)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(B)));

        (_H, _S, _V) = CMYK(C, M, Y, K).ToRgb().ToHsv();
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(H)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(S)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(V)));

        SetColor(RGB(R, G, B));
        return this;
    }

    ColorTabVM UpdateByHsv()
    {
        (_R, _G, _B) = HSV(H, S, V).ToRgb();
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(R)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(G)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(B)));

        (_C, _M, _Y, _K) = HSV(H, S, V).ToRgb().ToCmyk();
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(C)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(M)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(Y)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(K)));

        SetColor(RGB(R, G, B));
        return this;
    }

    public static RGB RGB(byte r, byte g, byte b) => new(r, g, b);
    public static CMYK CMYK(float c, float m, float y, float k) => new(c, m, y, k);
    public static HSV HSV(int h, float s, float v) => new(h, s, v);
}

public record struct RGB(byte R, byte G, byte B)
{
    public CMYK ToCmyk()
    {
        float r = R / 255f;
        float g = G / 255f;
        float b = B / 255f;

        float K = 1f - MathF.Max(MathF.Max(r, g), b);

        return new()
        {
            C = (1 - r - K) / (1 - K),
            M = (1 - g - K) / (1 - K),
            Y = (1 - b - K) / (1 - K),
            K = K
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

        return new()
        {
            H = (int)(60 * h_),
            S = c_max is 0 ? 0 : delta / c_max,
            V = c_max
        };
    }

    public static implicit operator System.Windows.Media.Color(RGB self)
        => System.Windows.Media.Color.FromRgb(self.R, self.G, self.B);
}

public record struct CMYK(float C, float M, float Y, float K)
{
    public RGB ToRgb() => new(
            R: (byte)(255 * (1 - C) * (1 - K)),
            G: (byte)(255 * (1 - M) * (1 - K)),
            B: (byte)(255 * (1 - Y) * (1 - K))
        );
}

public record struct HSV(int H, float S, float V)
{
    public RGB ToRgb()
    {
        float c = V * S;
        float x = c * (1 - Abs(((H / 60f) % 2) - 1));
        float m = V - c;

        (float r, float g, float b) =
              H < 60 ? (c, x, 0f)
            : H < 120 ? (x, c, 0f)
            : H < 180 ? (0f, c, x)
            : H < 240 ? (0f, x, c)
            : H < 300 ? (x, 0f, c)
            : H < 360 ? (c, 0f, x)
            : throw new InvalidOperationException();

        return new(
                R: (byte)((r + m) * 255),
                G: (byte)((g + m) * 255),
                B: (byte)((b + m) * 255)
            );
    }
}

