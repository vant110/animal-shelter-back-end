using System;
using System.Collections.Generic;

namespace vant110.AnimalShelter.Data.Models
{
    public partial class Species
    {
        public Species()
        {
            Animals = new HashSet<Animal>();
        }

        public byte SpeciesId { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Animal> Animals { get; set; }
    }
}
