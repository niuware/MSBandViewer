using Niuware.MSBandViewer.DataModel;

namespace Niuware.MSBandViewer.Sensor
{
    class SensorData
    {
        public int heartRate = 0;
        public double rrInterval;
        public int gsr;
        public double temperature;
        public VectorData3D<double> accelerometer;
        public VectorData3D<double> gyroscopeAngVel;
        public bool contact;

        public SensorData() { }

        public override string ToString()
        {
            return heartRate.ToString() + "," + rrInterval + "," + gsr.ToString() + "," + temperature + "," + 
                accelerometer.X + "," + accelerometer.Y + "," + accelerometer.Z + "," +
                gyroscopeAngVel.X + "," + gyroscopeAngVel.Y + "," + gyroscopeAngVel.Z + "," +
                contact;
        }

        public SensorData Copy()
        {
            return (SensorData)this.MemberwiseClone();
        }
    }
}
