using Gtk;
using System.Collections.Generic;

// Struct to hold some of the GUI buttons that need to be passed around
public struct Handles
{
    public List<Lasers> lasers;
    public List<Detector> detectors;
    public DrawingArea SDGplot;
    public DrawingArea Dataplot;
    public ComboBox whichdata;

    public CheckButton useLPF;
    public CheckButton useHPF;
    public CheckButton useMOCO;
    public Entry editHPF;
    public Entry editLPF;

    public Gtk.TreeView StimTree;
    public Gtk.TreeView DataTree;
    public Gtk.ListStore stimListStore;
    public Gtk.ListStore dataListStore;
    public Gtk.CheckButton SaveTempFile;
}

// Struct to hold info about the GUI-Source to instrument mappings (including the GUI-controls)
public struct Lasers
{
    public string name;
    public int[] wavelength;
    public bool[] state;
    public int[] gain;
    public int deviceIdx;
    public int[] laserIdx;
    public SpinButton[] spinButtons;
    public Button[] buttons;
    public Frame frame;
    public ColorButton[] led;
}

// Struct to hold info about the GUI-Detector to instrument mappings (including the GUI-controls)
public struct Detector
{
    public string name;
    public int gain;
    public int deviceIdx;
    public int detectorIdx;
    public Frame frame;
    public ColorButton led;
    public VScale vScale;
}