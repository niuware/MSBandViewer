using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niuware.MSBandViewer.DataModel
{
    public struct SensorValue
    {
        public string Label { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return Label + "," + Timestamp.ToString("HH:mm:ss:fff") + "," + Value;
        }
    }
}
