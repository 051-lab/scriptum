using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Scriptum.Models;
using Scriptum.ViewModels;
using Windows.Foundation;
using Windows.UI;

namespace Scriptum.Views;

/// <summary>
/// Pointer-driven drawing surface for the first Scriptum notebook MVP.
/// </summary>
public sealed partial class PageCanvasView : UserControl
{
    private const double DefaultStrokeThickness = 3.0;
    private static readonly Color InkColor = Color.FromArgb(255, 21, 21, 21);

    private Polyline? _currentPolyline;
    private InkStroke? _currentStroke;
    private uint? _activePointerId;

    public PageCanvasViewModel ViewModel { get; } = new();

    public PageCanvasView()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void DrawingSurface_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (_activePointerId is not null)
        {
            return;
        }

        var pointerPoint = e.GetCurrentPoint(DrawingSurface);
        _activePointerId = pointerPoint.PointerId;
        DrawingSurface.CapturePointer(e.Pointer);

        _currentStroke = new InkStroke
        {
            ColorHex = "#151515",
            Thickness = DefaultStrokeThickness
        };

        _currentPolyline = CreatePolyline(_currentStroke);
        DrawingSurface.Children.Add(_currentPolyline);
        AddPoint(pointerPoint.Position, pointerPoint.Properties.Pressure);

        e.Handled = true;
    }

    private void DrawingSurface_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_currentStroke is null || _currentPolyline is null || _activePointerId is null)
        {
            return;
        }

        var pointerPoint = e.GetCurrentPoint(DrawingSurface);
        if (pointerPoint.PointerId != _activePointerId || !pointerPoint.IsInContact)
        {
            return;
        }

        AddPoint(pointerPoint.Position, pointerPoint.Properties.Pressure);
        e.Handled = true;
    }

    private void DrawingSurface_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        CompleteActiveStroke(e);
    }

    private void DrawingSurface_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        CancelActiveStroke(e);
    }

    private void DrawingSurface_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        CancelActiveStroke(e);
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        var removedStroke = ViewModel.RemoveLastStroke();
        if (removedStroke is null)
        {
            return;
        }

        RemoveLastPolyline();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearPage();
        DrawingSurface.Children.Clear();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveAsync();
    }

    private async void LoadLatestButton_Click(object sender, RoutedEventArgs e)
    {
        var page = await ViewModel.LoadLatestAsync();
        if (page is not null)
        {
            RedrawPage();
        }
    }

    private void CompleteActiveStroke(PointerRoutedEventArgs e)
    {
        if (_currentStroke is not null && _currentStroke.Points.Count > 1)
        {
            ViewModel.AddStroke(_currentStroke);
        }
        else if (_currentPolyline is not null)
        {
            DrawingSurface.Children.Remove(_currentPolyline);
        }

        DrawingSurface.ReleasePointerCapture(e.Pointer);
        ResetActiveStroke();
        e.Handled = true;
    }

    private void CancelActiveStroke(PointerRoutedEventArgs e)
    {
        if (_currentPolyline is not null)
        {
            DrawingSurface.Children.Remove(_currentPolyline);
        }

        DrawingSurface.ReleasePointerCapture(e.Pointer);
        ResetActiveStroke();
        e.Handled = true;
    }

    private void AddPoint(Point point, float pressure)
    {
        if (_currentStroke is null || _currentPolyline is null)
        {
            return;
        }

        _currentStroke.Points.Add(new InkPoint
        {
            X = point.X,
            Y = point.Y,
            Pressure = pressure
        });

        _currentPolyline.Points.Add(point);
    }

    private void RedrawPage()
    {
        DrawingSurface.Children.Clear();

        foreach (var stroke in ViewModel.CurrentPage.Strokes)
        {
            DrawingSurface.Children.Add(CreatePolyline(stroke));
        }
    }

    private static Polyline CreatePolyline(InkStroke stroke)
    {
        var polyline = new Polyline
        {
            Stroke = new SolidColorBrush(InkColor),
            StrokeThickness = stroke.Thickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round
        };

        foreach (var point in stroke.Points)
        {
            polyline.Points.Add(new Point(point.X, point.Y));
        }

        return polyline;
    }

    private void RemoveLastPolyline()
    {
        for (var i = DrawingSurface.Children.Count - 1; i >= 0; i--)
        {
            if (DrawingSurface.Children[i] is Polyline polyline)
            {
                DrawingSurface.Children.Remove(polyline);
                return;
            }
        }
    }

    private void ResetActiveStroke()
    {
        _currentStroke = null;
        _currentPolyline = null;
        _activePointerId = null;
    }
}
