using QLBH.ViewModels.Catalog.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBH.Application.Catalog.Products.IRepository
{
    public interface IPublicService
    {
        Task<List<ProductVM>> GetAll(string languageId);
    }
}
