using System;
using Gtk;
using NIRSrecorder;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using MathNet.Filtering;
using System.Linq;

public partial class MainWindow : Window
{
    void DetChanged(object sender, EventArgs e)
    {
        int WidgetIdx = int.Parse(((VScale)sender).Name);
        Detector detector = _handles.detectors[WidgetIdx];
        int deviceIdx = detector.deviceIdx;
        int detIdx = detector.detectorIdx;
        int gain = (int)((VScale)sender).Value;
        detector.gain = gain;
        _handles.detectors[WidgetIdx] = detector;
        MainClass.devices[deviceIdx].SetDetGain(detIdx, gain);
        return;
    }




    protected void DetGain_SetAll(object sender, EventArgs e)
    {
        int gain = (int)((SpinButton)sender).Value;
        for (int i = 0; i < _handles.detectors.Count; i++)
        {
            Detector detector = _handles.detectors[i];
            detector.vScale.Value = gain;
        }


    }

}