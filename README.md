MSBandViewer
======

![MSBandViewer](http://niuware.github.io/public/assets/MSBandViewer/images/screen_3.png)

**MSBandViewer** is a Universal Windows Platform application used for tracking the sensors of a Microsoft Band such as the Heart Rate sensor, RR Interval Sensor, Skin Temperature, etc. so you can then create and export a single file for analyzing the session data. 

This application uses the custom **LineGraphCanvas** control for drawing time progressive Line Graphs. You can also [check the GIT repository](https://github.com/niuware/LineGraphCanvas) for this custom control.

## Requirements

You need a Microsoft Band device for using this application (tested with Microsoft Band 2).

## Installation

After pulling down the repository, open the Visual Studio solution, then go to the Package Manager Console and install the following NuGet packages:

```PM> Install-Package Microsoft.NETCore.UniversalWindowsPlatform```

```PM> Install-Package Microsoft.Band```

For details on the Microsoft Band SDK please refer to https://developer.microsoftband.com/bandSDK

## Author

This application was coded by Erik Lopez.

## License

Licensed under [GNU General Public License v 3.0](https://github.com/niuware/MSBandViewer/blob/master/LICENSE)
