using Bonsai;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

[Combinator]
[Description("Super-impose two images using weighted blend factor.")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class AlphaBlend
{
    public AlphaBlend()
    {
        Alpha = 0.5;
    }

    [Range(0, 1)]
    [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
    [Description("Determines how much of the foreground is super-imposed.")]
    public double Alpha { get; set; }

    public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage>> source)
    {
        return source.Select(value =>
        {
            var alpha = Alpha;
            var foreground = value.Item1;
            var background = value.Item2;
            var output = new IplImage(background.Size, background.Depth, background.Channels);
            CV.AddWeighted(foreground, alpha, background, 1 - alpha, 0, output);
            return output;
        });
    }
}
