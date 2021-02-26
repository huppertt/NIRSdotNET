using System;
using System.Xml;
//using System.Collections.Generic;
//using System.Xml.Serialization;
//using System.IO;
using Gdk;
using System.Collections.Generic;

namespace nirs
{
    public partial class core
    {
        public class Probe
        {
            public double[,] SrcPos;
            public double[,] DetPos;
            public double[,] LandmarkPos;
            public double[,] SrcPos3D;
            public double[,] DetPos3D;
            public double[,] LandmarkPos3D;

            private double[,] ROIdrawPos;

            public List<nirs.ROI> ROIs;

            public int numSrc;
            public int numDet;
            public int numChannels;
            public int numWavelengths;
            public probedisplay default_display;
            public ChannelMap[] ChannelMap;

            public double[] distances;
            public bool isregistered;

            public bool[] measlistAct;

            public string[] SourceLabels;
            public string[] DetectorLabels;
            public string[] LandmarkLabels;

            public int? uselocalIndex;

            // This specifies the colors used for the probe and comes from the DefaultConfig.xml file
            public Gdk.Color[] colormap;
            public Gdk.Color[] colormapstats;



            public Probe()
            {
                default_display = probedisplay.TwoDimensional;

                isregistered = false;
                ROIs = new List<ROI>();

                // Load the colormaps from the XML file
                XmlDocument doc = new XmlDocument();
                doc.Load(@"DefaultConfig.xml");
                XmlNodeList elemList;
                XmlNodeList elemListAll;
                XmlNodeList elemListsub;

                elemListAll = doc.GetElementsByTagName("colormap");
                doc = new XmlDocument();
                doc.LoadXml("<root>" + elemListAll[0].InnerXml + "</root>");
                elemList = doc.GetElementsByTagName("color");

                colormap = new Gdk.Color[elemList.Count];
                
                for (int i = 0; i < elemList.Count; i++)
                {

                    XmlDocument doc2 = new XmlDocument();
                    doc2.LoadXml("<root>" + elemList[i].InnerXml + "</root>");
                    elemListsub = doc2.GetElementsByTagName("R");
                    byte r = Convert.ToByte(elemListsub[0].InnerXml);
                    elemListsub = doc2.GetElementsByTagName("G");
                    byte g = Convert.ToByte(elemListsub[0].InnerXml);
                    elemListsub = doc2.GetElementsByTagName("B");
                    byte b = Convert.ToByte(elemListsub[0].InnerXml);
                 
                    colormap[i] = new Gdk.Color(r, g, b);

                }
                
                doc = new XmlDocument();
                doc.LoadXml("<root>" + elemListAll[1].InnerXml + "</root>");
                elemList = doc.GetElementsByTagName("color");

                colormapstats = new Gdk.Color[elemList.Count];
                for (int i = 0; i < elemList.Count; i++)
                {
                    XmlDocument doc2 = new XmlDocument();
                    doc2.LoadXml("<root>" + elemList[i].InnerXml + "</root>");
                    elemListsub = doc2.GetElementsByTagName("R");
                    byte r = Convert.ToByte(elemListsub[0].InnerXml);
                    elemListsub = doc2.GetElementsByTagName("G");
                    byte g = Convert.ToByte(elemListsub[0].InnerXml);
                    elemListsub = doc2.GetElementsByTagName("B");
                    byte b = Convert.ToByte(elemListsub[0].InnerXml);
                    colormapstats[i] = new Gdk.Color(r, g, b);
                }

            }

