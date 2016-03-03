using Niuware.MSBandViewer.DataModels;

namespace Niuware.MSBandViewer.MSBand
{
    /// <summary>
    /// Contains all sensors data
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

        /// <summary>
        /// Outputs the values in a formatted string
        /// </summary>
        /// <param name="separator">String values separator</param>
        /// <returns>String with all values</returns>
        public string Output(string separator = ",")
        {
            return heartRate.ToString() + separator + rrInterval + separator + gsr.ToString() + separator + temperature + separator +
                accelerometer.X + separator + accelerometer.Y + separator + accelerometer.Z + separator +
                gyroscopeAngVel.X + separator + gyroscopeAngVel.Y + separator + gyroscopeAngVel.Z + separator +
                contact;
        }

        /// <summary>
        /// Makes a copy of this object
        /// </summary>
        /// <returns>Copy of the object</returns>
        public SensorData Copy()
        {
            return (SensorData)this.MemberwiseClone();
        }
    }
}
