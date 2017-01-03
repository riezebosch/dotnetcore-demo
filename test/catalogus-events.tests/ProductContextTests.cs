using catalogus_events.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;

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
        public void ProductenGekoppeldAanCategorieen()
        {
            using (var context = new ProductContext())
            {
                var koppel = context.ProductCategorie.First();
                var product = context.Product.Find(koppel.ProdcatProdId);
                var categorie = context.Categorie.Find(koppel.ProdcatCatId);

                Assert.Contains(categorie, product.Categorieen.Select(pc => pc.Categorie));
            }
        }

        [Fact]
        public void LoadProductenMetCategorieenEnLeverancier()
        {
            using (var context = new ProductContext())
            {
                var products = context
                    .Product
                    .Include(p => p.Categorieen)
                    .ThenInclude(pc => pc.Categorie)
                    .Include(p => p.Leverancier);

                Assert.DoesNotContain(products, p => !p.Categorieen.Any());
                Assert.DoesNotContain(products, p => p.Leverancier == null);
            }
        }
    }
}
