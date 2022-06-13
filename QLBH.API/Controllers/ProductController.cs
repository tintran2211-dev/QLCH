using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QLBH.Application.Catalog.Products.IRepository;
using QLBH.ViewModels.Catalog.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLBH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IPublicService _publicProductService;
        private readonly IManageProductService _managerProductService;

        public ProductController(IPublicService publicProductService, IManageProductService managerProductService)
        {
            _publicProductService = publicProductService;
            _managerProductService = managerProductService;
        }

        //http://localhost:port/product/1
        [HttpGet("{languageId}")]
        public async Task<IActionResult> Get(string languageId)
        {
            var products = await _publicProductService.GetAll(languageId);
            return Ok(products);
        }

        //http://localhost:port/product/1
        [HttpGet("{productId}/{languageId}")]
        public async Task<IActionResult> GetById(int productId, string languageId)
        {
            var product = await _managerProductService.GetById(productId, languageId);
            if (product == null)
                return BadRequest("Cannot find product");
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateVM request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var productId = await _managerProductService.Create(request);
            if (productId == 0)
                return BadRequest();

            var product = await _managerProductService.GetById(productId, request.LanguageId);

            return CreatedAtAction(nameof(GetById), new { id = productId }, product);
        }


        [HttpPut]
        public async Task<IActionResult> Update([FromForm] ProductUpdateVM vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var Result = await _managerProductService.Update(vm);
            if (Result == 0)
                return BadRequest();
            return Ok();

        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> Delete(int productId)
        {
            var Result = await _managerProductService.Delete(productId);
            if (Result == 0)
                return BadRequest();
            return Ok();
        }

        [HttpPatch("{productId}/{newPrice}")]
        public async Task<IActionResult> UpdatePrice(int productId, decimal newPrice )
        {
            var Result = await _managerProductService.UpdatePrice(productId, newPrice);
            if (Result)
                return Ok();
            return BadRequest();
        }

        //Images
        [HttpPost("{productId}/image")]
        public async Task<IActionResult> AddImages(int productId, [FromForm] ProductImageCreate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var imageId = await _managerProductService.AddImages(productId, request);
            if (imageId == 0)
                return BadRequest();
            var image = await _managerProductService.GetImageById(imageId);
            return CreatedAtAction(nameof(GetImageById), new { id = imageId }, image);
        }

        [HttpPut("{productId}/image/{imageId}")]
        public async Task<IActionResult> UpdateImages(int imageId, [FromForm] ProductImageUpdate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var imgId = await _managerProductService.UpdateImages(imageId, request);
            if (imgId == 0)
                return BadRequest();
            return Ok();
        }

        [HttpDelete("{productId}/image/{imageId}")]
        public async Task<IActionResult> DeleteImages(int imageId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var imgId = await _managerProductService.RemoveImages(imageId);
            if (imgId == 0)
                return BadRequest();
            return Ok();
        }

        [HttpGet("{productId}/images/{imageId}")]
        public async Task<IActionResult> GetImageById(int productId,int imageId)
        {
            var image = await _managerProductService.GetImageById(imageId);
            if (image == null)
                return BadRequest("Cannot find image");
            return Ok(image);
        }
    }
}
