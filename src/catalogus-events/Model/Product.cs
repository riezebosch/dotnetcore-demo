using System;
using System.Collections.Generic;

namespace catalogus_events.Model
{
    public partial class Product
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Beschrijving { get; set; }
        public string AfbeeldingUrl { get; set; }
        public decimal Prijs { get; set; }
        public DateTime LeverbaarVanaf { get; set; }
        public DateTime? LeverbaarTot { get; set; }
        public string LeveranciersProductCode { get; set; }

        public int LeverancierId { get; set; }
        public virtual Leverancier Leverancier { get; set; }

        public IList<ProductCategorie> Categorieen { get; set; }
    }
}
