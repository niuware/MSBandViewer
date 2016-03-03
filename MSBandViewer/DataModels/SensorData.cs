namespace Niuware.MSBandViewer.DataModels
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
            return Helpers.Helper.GetFieldsAsHeaders(typeof(SensorData), separator, "", this);
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
