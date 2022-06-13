using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QLBH.Application.Catalog.Products.IRepository;
using QLBH.Application.Common;
using QLBH.Data.EF;
using QLBH.Data.Entities;
using QLBH.Utilities.Exceptions;
using QLBH.ViewModels.Catalog.Product;
using QLBH.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace QLBH.Application.Catalog.Products.Repository
{
    public class ManageProductService : IManageProductService
    {
        private readonly DBContext _context; 
        private readonly IStorageService _storageService;
        public ManageProductService(DBContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }
        
        //AddImage
        public async Task<int> AddImages(int productId, ProductImageCreate request)
        {
            var productImage = new ProductImage()
            {
                ProductId = productId,
                Caption = request.Caption,
                IsDefault = request.IsDefault,
                DateCreated = DateTime.UtcNow,
                SortOrder = request.SortOrder
            };
            if(request.ImageFile != null)
            {
                productImage.ImagePath = await this.SaveFile(request.ImageFile);
                productImage.FileSize = request.ImageFile.Length;
            }
            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();
            return productImage.Id;
        }

        //AddViewCount
        public async Task AddViewCount(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            product.ViewCount += 1;
            await _context.SaveChangesAsync();
        }

        //Create Product
        public async Task<int> Create(ProductCreateVM request)
        {
            var product = new Product()
            {
                Price = request.Price,
                OriginalPrice=request.OriginalPrice,
                Stock=request.Stock,
                ViewCount = 0,
                DateCreated = DateTime.UtcNow,
                ProductTranslations = new List<ProductTranslation>()
                {
                    new ProductTranslation()
                    {
                        Name = request.Name,
                        Description = request.Description,
                        Details= request.Details,
                        SeoDescription=request.SeoDescription,
                        SeoAlias=request.SeoAlias,
                        SeoTitle=request.SeoTitle,
                        LanguageId=request.LanguageId
                    }
                }
            };
            //Save Image
            if(request.ThumbnailImage != null)
            {
                product.ProductImages = new List<ProductImage>()
                {
                    new ProductImage()
                    {
                        ImagePath = await this.SaveFile(request.ThumbnailImage),
                        Caption = "Thumbnail Image",
                        IsDefault = true,
                        DateCreated = DateTime.UtcNow,
                        SortOrder = 1,
                        FileSize =request.ThumbnailImage.Length
                    }
                };
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product.Id;
        }

        //Delete Product
        public async Task<int> Delete(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new EShopException($"Can not found {productId}");

            var images = _context.ProductImages.Where(i => i.ProductId == productId);
            foreach(var image in  images)
            {
                await _storageService.DeleteFileAsync(image.ImagePath);
            }

            _context.Products.Remove(product);
            return await _context.SaveChangesAsync();
        }

        //GetAllProduct
        public async Task<List<ProductVM>> GetAll()
        {
            //1.Select join
            var query = from p in _context.Products
                        join pt in _context.ProductTranslations on p.Id equals pt.ProductId
                        join pic in _context.ProductInCategories on p.Id equals pic.ProductId
                        join c in _context.Categories on pic.CategoryId equals c.Id
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

        //GetAllPage
        public async Task<PagedResult<ProductVM>> GetAllPaging(ProductPagingVM request)
        {
            //1.Select join
            var query = from p in _context.Products
                        join pt in _context.ProductTranslations on p.Id equals pt.ProductId
                        join pic in _context.ProductInCategories on p.Id equals pic.ProductId
                        join c in _context.Categories on pic.CategoryId equals c.Id
                        select new { p, pt, pic };

            //2.filter
            if (!string.IsNullOrEmpty(request.Keyword))
                query = query.Where(x => x.pt.Name.Contains(request.Keyword));
            if(request.CategoryIds.Count>0)
            {
                query = query.Where(p => request.CategoryIds.Contains(p.pic.CategoryId));
            }

            //3.Paging
            int totalRow = await query.CountAsync();
            var data = await query.Skip((request.PageIndex - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .Select(x => new ProductVM()
                        {
                            Id = x.p.Id,
                            Price = x.p.Price,
                            OriginalPrice = x.p.OriginalPrice,
                            Stock = x.p.Stock,
                            ViewCount = x.p.ViewCount,
                            DateCreated = x.p.DateCreated,
                            Name = x.pt.Name,
                            Description= x.pt.Description,
                            Details = x.pt.Details,
                            SeoDescription = x.pt.SeoDescription,
                            SeoTitle = x.pt.SeoTitle,
                            SeoAlias = x.pt.SeoAlias,
                            LanguageId = x.pt.LanguageId
                        }).ToListAsync();

            //4.Select and projection
            var pagedResult = new PagedResult<ProductVM>()
            {
                TotalRecord = totalRow,
                Items = data
            };
            return pagedResult;
        }

        //GetById
        public async Task<ProductVM> GetById(int productId, string languageId)
        {
            var product = await _context.Products.FindAsync(productId);
            var productTranslation = await _context.ProductTranslations.FirstOrDefaultAsync(x => x.ProductId == productId
            && x.LanguageId == languageId);

            var productVm = new ProductVM()
            {
                Id = product.Id,
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                Stock = product.Stock,
                ViewCount = product.ViewCount,
                DateCreated = product.DateCreated,
                Name = productTranslation != null ? productTranslation.Name : null,
                Description = productTranslation != null ? productTranslation.Description : null,
                Details = productTranslation != null ? productTranslation.Details : null,
                SeoDescription = productTranslation != null ? productTranslation.SeoDescription : null,
                SeoTitle = productTranslation != null ? productTranslation.SeoDescription : null,
                SeoAlias = productTranslation != null ? productTranslation.SeoAlias : null,
                LanguageId = productTranslation.LanguageId
            };
            return productVm;
        }

        public async Task<ProductImageVM> GetImageById(int imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null) throw new EShopException("Can not find imageId {imageId}");

            var viewimage = new ProductImageVM()
            {
                        Id = image.Id,
                        ProductId = image.ProductId,
                        ImagePath = image.ImagePath,
                        Caption = image.Caption,
                        IsDefault = image.IsDefault,
                        DateCreated = image.DateCreated,
                        SortOrder = image.SortOrder,
                        FileSize = image.FileSize
            };
            return viewimage;
        }

        //GetListImage
        public async Task<List<ProductImageVM>> GetListImage(int productId)
        {
            return await _context.ProductImages.Where(x => x.ProductId == productId)
                    .Select(i => new ProductImageVM()
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ImagePath = i.ImagePath,
                        Caption = i.Caption,
                        IsDefault = i.IsDefault,
                        DateCreated = i.DateCreated,
                        SortOrder = i.SortOrder,
                        FileSize = i.FileSize
                    }).ToListAsync();
        }

        //RemoveImage
        public async Task<int> RemoveImages(int imageId)
        {
            var productImage = await _context.ProductImages.FindAsync(imageId);
            if (productImage == null) throw new EShopException($"Can not found {imageId}");
            _context.ProductImages.Remove(productImage);
            return await _context.SaveChangesAsync();
        }

        //UpdateProduct
        public async Task<int> Update(ProductUpdateVM request)
        {
            var product = await _context.Products.FindAsync(request.Id);
            var productTranslations = await _context.ProductTranslations.FirstOrDefaultAsync(x => x.ProductId == request.Id
            && x.LanguageId == request.LanguageId);
            if (product == null || productTranslations == null) throw new EShopException($"Cannot found a product id: {request.Id}");

            productTranslations.Name = request.Name;
            productTranslations.Description = request.Description;
            productTranslations.Details = request.Details;
            productTranslations.SeoDescription = request.SeoDescription;
            productTranslations.SeoTitle = request.SeoTitle;
            productTranslations.SeoAlias = request.SeoAlias;

            //SaveImage
            if(request.ThumbnailImage != null)
            {
                var thumbnailImage = await _context.ProductImages.FirstOrDefaultAsync(i => i.IsDefault == true && i.ProductId == request.Id);
                if(thumbnailImage != null)
                {
                    thumbnailImage.FileSize = request.ThumbnailImage.Length;
                    thumbnailImage.ImagePath = await this.SaveFile(request.ThumbnailImage);
                    _context.ProductImages.Update(thumbnailImage);
                }
            }

            return await _context.SaveChangesAsync();
        }

        //UpdateImage
        public async Task<int> UpdateImages( int imageId, ProductImageUpdate request)
        {
            var productImage = await _context.ProductImages.FindAsync(imageId);
            if (productImage == null) throw new EShopException("Cannot find an image with id {imageId}");

            if (request.ImageFile != null)
            {
                productImage.ImagePath = await this.SaveFile(request.ImageFile);
                productImage.FileSize = request.ImageFile.Length;
            }
            _context.ProductImages.Update(productImage);
            return await _context.SaveChangesAsync();
        }

        //Update Price 
        public async Task<bool> UpdatePrice(int productId, decimal newPrice)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new EShopException($"Cannot find a product with id: {productId}");
            product.Price = newPrice;
            return await _context.SaveChangesAsync() > 0;
        }

        //Update Stock
        public async Task<bool> UpdateStock(int productId, int addQuantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new EShopException($"Cannot find a product with id: {productId}");
            product.Stock += addQuantity;
            return await _context.SaveChangesAsync() > 0;
        }

        //SaveFileImage
        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }
    }
}
