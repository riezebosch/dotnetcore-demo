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
        private static DbContextOptions<ProductContext> Options
        {
            get
            {
                return new DbContextOptionsBuilder<ProductContext>()
                    .UseSqlServer(@"Data Source=.\SQLEXPRESS;Initial Catalog=Product;Integrated Security=SSPI")
                    .Options;
            }
        }

        [Fact]
        public void ReadArtikelenFromDatabase()
        {
            using (var context = new ProductContext(Options))
            {
                Assert.True(context.Product.Any());
            }
        }

        [Fact]
        public void ProductenGekoppeldAanCategorieen()
        {
            using (var context = new ProductContext(Options))
            {
                var koppel = context.ProductCategorie.First();
                var product = context.Product.Find(koppel.ProdcatProdId);
                var categorie = context.Categorie.Find(koppel.ProdcatCatId);

                Assert.Contains(categorie, product.Categorieen.Select(pc => pc.Categorie));
            }
        }

        [Fact]
        public void LoadProductenMetCategorieenEnLeverancierTest()
        {
            using (var context = new ProductContext(Options))
            {
                var repository = new ProductRepository(context);
                var products = repository.LoadProductenMetCategorieenEnLeverancier();

                Assert.DoesNotContain(products, p => !p.Categorieen.Any());
                Assert.DoesNotContain(products, p => p.Leverancier == null);
            }
        }
    }
   
}
