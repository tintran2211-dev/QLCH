using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLBH.Data.Entities;
using QLBH.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBH.Data.Seed
{
    public static class Seeding
    {
        public static void SeedData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppConfig>().HasData(
                    new AppConfig() { Key = "HomeTitle",Value = "This is home pasge of the EShop"},
                    new AppConfig() { Key = "HomeKeyword", Value = "This is keyword of the EShop" },
                    new AppConfig() { Key = "HomeDescription", Value = "This is description of the EShop" }
                );

            modelBuilder.Entity<Language>().HasData(
                    new Language() { Id = "vi-VN", Name = "Tiếng Việt",IsDefault= false},
                    new Language() { Id = "en-US", Name = "English", IsDefault = false }
                );

            modelBuilder.Entity<Category>().HasData(
                    new Category() { Id = 1,SortOrder = 1, IsShowOnHome = true, ParentId = null,Status = Status.Active},
                    new Category() { Id = 2, SortOrder = 2, IsShowOnHome = true, ParentId = null, Status = Status.Active }
                );

            modelBuilder.Entity<CategoryTranslation>().HasData(
                    new CategoryTranslation() { Id =1, CategoryId = 1, Name = "Áo nam",  LanguageId = "vi-VN",SeoAlias = "ao-nam",SeoDescription= "Sản phẩm áo thời trang nam",SeoTitle = "Sản phẩm áo thời trang nam" },
                    new CategoryTranslation() { Id = 2, CategoryId = 1, Name = "Men Shirt", LanguageId = "en-US", SeoAlias = "men-shirt", SeoDescription = "The shirt products for men", SeoTitle = "The shirt products for men" },
                    new CategoryTranslation() { Id = 3, CategoryId = 2, Name = "Áo nữ", LanguageId = "vi-VN", SeoAlias = "ao-nu", SeoDescription = "Sản phẩm áo thời trang nữ", SeoTitle = "Sản phẩm áo thời trang nữ" },
                    new CategoryTranslation() { Id = 4, CategoryId = 2, Name = "Women Shirt", LanguageId = "en-US", SeoAlias = "women-shirt", SeoDescription = "The shirt products for women", SeoTitle = "The shirt products for women" }
                );

            modelBuilder.Entity<Product>().HasData(
                    new Product() { Id= 1,Price = 300000,OriginalPrice = 100000,DateCreated = DateTime.UtcNow,Stock = 0,ViewCount = 0}
                );

            modelBuilder.Entity<ProductTranslation>().HasData(
                    new ProductTranslation() 
                    { Id = 1,
                      ProductId = 1,
                      Name = "Áo Sơ mi nam trắng",
                      Description = "Áo sơ mi trắng Việt Tiến",
                      LanguageId = "vi-VN",
                      Details = "Áo sơ mi trắng Việt Tiến",
                      SeoAlias = "ao-so-mi-nam-trang-viet-tien",
                      SeoDescription = "Áo sơ mi nam trắng Việt Tiến",
                      SeoTitle= "Áo sơ mi nam trắng Việt Tiến"
                    },
                    new ProductTranslation()
                    {
                        Id = 2,
                        ProductId = 1,
                        Name = "Viet Tien Men T-Shirt",
                        LanguageId = "en-US",
                        SeoAlias = "viet-tien-men-t-shirt",
                        SeoDescription = "Viet Tien Men T-Shirt",
                        SeoTitle = "Viet Tien Men T-Shirt",
                        Details = "Viet Tien Men T-Shirt",
                        Description = "Viet Tien Men T-Shirt"
                    }
                );

            modelBuilder.Entity<ProductInCategory>().HasData(
                    new ProductInCategory() { ProductId = 1 , CategoryId = 1}
                );

            // any guid
            var roleId = new Guid("8D04DCE2-969A-435D-BBA4-DF3F325983DC");
            var adminId = new Guid("69BD714F-9576-45BA-B5B7-F00649BE00DE");
            modelBuilder.Entity<AppRole>().HasData(new AppRole
            {
                Id = roleId,
                Name = "admin",
                NormalizedName = "admin",
                Description = "Administrator role"
            });

            var hasher = new PasswordHasher<AppUser>();
            modelBuilder.Entity<AppUser>().HasData(new AppUser
            {
                Id = adminId,
                UserName = "admin",
                NormalizedUserName = "admin",
                Email = "tedu.international@gmail.com",
                NormalizedEmail = "tedu.international@gmail.com",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Abcd1234$"),
                SecurityStamp = string.Empty,
                FirstName = "Toan",
                LastName = "Bach",
                Dob = new DateTime(2020, 01, 31)
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(new IdentityUserRole<Guid>
            {
                RoleId = roleId,
                UserId = adminId
            });
        }
    }
}