            public nirs.core.Probe Clone(){
                nirs.core.Probe other = new Probe();

                other.numDet = this.numDet;
                other.numSrc = this.numSrc;
                other.numChannels = this.numChannels;
                other.numWavelengths = this.numWavelengths;
                other.ROIs = this.ROIs;

                other.SrcPos = this.SrcPos;
                other.DetPos = this.DetPos;
                other.SrcPos3D = this.SrcPos3D;
                other.DetPos3D = this.DetPos3D;
                other.LandmarkPos = this.LandmarkPos;
                other.LandmarkPos3D = this.LandmarkPos3D;
                other.default_display = this.default_display;
                other.ChannelMap = new ChannelMap[this.ChannelMap.Length];
                for(int i=0; i<this.ChannelMap.Length; i++)
                {
                    other.ChannelMap[i] = this.ChannelMap[i];
                }
                other.distances = this.distances;
                other.isregistered = this.isregistered;
                other.measlistAct = new bool[this.measlistAct.Length];
                for(int i=0; i< this.measlistAct.Length; i++)
                {
                    other.measlistAct[i] = this.measlistAct[i];
                }
                other.SourceLabels = this.SourceLabels;
                other.DetectorLabels = this.DetectorLabels;
                other.LandmarkLabels = this.LandmarkLabels;
                other.uselocalIndex = this.uselocalIndex;
                other.colormap = this.colormap;
                other.colormapstats = this.colormapstats;

                return other;

        }

        public void draw(Gdk.Drawable da)
            {
                if (SrcPos == null)
                {
                    return;
                }


             
                // This function draws the SDG probe
                if (default_display == probedisplay.TenTwenty)
                {
                    draw1020(da);
                    return;
                }
                if (default_display == probedisplay.TwoDimensional)
                {
                    draw2D(da);
                    return;
                }

            }


