using Microsoft.AspNetCore.Http;
using QLBH.ViewModels.Catalog.Product;
using QLBH.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBH.Application.Catalog.Products.IRepository
{
    public interface IManageProductService
    {
        Task<int> Create(ProductCreateVM request);
        Task<int> Update(ProductUpdateVM request);
        Task<int> Delete(int productId);
        Task<ProductVM> GetById(int productId,string languageId);
        Task<bool> UpdatePrice(int productId, decimal newPrice);
        Task<bool> UpdateStock(int productId, int addQuantity);
        Task AddViewCount(int productId);
        Task<List<ProductVM>> GetAll();
        Task<PagedResult<ProductVM>> GetAllPaging(ProductPagingVM request);
        Task<int> AddImages(int productId, ProductImageCreate request);
        Task<int> RemoveImages( int imageId);
        Task<int> UpdateImages( int imageId, ProductImageUpdate request);
        Task<ProductImageVM> GetImageById(int imageId);
        Task<List<ProductImageVM>> GetListImage(int productId);
    }
}
