﻿using Niuware.MSBandViewer.DataModels;

namespace Niuware.MSBandViewer.MSBand
{
    /// <summary>
    /// Contains all sensor data
    /// </summary>
    public class SensorData
    {
        public int heartRate = 0;
        public double rrInterval;
        public int gsr;
        public double temperature;
        public VectorData3D<double> accelerometer;
        public VectorData3D<double> gyroscopeAngVel;
        public bool contact;

        public SensorData() { }

        public string Output(string separator = ",")
        {
            return heartRate.ToString() + separator + rrInterval + separator + gsr.ToString() + separator + temperature + separator +
                accelerometer.X + separator + accelerometer.Y + separator + accelerometer.Z + separator +
                gyroscopeAngVel.X + separator + gyroscopeAngVel.Y + separator + gyroscopeAngVel.Z + separator +
                contact;
        }

        public SensorData Copy()
        {
            return (SensorData)this.MemberwiseClone();
        }
    }
}