            public void draw2D(Gdk.Drawable da){

                if (SrcPos == null)
                {
                    return;
                }

                int width, height;
                da.GetSize(out width, out height);

                double dx, dy;
                dx = 20;
                dy = 20;
                width = width - 2 * (int)dy;
                height = height - 2 * (int)dx;

                double maxX, minX, maxY, minY;
                maxX = -999; maxY = -999;
                minX = 999; minY = 999;
                for (int i = 0; i < this.numDet; i++)
                {
                    if (maxX < this.DetPos[i, 0])
                        maxX = this.DetPos[i, 0];
                    if (maxY < this.DetPos[i, 1])
                        maxY = this.DetPos[i, 1];
                    if (minX > this.DetPos[i, 0])
                        minX = this.DetPos[i, 0];
                    if (minY > this.DetPos[i, 1])
                        minY = this.DetPos[i, 1];
                }

              
                for (int i = 0; i < this.numSrc; i++)
                {

                    if (maxX < this.SrcPos[i, 0])
                        maxX = this.SrcPos[i, 0];
                    if (maxY < this.SrcPos[i, 1])
                        maxY = this.SrcPos[i, 1];
                    if (minX > this.SrcPos[i, 0])
                        minX = this.SrcPos[i, 0];
                    if (minY > this.SrcPos[i, 1])
                        minY = this.SrcPos[i, 1];
                }


                ROIdrawPos = new double[ROIs.Count, 2];
                for(int i=0; i<ROIs.Count; i++)
                {
                    ROIdrawPos[i, 0] = minX - .1 * (maxX - minX);
                    ROIdrawPos[i, 1] = minY - .2 * (i)*(maxY - minY);
                }
                if (ROIs.Count > 0)
                {
                    minY = minY - (ROIs.Count) * .2 * (maxY - minY);
                    minX = minX - .1 * (maxX - minX);
                }

                double rangeX = maxX - minX;
                double rangeY = maxY - minY;
               
                Gdk.GC gc = new Gdk.GC(da);

                gc.RgbBgColor = new Gdk.Color(0, 0, 0);
                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                Rectangle rarea = new Rectangle();
                rarea.Height = height + 2 * (int)dy;
                rarea.Width = width + 2 * (int)dx;
                da.DrawRectangle(gc, true, rarea);

                gc.RgbBgColor = new Gdk.Color(0, 0, 0);
                gc.RgbFgColor = new Gdk.Color(255, 255, 255);
                rarea = new Rectangle();
                rarea.Height = height - 2 + 2 * (int)dx; ;
                rarea.Width = width - 2 + 2 * (int)dx; ;
                rarea.X = 1;
                rarea.Y = 1;

                da.DrawRectangle(gc, true, rarea);

                int sz = 10;

                if(this.measlistAct==null){
                    this.measlistAct = new bool[this.ChannelMap.Length];
                    for (int i = 0; i < this.ChannelMap.Length; i++){
                        this.measlistAct[i] = false;
                    }
                    this.measlistAct[0] = true;
                }


                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                gc.SetLineAttributes(3, LineStyle.Solid, CapStyle.Projecting, JoinStyle.Round);
                for (int i = 0; i < this.numChannels; i++)
                {
                    if (this.ChannelMap[i].datasubtype.Equals(this.ChannelMap[0].datasubtype))
                    {
                        if (this.measlistAct[i])
                        {
                            gc.RgbFgColor = colormap[i]; //new Gdk.Color (0, 0, 0);
                        }
                        else
                        {
                            gc.RgbFgColor = new Gdk.Color(230, 230, 230);
                        }
                        int si = this.ChannelMap[i].sourceindex;
                        int di = this.ChannelMap[i].detectorindex;

                        double x1 = (this.DetPos[di, 0] - minX) / rangeX * width;
                        double y1 = height-((this.DetPos[di, 1] - minY) / rangeY * height) + dy;
                        double x2 = (this.SrcPos[si, 0] - minX) / rangeX * width;
                        double y2 = height-((this.SrcPos[si, 1] - minY) / rangeY * height)+dy;
                        da.DrawLine(gc, (int)x1, (int)y1, (int)x2, (int)y2);
                        //pts[cnt]=new Gdk.Point((int)x,(int)y);

                    }
                }


                gc.RgbFgColor = new Gdk.Color(0, 255, 0);
                gc.SetLineAttributes(3, LineStyle.Solid, CapStyle.Round, JoinStyle.Round);
                //  Gdk.Point[] pts = new Gdk.Point[this.numdet+this.numsrc];
                for (int i = 0; i < this.numDet; i++)
                {
                    double x = (this.DetPos[i, 0] - minX) / rangeX * width  - sz / 2;
                    double y = height-((this.DetPos[i, 1] - minY) / rangeY * height) - sz / 2 + dy;
                    da.DrawArc(gc, true, (int)x, (int)y, sz, sz, 0, 360 * 64);
                    //pts[cnt]=new Gdk.Point((int)x,(int)y);


                }
                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                 for (int i = 0; i < this.numDet; i++)
                {
                    double x = (this.DetPos[i, 0] - minX) / rangeX * width - sz / 2;
                    double y = height-((this.DetPos[i, 1] - minY) / rangeY * height) - sz / 2 + dy;
              
                    Gtk.Label lab = new Gtk.Label();
                    lab.Text = string.Format("D{0}", i + 1);
                    da.DrawLayout(gc, (int)x, (int)y, lab.Layout);
                }


                gc.RgbBgColor = new Gdk.Color(0, 255, 0);
                gc.RgbFgColor = new Gdk.Color(255, 0, 0);
                gc.SetLineAttributes(3, LineStyle.Solid, CapStyle.Round, JoinStyle.Round);
                for (int i = 0; i < this.numSrc; i++)
                {
                    double x = (this.SrcPos[i, 0] - minX) / rangeX * width - sz / 2;
                    double y = height-((this.SrcPos[i, 1] - minY) / rangeY * height) - sz / 2 + dy;
                    da.DrawArc(gc, true, (int)x, (int)y, sz, sz, 0, 360 * 64);

                }

                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                for (int i = 0; i < this.numSrc; i++)
                {
                    double x = (this.SrcPos[i, 0] - minX) / rangeX * width - sz / 2;
                    double y = height-((this.SrcPos[i, 1] - minY) / rangeY * height) - sz / 2 + dy;
                 
                    Gtk.Label lab = new Gtk.Label();
                    lab.Text = string.Format("S{0}", i + 1);
                    da.DrawLayout(gc, (int)x, (int)y, lab.Layout);
                }

                gc.RgbFgColor = new Gdk.Color(255, 0, 255);
                for (int i = 0; i < ROIs.Count; i++)
                {
                    double x = (ROIdrawPos[i, 0] - minX) / rangeX * width+sz/2;
                    double y = height - ((ROIdrawPos[i, 1] - minY) / rangeY * height) + dy;
                    da.DrawArc(gc, true, (int)x, (int)y, sz, sz, 0, 360 * 64);
                }

                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                for (int i = 0; i < ROIs.Count; i++)
                {
                    double x = (ROIdrawPos[i, 0] - minX) / rangeX * width + sz;
                    double y = height - ((ROIdrawPos[i, 1] - minY) / rangeY * height) + dy;
                    Gtk.Label lab = new Gtk.Label();
                    lab.Text = ROIs[i].name;
                    da.DrawLayout(gc, (int)(x), (int)y, lab.Layout);
                }

                Gtk.Label lab2 = new Gtk.Label();
                lab2.Text = "L";
                da.DrawLayout(gc, (int)(width-5), (int)(height-5), lab2.Layout);



                return;
            }
            public double[,] project1020(double[,] pts3d, int numpts, out double headcirc)
            {
                // Clarke far-side general prospective azumuthal projection
                //  r = -2.4;
                //  R = sqrt(sum(pts.^ 2, 2));
                //  x = r * R.* (pts(:, 1)./ abs(pts(:, 3) - r * R));
                //  y = r * R.* (pts(:, 2)./ abs(pts(:, 3) - r * R));


                double[,] pts2d = new double[numpts, 2];

                double r = -2.4f;
                double Rall = 0; ;
                for (int i = 0; i < numpts; i++)
                {
                    double R = Math.Sqrt(pts3d[i, 0] * pts3d[i, 0] + pts3d[i, 1] * pts3d[i, 1] +
                                         pts3d[i, 2] * pts3d[i, 2]);
                    Rall += R;
                    double x = r * R * (pts3d[i, 0] / Math.Abs(pts3d[i, 2] - r * R));
                    double y = r * R * (pts3d[i, 1] / Math.Abs(pts3d[i, 2] - r * R));
                    pts2d[i, 0] = x;
                    pts2d[i, 1] = y;
                }

                headcirc = Rall / numpts;

                return pts2d;
            }


