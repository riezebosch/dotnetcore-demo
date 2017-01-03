using System;
using System.Collections.Generic;

namespace catalogus_events.Model
{
    public partial class Categorie
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public IList<ProductCategorie> Producten { get; set; }
    }
}
