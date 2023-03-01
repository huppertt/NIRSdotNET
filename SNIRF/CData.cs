using System;
using System.Collections;
using Gdk;
using Gtk;
using System.Collections.Generic;
using MathNet.Numerics;

namespace nirs
{
    public partial class core
    {
    

        public class Data : ICloneable
        {
            public Probe probe;
            public List<Stimulus> stimulus;
            public List<double> time;
            public List<double>[] data;
            public string description;
            public Dictionary demographics;
            public int numsamples;
            public Color[] stimcolor;
            public auxillary[] auxillaries;
            public Guid UUID;

            public Data()
            {
                probe = new Probe();
                stimulus = new List<Stimulus>();
                demographics = new Dictionary();

                stimcolor = new Color[12];
                // for now
                stimcolor[0] = new Color(255, 200, 200);
                stimcolor[1] = new Color(200, 255, 200);
                stimcolor[2] = new Color(200, 200, 255);
                stimcolor[3] = new Color(200, 255, 255);
                stimcolor[4] = new Color(255, 200, 255);
                stimcolor[5] = new Color(255, 255, 200);
                stimcolor[6] = new Color(255, 200, 200);
                stimcolor[7] = new Color(200, 255, 200);
                stimcolor[8] = new Color(200, 200, 255);
                stimcolor[9] = new Color(200, 255, 255);
                stimcolor[10] = new Color(255, 200, 255);
                stimcolor[11] = new Color(255, 255, 200);


            }

            public object Clone(){
                return this.MemberwiseClone();
            }

            public void reset()
            {
                // resets the time and data to empty
                time.Clear();
                for(int i=0; i<data.Length; i++)
                {
                    data[i].Clear();
                }
                numsamples = 0;
            }

            //-------------------------------------------------------------
            public void draw(Gdk.Drawable da,bool autoscale= false, double tMin = 0, bool manualscaleMin = false, double manualMin = -999999, bool manualscaleMax = false, double manualMax = 99999999)
            {

                if (data == null)
                {
                    return;
                }

                string datasubtype = this.probe.ChannelMap[0].datasubtype;
                draw(da,datasubtype,autoscale,tMin,manualscaleMin, manualMin,manualscaleMax,manualMax);
            }



