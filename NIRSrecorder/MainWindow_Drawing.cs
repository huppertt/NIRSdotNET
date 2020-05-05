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


    protected void Sdgdraw(object sender, EventArgs e)
    {
        // This is evoked on exposure of the SDG window to draw the probe
        // drawing is handled by the CProbe class and is reused by several GUIs in the code

        if (nirsdata.Count == 0)
        {
            return;
        }


        if (combobox_selectview.ActiveText.Equals("Flat View"))
        {
            nirsdata[combobox_device1.Active].probe.default_display = nirs.probedisplay.TwoDimensional;
        }
        else
        {
            nirsdata[combobox_device1.Active].probe.default_display = nirs.probedisplay.TenTwenty;
        }

        nirsdata[combobox_device1.Active].probe.draw(drawingarea_SDG.GdkWindow);
        drawingarea_SDG.QueueDraw();
        if (DualViewAction.Active)
        {
            if (combobox_selectview.ActiveText.Equals("Flat View"))
            {
                nirsdata[combobox_device2.Active].probe.default_display = nirs.probedisplay.TwoDimensional;
            }
            else
            {
                nirsdata[combobox_device2.Active].probe.default_display = nirs.probedisplay.TenTwenty;
            }

            nirsdata[combobox_device2.Active].probe.draw(drawingarea_SDG2.GdkWindow);
            drawingarea_SDG2.QueueDraw();
        }

        return;
    }

    //-----------------------------------------------------------------------
    // This catches mouse clicks on the probe window and passes the X/Y coordinates to the 
    // probe class to update the measurement list of the probe.
    public void ClickSDG(object o, ButtonReleaseEventArgs args)
    {
        // This is evoked when someone clicks on the SDG window to update the active measurement list of the probe.
        // The actual update is handled by the CProbe class allowing it to be reused
        if (nirsdata.Count == 0)
        {
            return;
        }

        double x = args.Event.X;
        double y = args.Event.Y;

        drawingarea_SDG.GdkWindow.GetSize(out int width, out int height);

        // the reset flag controls if the measurement is expanded from the existing 
        // channels shown or if the list is reset.
        bool reset = true;
        if (args.Event.Button == 3)
        {
            reset = false;  // right clicked
        }

        if (combobox_selectview.ActiveText.Equals("Flat View"))
        {
            // This NIRSdotNET toolbox updateML function handles changing the measurement Active list
            nirsdata[combobox_device1.Active].probe.updateML((int)x, (int)y, reset, width, height);  // update the active measurement list
        }
        else
        {
            // This NIRSdotNET toolbox updateML function handles changing the measurement Active list
            nirsdata[combobox_device1.Active].probe.updateML1020((int)x, (int)y, reset, width, height);  // update the active measurement list
        }

        // update the probe and the data on the the next cycle
        drawingarea_SDG.QueueDraw();
        drawingarea_Data.QueueDraw();
    }


    //-----------------------------------------------------------------------
    // This catches mouse clicks on the probe window and passes the X/Y coordinates to the 
    // probe class to update the measurement list of the probe.
    public void ClickSDG2(object o, ButtonReleaseEventArgs args)
    {
        // This is evoked when someone clicks on the SDG window to update the active measurement list of the probe.
        // The actual update is handled by the CProbe class allowing it to be reused
        if (nirsdata.Count == 0)
        {
            return;
        }
        if (!DualViewAction.Active)
        {
            return;
        }


        double x = args.Event.X;
        double y = args.Event.Y;

        drawingarea_SDG2.GdkWindow.GetSize(out int width, out int height);

        // the reset flag controls if the measurement is expanded from the existing 
        // channels shown or if the list is reset.
        bool reset = true;
        if (args.Event.Button == 3)
        {
            reset = false;  // right clicked
        }

        if (combobox_selectview.ActiveText.Equals("Flat View"))
        {
            // This NIRSdotNET toolbox updateML function handles changing the measurement Active list
            nirsdata[combobox_device2.Active].probe.updateML((int)x, (int)y, reset, width, height);  // update the active measurement list
        }
        else
        {
            // This NIRSdotNET toolbox updateML function handles changing the measurement Active list
            nirsdata[combobox_device2.Active].probe.updateML1020((int)x, (int)y, reset, width, height);  // update the active measurement list
        }
        // update the probe and the data on the the next cycle
        drawingarea_SDG2.QueueDraw();
        drawingarea_Data2.QueueDraw();
    }


    //-----------------------------------------------------------------------
    protected void Datadraw(object sender, EventArgs e)
    {
        if (nirsdata.Count == 0)
        {
            return;
        }
        if (nirsdata[0].data == null)
        {
            return;
        }

        bool autoscale = checkbutton_autoscaleY.Active;

        if (nirsdata[0].time.Count < 1)
        {
            return;
        }
        double tMin = Math.Max(nirsdata[0].time[nirsdata[0].time.Count - 1] - Convert.ToDouble(entry_timeWindow.Text), 0);
        if (!checkbutton_timeWindow.Active)
        {
            tMin = 0;
        }

        // This is evoked on exposure of the main data window to update the drawing
        nirsdata[combobox_device1.Active].draw(drawingarea_Data.GdkWindow, combobox_whichdata.ActiveText, autoscale, tMin);
        if (DualViewAction.Active)
        {
            nirsdata[combobox_device2.Active].draw(drawingarea_Data2.GdkWindow, combobox_whichdata.ActiveText, autoscale, tMin);
            drawingarea_Data2.QueueDraw();
        }
        drawingarea_Data.QueueDraw();
        return;
    }

}