using catalogus_events.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace catalogus_events
{
    public class ProductRepository
    {
        private ProductContext _context;

        public ProductRepository(ProductContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> LoadProductenMetCategorieenEnLeverancier()
        {
            return _context
                .Product
                .Include(p => p.Categorieen)
                .ThenInclude(pc => pc.Categorie)
                .Include(p => p.Leverancier);
        }
    }

}