            public void draw(Gdk.Drawable da, string datasubtype, bool autoscale = false, double tMin = 0, bool manualscaleMin = false, double manualMin = -999999, bool manualscaleMax = false, double manualMax = 99999999)
            {
                try {

                    if (data == null)
                    {
                        return;
                    }

                    if (time.Count < 5)
                    {
                        return;
                    }

                    int width, height;
                    da.GetSize(out width, out height);

                    double maxY, minY;
                    minY = 99999; maxY = -99999;


                    int startIdx = 0;
                    for (int sIdx = 0; sIdx < this.time.Count; sIdx++)
                    {
                        if (this.time[sIdx] <= tMin)
                        {
                            startIdx = sIdx;
                        }
                    }

                    int skip = (int)Math.Max(1,Math.Floor((double)((this.time.Count - startIdx) / 1000)));
                    

                    if (this.probe.measlistAct == null)
                    {
                        this.probe.measlistAct = new bool[this.probe.ChannelMap.Length];
                        for (int i = 0; i < this.probe.ChannelMap.Length; i++)
                        {
                            this.probe.measlistAct[i] = false;
                        }
                        this.probe.measlistAct[0] = true;
                    }



                    for (int i = 0; i < this.probe.ChannelMap.Length; i++)
                    {
                        for (int j = startIdx; j < this.data[i].Count; j++)
                        {
                            if (this.probe.ChannelMap[i].datasubtype.ToLower().Equals(datasubtype.ToLower()))
                            {
                                if (this.probe.measlistAct[i])
                                {
                                    double d = this.data[i][j]; // Only shown data will define the scale
                                    if (maxY < d)
                                        maxY = d;
                                    if (minY > d)
                                        minY = d;
                                } else if (!autoscale) // This will include all the data in defining the scale
                                {
                                    double d = this.data[i][j];
                                    if (maxY < d)
                                        maxY = d;
                                    if (minY > d)
                                        minY = d;
                                }
                            }
                        }
                    }

                    if (manualscaleMax) {
                        maxY = manualMax;
                    }
                    if (manualscaleMin) {
                        minY = manualMin;
                    }
                    double rangeY = maxY - minY;
                    double rangeX = this.time[this.time.Count - 1] - this.time[startIdx];
                    int xoffset = 50;
                    int yoffset = 1;

                    height = height - 31;
                    width = width - 51;

                    Gdk.GC gc = new Gdk.GC(da);

                    gc.RgbBgColor = new Gdk.Color(0, 0, 0);
                    gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                    Rectangle rarea = new Rectangle();
                    rarea.X = xoffset - 1;
                    rarea.Y = yoffset - 1;
                    rarea.Height = height + 2;
                    rarea.Width = width + 2;
                    da.DrawRectangle(gc, true, rarea);

                    gc.RgbBgColor = new Color(0, 0, 0);
                    gc.RgbFgColor = new Color(255, 255, 255);
                    rarea = new Rectangle();
                    rarea.X = xoffset;
                    rarea.Y = yoffset;
                    rarea.Height = height;
                    rarea.Width = width;
                    da.DrawRectangle(gc, true, rarea);


                    gc.SetLineAttributes(2, LineStyle.Solid, CapStyle.Projecting, JoinStyle.Round);

                    // Draw stim events

                    for (int j = 0; j < stimulus.Count; j++)
                    {
                        gc.RgbFgColor = stimcolor[j];
                        Rectangle area = new Rectangle();


                        for (int k = 0; k < stimulus[j].onsets.Count; k++)
                        {
                            if (stimulus[j].amplitude[k] > 0 & stimulus[j].onsets[k] + stimulus[j].duration[k] >= this.time[startIdx])
                            {
                                area.Width = (int)(stimulus[j].duration[k] / rangeX * width);
                                if (area.Width == 0) {
                                    area.Width = 1;
                                }
                                area.Height = height;
                                area.X = (int)(xoffset + Math.Max((stimulus[j].onsets[k] - this.time[startIdx]), 0) / rangeX * width);
                                area.Y = yoffset;
                                da.DrawRectangle(gc, true, area);
                            }
                            //da.DrawLine(gc,(int)x+xoffset,yoffset,(int)x+xoffset,(int)height+yoffset);
                        }
                    }


                



                    gc.SetLineAttributes(1, LineStyle.Solid, CapStyle.Projecting, JoinStyle.Round);
                    for (int i = 0; i < this.probe.ChannelMap.Length; i++)
                    {
                        if (this.probe.measlistAct[i] & this.probe.ChannelMap[i].datasubtype.Equals(datasubtype))
                        {
                            gc.RgbFgColor = this.probe.colormap[i];
                            for (int j = 0; j < this.probe.ChannelMap.Length; j++)
                            {
                                if (this.probe.ChannelMap[i].sourceindex == this.probe.ChannelMap[j].sourceindex &
                                   this.probe.ChannelMap[i].detectorindex == this.probe.ChannelMap[j].detectorindex &
                                   this.probe.ChannelMap[j].datasubtype.Equals(this.probe.ChannelMap[0].datasubtype))
                                {
                                    gc.RgbFgColor = this.probe.colormap[j];
                                    break;
                                }
                            }

                            for (int j = startIdx + skip; j < Math.Min(this.data[i].Count, this.time.Count); j=j+skip)
                            {

                                double y2 = (this.data[i][j] - minY) / rangeY * height;
                                double y1 = (this.data[i][j - skip] - minY) / rangeY * height;

                                double x2 = (this.time[j] - this.time[startIdx]) / rangeX * width;
                                double x1 = (this.time[j - skip] - this.time[startIdx]) / rangeX * width;

                                da.DrawLine(gc, (int)x1 + xoffset, (int)(height - y1 + yoffset), (int)x2 + xoffset, (int)(height - y2 + yoffset));
                            }
                        }
                    }


                    if (stimulus.Count > 0)
                    {
                        // add legend to window
                        int w = 0;
                        int h = 0;

                        int maxw = 0;

                        for (int j = 0; j < stimulus.Count; j++)
                        {
                            Gtk.Label lab = new Gtk.Label();
                            gc.RgbFgColor = stimcolor[j];
                            lab.Text = stimulus[j].name;
                            da.DrawLayout(gc, xoffset + 10, yoffset + j * 10 + 10, lab.Layout);
                            lab.Layout.GetPixelSize(out w, out h);
                            if (w > maxw)
                                maxw = w;
                        }
                        gc.RgbBgColor = new Gdk.Color(0, 0, 0);
                        gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                        rarea = new Rectangle();
                        rarea.X = xoffset + 4;
                        rarea.Y = yoffset + 4;
                        rarea.Height = stimulus.Count * 10 + 12;
                        rarea.Width = maxw + 12;
                        da.DrawRectangle(gc, true, rarea);

                        gc.RgbBgColor = new Gdk.Color(0, 0, 0);
                        gc.RgbFgColor = new Gdk.Color(255, 255, 255);
                        rarea = new Rectangle();
                        rarea.X = xoffset + 5;
                        rarea.Y = yoffset + 5;
                        rarea.Height = stimulus.Count * 10 + 10;
                        rarea.Width = maxw + 10;
                        da.DrawRectangle(gc, true, rarea);
                        for (int j = 0; j < stimulus.Count; j++)
                        {
                            Gtk.Label lab = new Gtk.Label();
                            gc.RgbFgColor = stimcolor[j];
                            lab.Text = stimulus[j].name;
                            da.DrawLayout(gc, xoffset + 10, yoffset + j * 10 + 10, lab.Layout);
                        }

                    }

                    gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                    int numxlabels = 10;
                    int numylabels = 5;

                    // Add Xtick marks to the graph
                    double tstart, tend, dt;
                    tstart = this.time[startIdx];
                    tend = this.time[this.time.Count - 1];
                    dt = Math.Round((tend - tstart) / (1 + numxlabels));

                    if (dt < 1)
                        dt = 1;

                    for (double i = 0; i < rangeX; i += dt)
                    {
                        double x = i / rangeX * width;
                        Gtk.Label lab = new Gtk.Label();
                        lab.Text = String.Format("{0}", Math.Round((tstart + i) * 10) / 10);
                        da.DrawLayout(gc, (int)x + xoffset, (int)height + 2, lab.Layout);
                    }

                    double dy;
                    dy = rangeY / (1 + numylabels);

                    if (dy == 0.0)
                        dy = 1;

                    for (double i = 0; i < rangeY; i += dy)
                    {
                        double y = height - i / rangeY * height;
                        Gtk.Label lab = new Gtk.Label();
                        lab.Text = String.Format("{0}", Math.Round((i + minY) * 10) / 10);
                        da.DrawLayout(gc, 10, (int)y + yoffset, lab.Layout);
                    }

                }
                catch
                {
                    
                }
             

            }
        }

    }


}


   