            public void draw1020(Gdk.Drawable da)
            {
                if (!this.isregistered)
                {
                    draw(da);
                    return;
                }

                double headcirc = 0;
                double[,] lsrcpos = project1020(this.SrcPos3D, this.numSrc, out headcirc);
                double[,] ldetpos = project1020(this.DetPos3D, this.numDet, out headcirc);

                int width, height;
                da.GetSize(out width, out height);

                double dx, dy;
                dx = 10;
                dy = 10;
                width = width - 20;
                height = height - 20;

                double maxX = headcirc * 1.2;
                double minX = -1 * headcirc * 1.2;
                double maxY = headcirc * 1.2;
                double minY = -1 * headcirc * 1.2;


                ROIdrawPos = new double[ROIs.Count, 2];
                for (int i = 0; i < ROIs.Count; i++)
                {
                    ROIdrawPos[i, 0] = minX - .1 * headcirc;
                    ROIdrawPos[i, 1] = minY - .2 * (i) * headcirc;
                }
                if (ROIs.Count > 0)
                {
                    minY = minY - (ROIs.Count) * .2 * headcirc;
                    minX = minX - .1 * headcirc;
                }

                double rangeX = maxX - minX;
                double rangeY = maxY - minY;

                Gdk.GC gc = new Gdk.GC(da);

                gc.RgbBgColor = new Gdk.Color(0, 0, 0);
                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                Rectangle rarea = new Rectangle();
                rarea.Height = height + 20;
                rarea.Width = width + 20;
                da.DrawRectangle(gc, true, rarea);

                gc.RgbBgColor = new Gdk.Color(0, 0, 0);
                gc.RgbFgColor = new Gdk.Color(255, 255, 255);
                rarea = new Rectangle();
                rarea.Height = height + 18;
                rarea.Width = width + 18;
                rarea.X = 1;
                rarea.Y = 1;

                da.DrawRectangle(gc, true, rarea);

                int sz = 10;

                if (this.measlistAct == null)
                {
                    this.measlistAct = new bool[this.ChannelMap.Length];
                    for (int i = 0; i < this.ChannelMap.Length; i++)
                    {
                        this.measlistAct[i] = false;
                    }
                    this.measlistAct[0] = true;
                }


                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                gc.SetLineAttributes(3, LineStyle.Solid, CapStyle.Projecting, JoinStyle.Round);
                for (int i = 0; i < this.numChannels; i++)
                {
                    if (this.ChannelMap[i].datasubtype.Equals(this.ChannelMap[0].datasubtype))
                    {
                        if (this.measlistAct[i])
                        {
                            gc.RgbFgColor = colormap[i]; //new Gdk.Color (0, 0, 0);
                        }
                        else
                        {
                            gc.RgbFgColor = new Gdk.Color(230, 230, 230);
                        }
                        int si = this.ChannelMap[i].sourceindex;
                        int di = this.ChannelMap[i].detectorindex;

                        double x1 = (ldetpos[di, 0] - minX) / rangeX * width + dx;
                        double y1 = (ldetpos[di, 1] - minY) / rangeY * height + dy;
                        double x2 = (lsrcpos[si, 0] - minX) / rangeX * width + dx;
                        double y2 = (lsrcpos[si, 1] - minY) / rangeY * height + dy;
                        da.DrawLine(gc, (int)x1, (int)y1, (int)x2, (int)y2);
                        //pts[cnt]=new Gdk.Point((int)x,(int)y);

                    }
                }


                gc.RgbFgColor = new Gdk.Color(0, 255, 0);
                gc.SetLineAttributes(3, LineStyle.Solid, CapStyle.Round, JoinStyle.Round);
                //  Gdk.Point[] pts = new Gdk.Point[this.numdet+this.numsrc];
                for (int i = 0; i < this.numDet; i++)
                {
                    double x = (ldetpos[i, 0] - minX) / rangeX * width + dx - sz / 2;
                    double y = (ldetpos[i, 1] - minY) / rangeY * height + dy - sz / 2;
                    da.DrawArc(gc, true, (int)x, (int)y, sz, sz, 0, 360 * 64);
                    //pts[cnt]=new Gdk.Point((int)x,(int)y);

                }
                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                for (int i = 0; i < this.numDet; i++)
                {
                    double x = (ldetpos[i, 0] - minX) / rangeX * width + dx - sz / 2;
                    double y = (ldetpos[i, 1] - minY) / rangeY * height + dy - sz / 2;
             
                    Gtk.Label lab = new Gtk.Label();
                    lab.Text = string.Format("D{0}", i + 1);
                    da.DrawLayout(gc, (int)x, (int)y, lab.Layout);

                }


                gc.RgbBgColor = new Gdk.Color(0, 255, 0);
                gc.RgbFgColor = new Gdk.Color(255, 0, 0);
                gc.SetLineAttributes(3, LineStyle.Solid, CapStyle.Round, JoinStyle.Round);
                for (int i = 0; i < this.numSrc; i++)
                {
                    double x = (lsrcpos[i, 0] - minX) / rangeX * width + dx - sz / 2;
                    double y = (lsrcpos[i, 1] - minY) / rangeY * height + dy - sz / 2;
                    da.DrawArc(gc, true, (int)x, (int)y, sz, sz, 0, 360 * 64);

                }


                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                for (int i = 0; i < this.numSrc; i++)
                {
                    double x = (lsrcpos[i, 0] - minX) / rangeX * width + dx - sz / 2;
                    double y = (lsrcpos[i, 1] - minY) / rangeY * height + dy - sz / 2;
                 
                    Gtk.Label lab = new Gtk.Label();
                    lab.Text = string.Format("S{0}", i + 1);
                    da.DrawLayout(gc, (int)x, (int)y, lab.Layout);

                }

                gc.RgbBgColor = new Gdk.Color(0, 0, 0);
                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                double xx = (0) / rangeX * width + dx;
                double yy = (0) / rangeY * height + dy;
                da.DrawArc(gc, false, (int)xx, (int)yy, (int)(width), (int)(height), 0, 360 * 64);



                gc.RgbFgColor = new Gdk.Color(255, 0, 255);
                for (int i = 0; i < ROIs.Count; i++)
                {
                    double x = (ROIdrawPos[i, 0] - minX) / rangeX * width + sz / 2;
                    double y = height - ((ROIdrawPos[i, 1] - minY) / rangeY * height) + dy;
                    da.DrawArc(gc, true, (int)x, (int)y, sz, sz, 0, 360 * 64);
                }

                gc.RgbFgColor = new Gdk.Color(0, 0, 0);
                for (int i = 0; i < ROIs.Count; i++)
                {
                    double x = (ROIdrawPos[i, 0] - minX) / rangeX * width + sz;
                    double y = height - ((ROIdrawPos[i, 1] - minY) / rangeY * height) + dy;
                    Gtk.Label lab = new Gtk.Label();
                    lab.Text = ROIs[i].name;
                    da.DrawLayout(gc, (int)(x), (int)y, lab.Layout);
                }

                Gtk.Label lab2 = new Gtk.Label();
                lab2.Text = "R";
                da.DrawLayout(gc, (int)(width - 5), (int)(height - 5), lab2.Layout);


            }


