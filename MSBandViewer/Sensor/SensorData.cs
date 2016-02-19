using Niuware.MSBandViewer.DataModel;

namespace Niuware.MSBandViewer.Sensor
{
    public enum SensorType
    {
        HEARTRATE,
        GYROSCOPE,
        GSR,
        TEMPERATURE,
        ACCELEROMETER
    }

    class SensorData
    {
        public VectorData3D<double> gyroscopeAngVel;
        public VectorData3D<double> accelerometer;
        public int heartRate;
        public bool contact;
        public double rrInterval;
        public int gsr;
        public double temperature;

        public SensorData() { }

        public override string ToString()
        {
            return heartRate.ToString() + "," + gyroscopeAngVel.X + "," + gyroscopeAngVel.Y + "," + gyroscopeAngVel.Z;
        }

        public bool IsEmpty()
        {
            if (heartRate == 0)
                return true;

            if (gyroscopeAngVel.X == 0.0 && gyroscopeAngVel.Y == 0.0 && gyroscopeAngVel.Z == 0.0)
                return true;

            return false;
        }
    }
}
