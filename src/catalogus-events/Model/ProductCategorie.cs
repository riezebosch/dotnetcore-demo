using System;
using System.Collections.Generic;

namespace catalogus_events.Model
{
    public partial class ProductCategorie
    {
        public int ProductId { get; set; }
        public int CategorieId { get; set; }
        public Product Product { get; set; }
        public Categorie Categorie { get; set; }
    }
}
