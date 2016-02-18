using System;

namespace Niuware.MSBandViewer.DataModel
{
    public struct VectorData2D<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
    }

    public struct VectorDataTime3D<T>
    {
        public string Label { get; set; }
        public DateTime Timestamp { get; set; }

        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }

        public override string ToString()
        {
            return Timestamp.ToString("HH:mm:ss:fff") + "," + X.ToString() + "," + Y.ToString() + "," + Z.ToString();
        }
    }
}