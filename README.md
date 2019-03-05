# NIRSdotNET
NIRS toolbox .NET version

The SNIRF library (need to rename at some point) includes C# .net definitions for a data and probe object class (nirs.core.Data and nirs.core.Probe) which match the structure of the SNIRF data format.  The nirs.io namespace defines read/write methods for *.snirf, *.nirs, and NIRx (read-only) data formats.

The NIRSconverter.exe is a GUI app that reads snirf, nirs, and NIRx data into a "HOMER-like" drawing window.  The data can then be resaved into either .nirs or .snirf formats.

Known issues:
1)  the load commands do not handle all the data fields yet (e.g. demographics and stimulus info from the NIRx and stimulus info from the .nirs).
2)  the SNIRF read/write HDF5 commands only handle double valued data at the moment (and currently does not check for this when reading).
