MSBandViewer
======
![MSBandViewer](http://niuware.com/MSBandViewer/images/screen_3.png)

**MSBandViewer** is an application used for tracking the sensors of a Microsoft Band such as the Heart Rate sensor, RR Interval Sensor, Skin Temperature, etc. so you can then create/export a single file and analize the session data.

The application is currently a Universal Windows Platform App, but can be easily ported to a WPF application.

## Requirements

You need a Microsoft Band device for using this application (tested with Microsoft Band 2).

## Installation

After pulling down the repository, open the Visual Studio solution, then go to the Package Manager Console and install the following NuGet packages:

```PM> Install-Package Microsoft.NETCore.UniversalWindowsPlatform```

```PM> Install-Package Microsoft.Band```

For details on the Microsoft Band SDK please refer to https://developer.microsoftband.com/bandSDK

## Screenshots

You can check out some cool screenshots of the app [here](http://niuware.com/MSBandViewer/images/screen_0.png), [here](http://niuware.com/MSBandViewer/images/screen_2.png), and [here](http://niuware.com/MSBandViewer/images/screen_4.png).

## Author

This application was coded entirely by Erik Lopez.

## License

Licensed under [GNU General Public License v 3.0](https://github.com/niuware/MSBandViewer/blob/master/LICENSE)
