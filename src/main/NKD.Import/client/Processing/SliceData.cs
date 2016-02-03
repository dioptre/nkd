using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NKD.Import.Client.Processing
{
    class SliceData
    {
        public List<int> rowIDList { get; set; }

        public float sliceMin { get; set; }

        public float sliceMax { get; set; }

        public float sliceCentre { get; set; }
    }
}
