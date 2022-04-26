using Bonsai;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

[Combinator]
[Description("Warps ground plane perspective and keeps vertical plane aligned to top of selected quad.")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class OrthogonalWarp
{
    public OrthogonalWarp()
    {
        Flags = WarpFlags.Linear;
        ScaleX = ScaleY = 1;
    }

    [Range(0.5, 1.5)]
    [Precision(3, 0.05)]
    [Category("AffineTransform")]
    [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
    [Description("The horizontal scale factor to apply to the orthogonal image plane.")]
    public float ScaleX { get; set; }

    [Range(0.5, 1.5)]
    [Precision(3, 0.05)]
    [Category("AffineTransform")]
    [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
    [Description("The horizontal scale factor to apply to the orthogonal image plane.")]
    public float ScaleY { get; set; }

    [Category("AffineTransform")]
    [Description("The translation vector to apply to the orthogonal image plane.")]
    public Point2f Translation { get; set; }

    [Category("PerspectiveTransform")]
    [Description("The coordinates of the four source quadrangle vertices in the input image.")]
    [Editor("Bonsai.Vision.Design.IplImageInputQuadrangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
    public Point2f[] Source { get; set; }

    [Category("PerspectiveTransform")]
    [Description("The coordinates of the four corresponding quadrangle vertices in the output image.")]
    [Editor("Bonsai.Vision.Design.IplImageOutputQuadrangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
    public Point2f[] Destination { get; set; }

    [Description("Specifies the interpolation and operation flags for the image warp.")]
    public WarpFlags Flags { get; set; }

    [Description("The value to which all outlier pixels will be set to.")]
    public Scalar FillValue { get; set; }

    static Point2f[] InitializeQuadrangle(IplImage image)
    {
        return new[]
        {
            new Point2f(0, 0),
            new Point2f(0, image.Height),
            new Point2f(image.Width, image.Height),
            new Point2f(image.Width, 0)
        };
    }

    static Mat CreateAffineTransform(Point2f translation, Point2f scale, Point2f pivot)
    {
        var pivotOffsetX = -pivot.X * scale.X + pivot.X;
        var pivotOffsetY = -pivot.Y * scale.Y + pivot.Y;
        return Mat.FromArray(new float[,]
        {
            { scale.X, 0, pivotOffsetX + translation.X },
            { 0, scale.Y, pivotOffsetY + translation.Y }
        });
    }

    public IObservable<IplImage> Process(IObservable<IplImage> source)
    {
        return Observable.Defer(() =>
        {
            Rect groundPlane = default(Rect);
            Rect orthogonalPlane = default(Rect);
            Mat orthogonalTransform = null;
            Point2f currentScale = Point2f.Zero;
            Point2f currentTranslation = Point2f.Zero;
            Point2f[] currentSource = null;
            Point2f[] correctedSource = null;
            Point2f[] currentDestination = null;
            Point2f[] correctedDestination = null;
            var mapMatrix = new Mat(3, 3, Depth.F32, 1);
            return source.Select(input =>
            {
                var scale = new Point2f(ScaleX, ScaleY);
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                Source = Source ?? InitializeQuadrangle(output);
                Destination = Destination ?? InitializeQuadrangle(output);

                if (Source != currentSource || Destination != currentDestination)
                {
                    currentSource = Source;
                    currentDestination = Destination;
                    
                    // compute ground plane boundary by taking the top of the quadrangle:
                    // all pixels above this boundary are considered to be "vertical" and
                    // all pixels below this boundary are considered to be "floor"
                    var orthogonalBoundary = (int)currentSource.Min(point => point.Y);
                    orthogonalPlane = new Rect(0, 0, input.Width, orthogonalBoundary);
                    groundPlane = new Rect(0, orthogonalBoundary, input.Width, input.Height - orthogonalBoundary);

                    // correct warp perspective to apply only to "floor" region
                    correctedSource = Array.ConvertAll(currentSource, point => new Point2f(point.X, point.Y - orthogonalBoundary));
                    correctedDestination = Array.ConvertAll(currentDestination, point => new Point2f(point.X, point.Y - orthogonalBoundary));
                    CV.GetPerspectiveTransform(correctedSource, correctedDestination, mapMatrix);
                }

                if (currentScale != scale || currentTranslation != Translation)
                {
                    currentScale = scale;
                    currentTranslation = Translation;

                    // to scale height pivot is set to the edge of orthogonal plane
                    // horizontal scale default is image mid-point
                    var scalePivot = new Point2f(input.Width / 2, orthogonalPlane.Height);
                    orthogonalTransform = CreateAffineTransform(currentTranslation, currentScale, scalePivot);
                }

                var flags = Flags;
                var fillValue = FillValue;
                if (currentSource == null) return input;
                using (var orthogonalInput = input.GetSubRect(orthogonalPlane))
                using (var orthogonalOutput = output.GetSubRect(orthogonalPlane))
                {
                    // affine warp "vertical" pixels
                    CV.WarpAffine(orthogonalInput, orthogonalOutput, orthogonalTransform, flags | WarpFlags.FillOutliers, fillValue);
                }

                using (var groundInput = input.GetSubRect(groundPlane))
                using (var groundOutput = output.GetSubRect(groundPlane))
                {
                    // perspective warp "floor" pixels
                    CV.WarpPerspective(groundInput, groundOutput, mapMatrix, flags | WarpFlags.FillOutliers, fillValue);
                }

                return output;
            });
        });
    }
}
