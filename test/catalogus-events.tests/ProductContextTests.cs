using catalogus_events.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace catalogus_events.tests
{
    public class ProductContextTests
    {
        [Fact]
        public void ReadArtikelenFromDatabase()
        {
            using (var context = new ProductContext())
            {
                Assert.True(context.Product.Any());
            }
        }

        [Fact]
        public void ProductenMetCategorieen()
        {
            using (var context = new ProductContext())
            {
                var koppel = context.ProductCategorie.First();
                var product = context.Product.Find(koppel.ProdcatProdId);
                var categorie = context.Categorie.Find(koppel.ProdcatCatId);

                Assert.True(product.Categorieen.Any());
            }
        }
    }
}
