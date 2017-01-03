using System;
using System.Collections.Generic;

namespace catalogus_events.Model
{
    public partial class Categorie
    {
        public int CatId { get; set; }
        public string CatNaam { get; set; }
        public IList<ProductCategorie> Producten { get; set; }
    }
}
