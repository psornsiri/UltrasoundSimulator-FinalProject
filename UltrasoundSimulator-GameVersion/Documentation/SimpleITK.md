# SimpleITK

**NOTE: The SimpleITK importers currently only work on Windows!**

SimpleITK is a library that supports a wide range for formats, such as:
- DICOM (with JPEG compression)
- NRRD
- NIFTI

This project optionally uses SimpleITK for the above formats. There is another fallback DICOM importer, but SimpleITK is a requirement for NRRD and NIFTI.

Since SimpleITK is a native library, that requires you to download some large binaries for each target platform, it has been disabled by default.

To enable SimpleITK, you simply have to do the following:
1. In Unity's top toolbar, click "Volume rendering" and then "Settings", to open the settings menu.
2. In the settings menu, click "Enable SimpleITK"

<img src="img/settings-toolbar.jpg" width="150px">
<img src="img/settings.jpg" width="300px">

This will automatically download the SimpleITK binaries, and enable support for SimpleITK in the code. The `ImporterFactory` class will then return the SimpleITK-based importer implementations.

## Supported platforms

Currently the SimpleITK integration only works on Windows, because the SimpleITK C# wrapper only has distributed binaries for Windows and no other platforms. However, SimpleITK is a cross platform library. To use it on other platforms you could probably try building [the official C# wrapper](https://github.com/SimpleITK/SimpleITK/tree/master/Wrapping/CSharp) for that platform, or manually download the SimpleITK binaries for that platform and create your own C# wrapper. However, I'll look into distributing binaries for at least Linux (which is what I use as a daily driver).