            public void updateML(int DAx, int DAy, bool reset, int DAwidth, int DAheight,double cutoff=8,double cutoff2=3)
            {
                // This function is called whrn the user clicks on the SDG and used to update the MeasurementList

               // double cutoff = 8;
               // double cutoff2 = 3;

                DAx -= 3;
                DAy += 10;

                int sz = 10;
                double dy;
                dy = 20;
                DAwidth = DAwidth - 20;
                DAheight = DAheight - 20;

                double maxX, minX, maxY, minY;
                maxX = -999; maxY = -999;
                minX = 999; minY = 999;
                for (int i = 0; i < this.numDet; i++)
                {
                    if (maxX < this.DetPos[i, 0])
                        maxX = this.DetPos[i, 0];
                    if (maxY < this.DetPos[i, 1])
                        maxY = this.DetPos[i, 1];
                    if (minX > this.DetPos[i, 0])
                        minX = this.DetPos[i, 0];
                    if (minY > this.DetPos[i, 1])
                        minY = this.DetPos[i, 1];
                }
                for (int i = 0; i < this.numSrc; i++)
                {

                    if (maxX < this.SrcPos[i, 0])
                        maxX = this.SrcPos[i, 0];
                    if (maxY < this.SrcPos[i, 1])
                        maxY = this.SrcPos[i, 1];
                    if (minX > this.SrcPos[i, 0])
                        minX = this.SrcPos[i, 0];
                    if (minY > this.SrcPos[i, 1])
                        minY = this.SrcPos[i, 1];
                }


                ROIdrawPos = new double[ROIs.Count, 2];
                for (int i = 0; i < ROIs.Count; i++)
                {
                    ROIdrawPos[i, 0] = minX - .1 * (maxX - minX);
                    ROIdrawPos[i, 1] = minY - .2 * (i) * (maxY - minY);
                }
                if (ROIs.Count > 0)
                {
                    minY = minY - (ROIs.Count) * .2 * (maxY - minY);
                    minX = minX - .1 * (maxX - minX);
                }

                double rangeX = maxX - minX;
                double rangeY = maxY - minY;

                double distance;
               

                if (this.measlistAct == null)
                {
                    this.measlistAct = new bool[this.ChannelMap.Length];
                    for (int i = 0; i < this.ChannelMap.Length; i++)
                    {
                        this.measlistAct[i] = false;
                    }
                    this.measlistAct[0] = true;
                }


                if (reset)
                {
                    for (int j = 0; j < this.ChannelMap.Length; j++)
                    {
                        this.measlistAct[j] = false;
                    }
                }

                // Do lines
                for (int i = 0; i < this.ChannelMap.Length; i++)
                {
                    int si = this.ChannelMap[i].sourceindex;
                    int di = this.ChannelMap[i].detectorindex;


                    double x1 = (this.DetPos[di, 0] - minX) / rangeX * DAwidth ;
                    double y1 = DAheight-((this.DetPos[di, 1] - minY) / rangeY * DAheight) + dy;
                    double x2 = (this.SrcPos[si, 0] - minX) / rangeX * DAwidth ;
                    double y2 = DAheight-((this.SrcPos[si, 1] - minY) / rangeY * DAheight) + dy;

                    double dist2S, dist2D, distSD;
                    distSD = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
                    dist2D = Math.Sqrt((DAx - x1) * (DAx - x1) + (DAy - y1) * (DAy - y1));
                    dist2S = Math.Sqrt((DAx - x2) * (DAx - x2) + (DAy - y2) * (DAy - y2));

                    if (dist2D + dist2S - distSD < cutoff2)
                    {

                        this.measlistAct[i] = !this.measlistAct[i];
                    }


                }

                for (int i = 0; i < this.numDet; i++)
                {
                    double x = (this.DetPos[i, 0] - minX) / rangeX * DAwidth  - sz / 2;
                    double y = DAheight-((this.DetPos[i, 1] - minY) / rangeY * DAheight) + dy - sz / 2;
                    distance = (DAx - x) * (DAx - x) + (DAy - y) * (DAy - y);

                    if (distance < cutoff * cutoff)
                    {
                        // Selected this detector
                        if (reset)
                        {
                            for (int j = 0; j < this.ChannelMap.Length; j++)
                            {
                                this.measlistAct[j] = false;
                            }
                        }
                        for (int j = 0; j < this.ChannelMap.Length; j++)
                        {
                            if (this.ChannelMap[j].detectorindex == i)
                            {
                                this.measlistAct[j] = true;
                            }
                        }
                    }

                }
                for (int i = 0; i < this.numSrc; i++)
                {
                    double x = (this.SrcPos[i, 0] - minX) / rangeX * DAwidth  - sz / 2;
                    double y = DAheight-((this.SrcPos[i, 1] - minY) / rangeY * DAheight) + dy - sz / 2;
                    distance = (DAx - x) * (DAx - x) + (DAy - y) * (DAy - y);

                    if (distance < cutoff * cutoff)
                    {
                        if (reset)
                        {
                            for (int j = 0; j < this.ChannelMap.Length; j++)
                            {
                                this.measlistAct[j] = false;
                            }
                        }
                        // Selected this detector
                        for (int j = 0; j < this.ChannelMap.Length; j++)
                        {
                            if (this.ChannelMap[j].sourceindex == i)
                            {
                                this.measlistAct[j] = true;
                            }
                        }
                    }

                }




                for (int i = 0; i < ROIs.Count; i++)
                {
                    double x = (ROIdrawPos[i, 0] - minX) / rangeX * DAwidth + sz / 2;
                    double y = DAheight - ((ROIdrawPos[i, 1] - minY) / rangeY * DAheight) + dy;
                    distance = (DAx - x) * (DAx - x) + (DAy - y) * (DAy - y);
                    if (distance < cutoff * cutoff)
                    {
                        selectROI(ROIs[i].name);
                    }
                }


                return;

            }


