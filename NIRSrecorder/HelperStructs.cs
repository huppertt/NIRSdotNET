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


public partial class MainWindow : Window
{
    [TreeNode(ListOnly = true)]
public class MyTreeNode : TreeNode
{
    public double onset;
    public double duration;
    public double amp;
    public int index;
    public string condname;

    public MyTreeNode(string name, double onset, double duration, double amp, int index)
    {
        Name = name;
        this.condname = name;
        this.onset = onset;
        this.index = index;
        this.duration = duration;
        this.amp = amp;
    }

    [TreeNodeValue(Column = 0)]
    public string Name;
    [TreeNodeValue(Column = 1)]
    public string Onset { get { return string.Format("{0}", onset); } }
    [TreeNodeValue(Column = 2)]
    public string Duration => string.Format("{0}", duration);
    [TreeNodeValue(Column = 3)]
    public string Amplitude { get { return string.Format("{0}", amp); } }
}




[TreeNode(ListOnly = true)]
public class MyTreeNodeData : TreeNode
{

    public MyTreeNodeData(string filename, string comments)
    {
        FileName = filename;
        Comments = comments;
    }

    [TreeNodeValue(Column = 0)]
    public string FileName;
    [TreeNodeValue(Column = 1)]
    public string Comments;
}


[TreeNode(ListOnly = true)]
public class MyTreeNodeDemo : TreeNode
{

    public MyTreeNodeDemo(string subjid, string group, string age,
        string gender, string headsize)
    {
        SubjID = subjid;
        Group = group;
        Age = age;
        Gender = gender;
        Headsize = headsize;
    }

    [TreeNodeValue(Column = 0)]
    public string SubjID;
    [TreeNodeValue(Column = 1)]
    public string Group;
    [TreeNodeValue(Column = 2)]
    public string Age;
    [TreeNodeValue(Column = 3)]
    public string Gender;
    [TreeNodeValue(Column = 4)]
    public string Headsize;

}
}