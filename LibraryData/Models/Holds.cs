using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreaterSydneyLibraries.Models
{
    public class Holds
    {
        public int Id { get; set; }
        public virtual LibraryAsset LibraryAsset { get; set; }
        public virtual LibraryCard LibraryCard { get; set; }
        public DateTime HoldPlaced { get; set; }
        


    }
}
