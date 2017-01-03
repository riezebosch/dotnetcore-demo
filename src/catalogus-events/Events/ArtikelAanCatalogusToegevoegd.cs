using System.Collections.Generic;
using catalogus_events.Model;
using System;

namespace catalogus_events.Events
{
    public class ArtikelAanCatalogusToegevoegd
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Beschrijving { get; set; }
        public decimal Prijs { get; set; }
        public DateTime LeverbaarVanaf { get; set; }
        public DateTime LeverbaarTot { get; set; }
        public string LeverancierCode { get; set; }
        public string Leverancier { get; set; }
        public IList<string> Categorieen { get; set; }
    }
}