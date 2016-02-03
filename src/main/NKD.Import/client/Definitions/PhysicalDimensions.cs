using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NKD.Import.Client.Definitions
{
    public class PhysicalDimensions
    {

        public double originX;
        public double originY;
        public double originZ;

        public double maximumX;
        public double maximumY;
        public double maximumZ;

        public double parentBlockSize;

        public double blockXWidth;
        public double blockYWidth;
        public double blockZWidth;


        public string message { get; set; }

        internal string GetMessage()
        {
            string mess = "Direction, Min, Max\n";
                mess +=   "X, "+originX+", "+maximumX+"\n";
                mess +=   "Y, "+originY+", "+maximumY+"\n";
                mess +=   "Z, "+originZ+", "+maximumZ+"\n";
            return mess;
        }
    }
}