            public void selectROI(string roiname)
            {
                for(int i=0; i<measlistAct.Length; i++)
                {
                    measlistAct[i] = false;
                }
                for(int i=0; i<ROIs.Count; i++)
                {
                    if (ROIs[i].name.Equals(roiname))
                    {
                        for(int j=0; j<ROIs[i].sourceindex.Count; j++)
                        {
                            for(int k=0; k<measlistAct.Length; k++)
                            {
                                if(ChannelMap[k].sourceindex==ROIs[i].sourceindex[j] &
                                   ChannelMap[k].detectorindex == ROIs[i].detectorindex[j])
                                {
                                    measlistAct[k] = true;
                                }
                            }
                        }
                    }
                }

            }


            public void updateML1020(int DAx, int DAy, bool reset, int DAwidth, int DAheight,double cutoff = 8, double cutoff2 = 3)
            {
                // This function is called whrn the user clicks on the SDG and used to update the MeasurementList
                if (!this.isregistered)
                {
                    updateML(DAx,DAy,reset,DAwidth,DAheight);
                    return;
                }

                //double cutoff = 8;
               // double cutoff2 = 3;

                DAx -= 3;
                DAy += 10;

                double headcirc = 0;
                double[,] lsrcpos = project1020(this.SrcPos3D, this.numSrc, out headcirc);
                double[,] ldetpos = project1020(this.DetPos3D, this.numDet, out headcirc);


                double dy;
                dy = 20;

                double maxX = headcirc * 1.2;
                double minX = -1 * headcirc * 1.2;
                double maxY = headcirc * 1.2;
                double minY = -1 * headcirc * 1.2;

                ROIdrawPos = new double[ROIs.Count, 2];
                for (int i = 0; i < ROIs.Count; i++)
                {
                    ROIdrawPos[i, 0] = minX - .1 * headcirc;
                    ROIdrawPos[i, 1] = minY - .2 * (i) * headcirc;
                }
                if (ROIs.Count > 0)
                {
                    minY = minY - (ROIs.Count) * .2 * headcirc;
                    minX = minX - .1 * headcirc;
                }

                int sz = 10;
                DAwidth = DAwidth - 20;
                DAheight = DAheight - 20;

                double rangeX = maxX - minX;
                double rangeY = maxY - minY;

                double distance;
               

                if (this.measlistAct == null)
                {
                    this.measlistAct = new bool[this.ChannelMap.Length];
                    for (int i = 0; i < this.ChannelMap.Length; i++)
                    {
                        this.measlistAct[i] = false;
                    }
                    this.measlistAct[0] = true;
                }


                if (reset)
                {
                    for (int j = 0; j < this.ChannelMap.Length; j++)
                    {
                        this.measlistAct[j] = false;
                    }
                }

                // Do lines
                for (int i = 0; i < this.ChannelMap.Length; i++)
                {
                    int si = this.ChannelMap[i].sourceindex;
                    int di = this.ChannelMap[i].detectorindex;


                    double x1 = (ldetpos[di, 0] - minX) / rangeX * DAwidth ;
                    double y1 = (ldetpos[di, 1] - minY) / rangeY * DAheight + dy;
                    double x2 = (lsrcpos[si, 0] - minX) / rangeX * DAwidth ;
                    double y2 = (lsrcpos[si, 1] - minY) / rangeY * DAheight + dy;

                    double dist2S, dist2D, distSD;
                    distSD = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
                    dist2D = Math.Sqrt((DAx - x1) * (DAx - x1) + (DAy - y1) * (DAy - y1));
                    dist2S = Math.Sqrt((DAx - x2) * (DAx - x2) + (DAy - y2) * (DAy - y2));

                    if (dist2D + dist2S - distSD < cutoff2)
                    {

                        this.measlistAct[i] = !this.measlistAct[i];
                    }


                }

                for (int i = 0; i < this.numDet; i++)
                {
                    double x = (ldetpos[i, 0] - minX) / rangeX * DAwidth  - sz / 2;
                    double y = (ldetpos[i, 1] - minY) / rangeY * DAheight + dy - sz / 2;
                    distance = (DAx - x) * (DAx - x) + (DAy - y) * (DAy - y);

                    if (distance < cutoff * cutoff)
                    {
                        // Selected this detector
                        if (reset)
                        {
                            for (int j = 0; j < this.ChannelMap.Length; j++)
                            {
                                this.measlistAct[j] = false;
                            }
                        }
                        for (int j = 0; j < this.ChannelMap.Length; j++)
                        {
                            if (this.ChannelMap[j].detectorindex == i)
                            {
                                this.measlistAct[j] = true;
                            }
                        }
                    }

                }
                for (int i = 0; i < this.numSrc; i++)
                {
                    double x = (lsrcpos[i, 0] - minX) / rangeX * DAwidth - sz / 2;
                    double y = (lsrcpos[i, 1] - minY) / rangeY * DAheight + dy - sz / 2;
                    distance = (DAx - x) * (DAx - x) + (DAy - y) * (DAy - y);

                    if (distance < cutoff * cutoff)
                    {
                        if (reset)
                        {
                            for (int j = 0; j < this.ChannelMap.Length; j++)
                            {
                                this.measlistAct[j] = false;
                            }
                        }
                        // Selected this detector
                        for (int j = 0; j < this.ChannelMap.Length; j++)
                        {
                            if (this.ChannelMap[j].sourceindex == i)
                            {
                                this.measlistAct[j] = true;
                            }
                        }
                    }

                }

                for (int i = 0; i < ROIs.Count; i++)
                {
                    double x = (ROIdrawPos[i, 0] - minX) / rangeX * DAwidth + sz / 2;
                    double y = DAheight - ((ROIdrawPos[i, 1] - minY) / rangeY * DAheight) + dy;
                    distance = (DAx - x) * (DAx - x) + (DAy - y) * (DAy - y);
                    if (distance < cutoff * cutoff)
                    {
                        selectROI(ROIs[i].name);
                    }
                }


                return;

            }



        }
    }
}
