MSBandViewer
======
**MSBandViewer** is an application used for tracking the sensors of a Microsoft Band such as the Heart Rate sensor, RR Interval Sensor, Skin Temperature, etc. so you can then create/export a single file and analize the session data.

The application is currently a Universal Windows Platform App, but can be easily ported to a WPF application.

## Installation

After pulling down the repository, open the Visual Studio solution, then go to the Package Manager Console and install the following NuGet packages:

```PM> Install-Package Microsoft.NETCore.UniversalWindowsPlatform```

```PM> Install-Package Microsoft.Band```

For details on the Microsoft Band SDK please refer to https://developer.microsoftband.com/bandSDK

## Contributors

This application was coded entirely by Erik Lopez.

## License

Please refer to the [LICENSE](https://github.com/niuware/MSBandViewer/blob/master/LICENSE) file
