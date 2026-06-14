using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataAndFormat
{
    public class Datamodel
    {
        public string Name { get; set; }
        public string dateDate { get; set; }
        public string Open { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
        public string Close { get; set; }
        public string Volume { get; set; }
    }

    public class stocklistdata
    {
        public string Name { get; set; }
        public string Volume { get; set; }
    }

    public class fatchstockdata
    {
        public string Symbol { get; set; }
        public DateTime Date { get; set; }
        public string Open { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
        public string Close { get; set; }
        public string AClose { get; set; }
        public string Volume { get; set; }


    }

}
