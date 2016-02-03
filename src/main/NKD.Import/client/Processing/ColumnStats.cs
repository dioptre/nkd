using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NKD.Import.Client.Processing
{
    class ColumnStats
    {
        public float min { get; set; }

        public float max { get; set; }

        public float diff { get; set; }

        public int count { get; set; }

        public string message { get; set; }
    }
}
