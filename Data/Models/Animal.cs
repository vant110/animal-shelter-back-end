using System;
using System.Collections.Generic;

namespace vant110.AnimalShelter.Data.Models
{
    public partial class Animal
    {
        public short AnimalId { get; set; }
        public string Name { get; set; } = null!;
        public short BirthYear { get; set; }
        public DateTime ArrivalDate { get; set; }
        public bool VaccinationStatus { get; set; }
        public bool SterilizationStatus { get; set; }
        public bool ChipStatus { get; set; }
        public string ImageName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public byte SpeciesId { get; set; }

        public virtual Species Species { get; set; } = null!;
    }
}
