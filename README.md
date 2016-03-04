MSBandViewer
======
**MSBandViewer** is used for tracking the sensors of a Microsoft Band such as the Heart Rate sensor, RR Interval, Skin Temperature, etc. and then create/export into a session file.

The application is currently a Universal Windows Platform App, but could be easily ported to a WPF application.

## Installation

After pulling down the repository, open the Visual Studio solution, then got to the Package Manager Console and install the following NuGet packages:

```PM> Install-Package Microsoft.NETCore.UniversalWindowsPlatform```

```PM> Install-Package Microsoft.Band```

For details on the Microsoft Band SDK please refer to https://developer.microsoftband.com/bandSDK

## Contributors

This application was coded entirely by Erik Lopez.

## License

Please refer to the [LICENSE](https://github.com/niuware/MSBandViewer/blob/master/LICENSE) file
