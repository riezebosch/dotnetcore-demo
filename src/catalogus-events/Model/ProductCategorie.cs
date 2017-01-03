using System;
using System.Collections.Generic;

namespace catalogus_events.Model
{
    public partial class ProductCategorie
    {
        public int ProdcatProdId { get; set; }
        public int ProdcatCatId { get; set; }
        public Product Product { get; set; }
        public Categorie Categorie { get; set; }
    }
}
