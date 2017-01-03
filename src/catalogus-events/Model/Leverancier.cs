using System;
using System.Collections.Generic;

namespace catalogus_events.Model
{
    public partial class Leverancier
    {
        public Leverancier()
        {
            Product = new HashSet<Product>();
        }

        public int Id { get; set; }
        public string Naam { get; set; }

        public virtual ICollection<Product> Product { get; set; }
    }
}
