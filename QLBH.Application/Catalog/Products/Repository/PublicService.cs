using Microsoft.EntityFrameworkCore;
using QLBH.Application.Catalog.Products.IRepository;
using QLBH.Data.EF;
using QLBH.ViewModels.Catalog.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBH.Application.Catalog.Products.Repository
{
    public class PublicService : IPublicService
    {
        private readonly DBContext _context;
        public PublicService(DBContext context)
        {
            _context = context;
            
        }
        public async Task<List<ProductVM>> GetAll(string languageId)
        {
            //1.Select join
            var query = from p in _context.Products
                        join pt in _context.ProductTranslations on p.Id equals pt.ProductId
                        join pic in _context.ProductInCategories on p.Id equals pic.ProductId
                        join c in _context.Categories on pic.CategoryId equals c.Id
                        where pt.LanguageId == languageId
                        select new { p, pt, pic };

            var data = await query.Select(x => new ProductVM()
            {
                Id = x.p.Id,
                Price = x.p.Price,
                OriginalPrice = x.p.OriginalPrice,
                Stock = x.p.Stock,
                ViewCount = x.p.ViewCount,
                DateCreated = x.p.DateCreated,
                Name = x.pt.Name,
                Description = x.pt.Description,
                Details = x.pt.Details,
                SeoDescription = x.pt.SeoDescription,
                SeoTitle = x.pt.SeoTitle,
                SeoAlias = x.pt.SeoAlias,
                LanguageId = x.pt.LanguageId
            }).ToListAsync();
            return data;
        }
    }
}
