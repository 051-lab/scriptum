using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Core;
using Scriptum.ViewModels;
using System.Windows.Input;

namespace Scriptum.Controls;

public sealed partial class InkCanvasControl : UserControl
{
    private CanvasStrokeStyle _strokeStyle;
    private List<PointF> _currentStroke = new();
    private List<List<PointF>> _strokes = new();
    private bool _isDrawing;
    private float _brushSize = 3f;
    private Windows.UI.Color _brushColor = Windows.UI.Colors.Black;
    private NoteEditorViewModel? _viewModel;

    // Commands for ViewModel integration
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(NoteEditorViewModel), 
            typeof(InkCanvasControl), new PropertyMetadata(null, OnViewModelChanged));

    public NoteEditorViewModel? ViewModel
    {
        get => (NoteEditorViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InkCanvasControl control && e.NewValue is NoteEditorViewModel vm)
        {
            control._viewModel = vm;
        }
    }

    public InkCanvasControl()
    {
        this.InitializeComponent();
        _strokeStyle = new CanvasStrokeStyle
        {
            LineCap = CanvasLineCap.Round,
            LineJoin = CanvasLineJoin.Round
        };
    }

    public float BrushSize
    {
        get => _brushSize;
        set
        {
            _brushSize = value;
            if (_viewModel != null)
            {
                _viewModel.BrushSize = value;
            }
        }
    }

    public Windows.UI.Color BrushColor
    {
        get => _brushColor;
        set
        {
            _brushColor = value;
            if (_viewModel != null)
            {
                _viewModel.BrushColor = value;
            }
        }
    }

    public void Clear()
    {
        _strokes.Clear();
        Canvas.Invalidate();
    }

    public List<List<PointF>> GetStrokes()
    {
        return new List<List<PointF>>(_strokes);
    }

    public void SetStrokes(List<List<PointF>> strokes)
    {
        _strokes = strokes;
        Canvas.Invalidate();
    }

    private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        var session = args.DrawingSession;
        
        foreach (var stroke in _strokes)
        {
            if (stroke.Count < 2) continue;
            
            var points = stroke.ToArray();
            session.DrawGeometry(CreatePathGeometry(points), _brushColor, _brushSize);
        }

        if (_isDrawing && _currentStroke.Count >= 2)
        {
            var points = _currentStroke.ToArray();
            session.DrawGeometry(CreatePathGeometry(points), _brushColor, _brushSize);
        }
    }

    private CanvasPathGeometry CreatePathGeometry(PointF[] points)
    {
        var builder = new CanvasPathBuilder(Canvas);
        builder.BeginFigure(points[0]);
        
        for (int i = 1; i < points.Length; i++)
        {
            builder.AddLine(points[i]);
        }
        
        builder.EndFigure(CanvasFigureLoop.Open);
        return CanvasPathGeometry.Create(builder);
    }

    private void Canvas_PointerPressed(object sender, CanvasPointerEventArgs e)
    {
        _isDrawing = true;
        _currentStroke.Clear();
        var point = e.CurrentPoint.Position;
        var pointF = new PointF((float)point.X, (float)point.Y);
        _currentStroke.Add(pointF);
        
        // Notify ViewModel
        _viewModel?.AddStrokePoint(pointF);
        
        Canvas.Invalidate();
    }

    private void Canvas_PointerMoved(object sender, CanvasPointerEventArgs e)
    {
        if (!_isDrawing) return;
        
        var point = e.CurrentPoint.Position;
        var pointF = new PointF((float)point.X, (float)point.Y);
        _currentStroke.Add(pointF);
        
        // Notify ViewModel
        _viewModel?.AddStrokePoint(pointF);
        
        Canvas.Invalidate();
    }

    private void Canvas_PointerReleased(object sender, CanvasPointerEventArgs e)
    {
        if (!_isDrawing) return;
        
        _isDrawing = false;
        if (_currentStroke.Count >= 2)
        {
            _strokes.Add(new List<PointF>(_currentStroke));
            // Notify ViewModel to complete the stroke
            _viewModel?.CompleteStroke();
        }
        _currentStroke.Clear();
        Canvas.Invalidate();
    }
}
