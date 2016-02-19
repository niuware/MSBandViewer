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
        public int heartRateBpm;

        public SensorData() { }

        public override string ToString()
        {
            return heartRateBpm.ToString() + "," + gyroscopeAngVel.X + "," + gyroscopeAngVel.Y + "," + gyroscopeAngVel.Z;
        }

        public bool IsEmpty()
        {
            if (heartRateBpm == 0)
                return true;

            if (gyroscopeAngVel.X == 0.0 && gyroscopeAngVel.Y == 0.0 && gyroscopeAngVel.Z == 0.0)
                return true;

            return false;
        }
    }
}
