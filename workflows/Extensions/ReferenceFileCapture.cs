using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Bonsai.Vision;
using OpenCV.Net;
using Bonsai.Vision.Design;

[Combinator]
[TypeVisualizer(typeof(ReferenceFileCaptureVisualizer))]
[Description("Generates a sequence of time-aligned frame pairs from the specified focus and reference movies.")]
[WorkflowElementCategory(ElementCategory.Source)]
public class ReferenceFileCapture : FileCapture
{
    readonly FileCapture focusCapture = new FileCapture
    {
        PositionUnits = CapturePosition.Milliseconds
    };

    [Description("The name of the focus movie file.")]
    [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
    public string FocusFileName
    {
        get { return focusCapture.FileName; }
        set { focusCapture.FileName = value; }
    }

    [Precision(0, 10)]
    [Description("The offset, in milliseconds, between the focus and reference movies.")]
    [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
    public double Offset { get; set; }

    public IObservable<PairedFrame> Process()
    {
        return Observable.Defer(() =>
        {
            return Generate().Publish(ps => ps.Zip(focusCapture.Generate(ps.Do(frame =>
            {
                var referenceTime = Capture.GetProperty(CaptureProperty.PosMsec);
                var focusTime = referenceTime + Offset;
                var focusFileCapture = focusCapture.Capture;
                if (focusFileCapture == null)
                {
                    focusCapture.StartPosition = focusTime;
                }
                else focusFileCapture.SetProperty(CaptureProperty.PosMsec, focusTime);
            })), (referenceFrame, focusFrame) => new PairedFrame
            {
                ReferenceFrame = referenceFrame,
                FocusFrame = focusFrame
            }));
        });
    }
}

public class PairedFrame
{
    public IplImage ReferenceFrame;
    public IplImage FocusFrame;
}

class ReferenceFileCaptureVisualizer : FileCaptureVisualizer
{
    protected override void UpdateValues(IList<object> values)
    {
        var frame = (PairedFrame)values[0];
        values[0] = frame.ReferenceFrame;
        base.UpdateValues(values);
    }
}