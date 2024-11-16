﻿using FourLeafCloverShoe.Data;
using FourLeafCloverShoe.IServices;
using FourLeafCloverShoe.Share.Models;
using FourLeafCloverShoe.Share.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FourLeafCloverShoe.Services
{
    public class ProductDetailService : IProductDetailService
    {
        private readonly MyDbContext _myDbContext;

       
        public ProductDetailService(MyDbContext myDbContext)
        {
            _myDbContext = myDbContext;
        }
        public async Task<bool> Add(ProductDetail obj)
        {
            try
            {
                obj.CreateAt = DateTime.Now;
                await _myDbContext.ProductDetails.AddAsync(obj);
                await _myDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<bool> AddMany(List<ProductDetail> lstobj)
        {
            try
            {
                await _myDbContext.ProductDetails.AddRangeAsync(lstobj);
                await _myDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<bool> Delete(Guid Id)
        {
            try
            {
                var obj = await GetById(Id);
                if (obj != null)
                {
                    _myDbContext.ProductDetails.Remove(obj);
                    await _myDbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<ProductDetail> GetById(Guid Id)
        {
            try
            {
                var lstobj = await _myDbContext.ProductDetails
                   .Include(c => c.Products)
                        .ThenInclude(c => c.ProductImages)
                   .Include(c => c.Size)
                   .Include(c => c.Colors)

                   .ToListAsync();
                var obj = lstobj.FirstOrDefault(c => c.Id == Id);
                if (obj != null)
                {

                    return obj;
                }
                return new ProductDetail();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new ProductDetail();
            }
        }

        public async Task<List<ProductDetail>> GetByProductId(Guid ProductId)
        {
            try
            {
                var lstobj = await _myDbContext.ProductDetails
                   .Include(c => c.Products)
                   .ThenInclude(c => c.ProductImages)
                   .Include(c => c.Size)
                   .Include(c => c.Colors)
                   .Include(c => c.Material)
                   .ToListAsync();
                var obj = lstobj.Where(c => c.ProductId == ProductId).ToList();
                if (obj != null)
                {

                    return obj;
                }
                return new List<ProductDetail>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<ProductDetail>();
            }
        }

        public async Task<List<ProductDeailViewModel>> GetProductDetails()
        {
            var lst = await (from a in _myDbContext.ProductDetails
                             join b in _myDbContext.Products on a.ProductId equals b.Id
                             join c in _myDbContext.ProductImages on a.ProductId equals c.ProductId
                             join d in _myDbContext.Sizes on a.SizeId equals d.Id
                             join e in _myDbContext.Colors on a.ColorId equals e.Id
                             select new ProductDeailViewModel()
                             {
                                 Id = a.Id,
                                 ProductName = b.ProductName,
                                 Quantity = a.Quantity,
                                 Price = a.PriceSale,
                                 SizeName = d.Name,
                                 ColorName = e.ColorName,
                                 ImageUrl = c.ImageUrl,
                                 StatusPro = (int)a.Status,
                             }).ToListAsync();
            return lst;

        }

        public async Task<List<ProductDetail>> Gets()
        {
            try
            {
                var obj = await _myDbContext.ProductDetails
                    .Include(c => c.Products)
                    .ThenInclude(c => c.ProductImages)
                    .Include(c => c.Size)
                    .Include(c=>c.Colors)
                    .ToListAsync();
                if (obj != null)
                {

                    return obj;
                }
                return new List<ProductDetail>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<ProductDetail>();
            }
        }

        public async Task<bool> Update(ProductDetail obj)
        {
            try
            {
                var objFromDb = await GetById(obj.Id);
                if (obj != null)
                {
                    objFromDb.SizeId = obj.SizeId;
                    objFromDb.ColorId = obj.ColorId;
                    objFromDb.SKU = obj.SKU;
                    objFromDb.Quantity = obj.Quantity;
                    objFromDb.PriceSale = obj.PriceSale;
                    objFromDb.UpdateAt = DateTime.Now;
                    objFromDb.Status = obj.Status;
                    _myDbContext.ProductDetails.Update(objFromDb);
                    await _myDbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<bool> UpdateMany(List<ProductDetail> lstobj)
        {
            try
            {
                 _myDbContext.ProductDetails.UpdateRange(lstobj);
                await _myDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        public async Task<bool> UpdateQuantityById(Guid? productDetailId, int? quantity)
        {
            try
            {
                var productDetail = _myDbContext.ProductDetails.FirstOrDefault(c => c.Id == productDetailId);
                productDetail.Quantity -= quantity;
                _myDbContext.ProductDetails.Update(productDetail);
                await _myDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public async Task<bool> UpdateQuantityOrderFail(Guid productDetailId, int? quantity)
        {
            try
            {
                var productDetail = _myDbContext.ProductDetails.FirstOrDefault(c => c.Id == productDetailId);
                productDetail.Quantity += quantity;
                _myDbContext.ProductDetails.Update(productDetail);
                await _myDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}