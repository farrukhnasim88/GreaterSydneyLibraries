using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GreaterSydneyLibraries.Models
{
    public class LibraryBranch
    {
        public int Id { get; set; }
        [Required]
        [StringLength (30, ErrorMessage ="Sorry,Maximum 30 Character Only")]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Telephone { get; set; }
        public string Description { get; set; }
        public DateTime OpneDate { get; set; }
        
        public virtual IEnumerable<Customers> Customers { get; set; }
        public virtual IEnumerable<LibraryAsset> GetLibraryAssets { get; set; }
        public string ImageUrl { get; set; }


    }
}
