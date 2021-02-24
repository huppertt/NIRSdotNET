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

    void SrcClicked(object sender, EventArgs e)
    {

        int WidgetIdx = int.Parse(((Button)sender).Name);
        Lasers laser = _handles.lasers[WidgetIdx];
        int deviceIdx = laser.deviceIdx;
        bool[] state = laser.state;
        string WL = ((Button)sender).Label;
        WL = WL.Remove(WL.Length - 2); // remove the trailing "nm"
        int laserIdx = int.Parse(WL);
        for (int i = 0; i < laser.wavelength.Length; i++)
        {
            if (laser.wavelength[i] == laserIdx)
            {
                laserIdx = i;
                break;
            }
        }


        bool newstate = !state[laserIdx];
        if (checkbutton_linklasers.Active)
        {
            for (int i = 0; i < state.Length; i++)
            {
                if (state[i] != newstate)
                {
                    MainClass.devices[deviceIdx].SetLaserState(laser.laserIdx[i], newstate);
                    state[i] = newstate;
                }
                if (newstate)
                {
                    laser.buttons[i].ModifyBg(StateType.Normal, new Gdk.Color(255, 0, 0));
                    laser.buttons[i].ModifyBg(StateType.Selected, new Gdk.Color(255, 0, 0));
                    laser.buttons[i].ModifyBg(StateType.Active, new Gdk.Color(255, 0, 0));
                    laser.led[i].Color = new Gdk.Color(255, 0, 0);
                }
                else
                {
                    laser.buttons[i].ModifyBg(StateType.Normal, new Gdk.Color(128, 128, 128));
                    laser.buttons[i].ModifyBg(StateType.Selected, new Gdk.Color(128, 128, 128));
                    laser.buttons[i].ModifyBg(StateType.Active, new Gdk.Color(128, 128, 128));
                    laser.led[i].Color = new Gdk.Color(128,128,128);
                }
            }
        }
        else
        {
            MainClass.devices[deviceIdx].SetLaserState(laser.laserIdx[laserIdx], newstate);
            state[laserIdx] = newstate;
            if (newstate)
            {
                laser.buttons[laserIdx].ModifyBg(StateType.Normal, new Gdk.Color(255, 0, 0));
                laser.buttons[laserIdx].ModifyBg(StateType.Selected, new Gdk.Color(255, 0, 0));
                laser.buttons[laserIdx].ModifyBg(StateType.Active, new Gdk.Color(255, 0, 0));
                laser.led[laserIdx].Color = new Gdk.Color(255, 0, 0);
            }
            else
            {
                laser.buttons[laserIdx].ModifyBg(StateType.Normal, new Gdk.Color(128, 128, 128));
                laser.buttons[laserIdx].ModifyBg(StateType.Selected, new Gdk.Color(128, 128, 128));
                laser.buttons[laserIdx].ModifyBg(StateType.Active, new Gdk.Color(128, 128, 128));
                laser.led[laserIdx].Color = new Gdk.Color(128,128,128);
            }
        }
        laser.state = state;
        _handles.lasers[WidgetIdx] = laser;

        bool flag = false;
        for (int i = 0; i < state.Length; i++)
        {
            if (state[i]) { flag = true; }
        }
        if (flag)
        {
            label_srcbottom.Text = "Sources are On";
            colorbutton_srcbottom.Color = new Gdk.Color(255, 0, 0);
            colorbutton4.Color = new Gdk.Color(255, 0, 0);
            button_srcOnOff.Label = "Turn Sources Off";

        }
        else
        {
            label_srcbottom.Text = "Sources are Off";
            colorbutton_srcbottom.Color = new Gdk.Color(128, 128, 128);
            colorbutton4.Color = new Gdk.Color(128, 128, 128);
            button_srcOnOff.Label = "Turn Sources On";
        }

        return;

    }
    void SrcValueChanged(object sender, EventArgs e)
    {
        string[] name = ((SpinButton)sender).Name.Split(' ');
        int WidgetIdx = int.Parse(name[0]);
        Lasers laser = _handles.lasers[WidgetIdx];
        int deviceIdx = laser.deviceIdx;
        int laserIdx = int.Parse(name[1]);

        laser.gain[laserIdx] = (int)((SpinButton)sender).Value;
        _handles.lasers[WidgetIdx] = laser;
        MainClass.devices[deviceIdx].SetLaserPower(laser.laserIdx[laserIdx], laser.gain[laserIdx]);
        return;
    }

    protected void SourcePower_SetAll(object sender, EventArgs e)
    {
        int gain = (int)((SpinButton)sender).Value;
        for (int i = 0; i < _handles.lasers.Count; i++)
        {
            Lasers lasers = _handles.lasers[i];
            for (int k = 0; k < lasers.spinButtons.Length; k++)
            {
                lasers.spinButtons[k].Value = gain;
            }
        }

    }




    protected void LasersAllOnOff(object sender, EventArgs e)
    {
        bool state = ((Button)sender).Label.Equals("Turn Sources On");
        if (state)
        {
            // turn them all off
            ((Button)sender).Label = "Turn Sources Off";
            label_srcbottom.Text = "Sources are On";
            colorbutton_srcbottom.Color = new Gdk.Color(255, 0, 0);
            colorbutton4.Color = new Gdk.Color(255, 0, 0);

            for (int i = 0; i < _handles.lasers.Count; i++)
            {
                Lasers lasers = _handles.lasers[i];
                int deviceIdx = lasers.deviceIdx;
                for (int j = 0; j < lasers.wavelength.Length; j++)
                {
                    MainClass.devices[deviceIdx].SetLaserState(lasers.laserIdx[j], state);
                    lasers.state[j] = state;
                    lasers.buttons[j].ModifyBg(StateType.Normal, new Gdk.Color(255, 0, 0));
                    lasers.buttons[j].ModifyBg(StateType.Selected, new Gdk.Color(255, 0, 0));
                    lasers.buttons[j].ModifyBg(StateType.Active, new Gdk.Color(255, 0, 0));
                    lasers.led[j].Color = new Gdk.Color(255,0,0);
                }
                _handles.lasers[i] = lasers;
            }

        }
        else
        {
            ((Button)sender).Label = "Turn Sources On";
            label_srcbottom.Text = "Sources are Off";
            colorbutton_srcbottom.Color = new Gdk.Color(128, 128, 128);
            colorbutton4.Color = new Gdk.Color(128, 128, 128);

            for (int i = 0; i < _handles.lasers.Count; i++)
            {
                Lasers lasers = _handles.lasers[i];
                int deviceIdx = lasers.deviceIdx;
                for (int j = 0; j < lasers.wavelength.Length; j++)
                {
                    MainClass.devices[deviceIdx].SetLaserState(lasers.laserIdx[j], state);
                    lasers.state[j] = state;
                    lasers.buttons[j].ModifyBg(StateType.Normal, new Gdk.Color(128, 128, 128));
                    lasers.buttons[j].ModifyBg(StateType.Selected, new Gdk.Color(128, 128, 128));
                    lasers.buttons[j].ModifyBg(StateType.Active, new Gdk.Color(128, 128, 128));
                    lasers.led[j].Color = new Gdk.Color(128,128,128);
                }
                _handles.lasers[i] = lasers;
            }

        }

    }

}