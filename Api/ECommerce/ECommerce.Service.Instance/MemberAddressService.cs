using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Member;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class MemberAddressService : Base_Service, IMemberAddressService
    {
        private readonly MemberAddress_EFCore _efCore;

        public MemberAddressService(MemberAddress_EFCore efCore)
        {
            _efCore = efCore;
        }

        public async Task<Api_Response> GetListAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Fail("用户未登录");
            }

            var list = await _efCore
                .Query<DateTime?>(false, d => d.CreateTime, d => d.UserId == userId)
                .ToListAsync();

            // 默认地址排最前
            var sorted = list
                .OrderByDescending(d => d.IsDefault ?? 0)
                .ThenByDescending(d => d.CreateTime)
                .Select(To_Dto)
                .ToList();

            return Ok("ok", sorted);
        }

        public async Task<Api_Response> GetAsync(string userId, string id)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(id))
            {
                return Fail("参数不正确");
            }

            var entity = await _efCore
                .Query(d => d.UserId == userId && d.Id == id)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return Fail("地址不存在");
            }

            return Ok("ok", To_Dto(entity));
        }

        public async Task<Api_Response> SaveAsync(string userId, MemberAddress_Request_Dto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Fail("用户未登录");
            }
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Mobile)
                || string.IsNullOrWhiteSpace(dto.DetailAddr))
            {
                return Fail("收件人、手机号、详细地址均不能为空");
            }

            var now = DateTime.Now;
            MemberAddress entity;
            bool isNew = string.IsNullOrWhiteSpace(dto.Id);

            if (isNew)
            {
                entity = new MemberAddress
                {
                    Id = Config.NewSequentialGuid(),
                    UserId = userId,
                    CreateTime = now,
                    UpdateTime = now
                };
            }
            else
            {
                entity = await _efCore
                    .Query(d => d.UserId == userId && d.Id == dto.Id)
                    .FirstOrDefaultAsync();
                if (entity == null)
                {
                    return Fail("地址不存在");
                }
                entity.UpdateTime = now;
            }

            entity.Name = dto.Name;
            entity.Mobile = dto.Mobile;
            entity.Province = dto.Province;
            entity.City = dto.City;
            entity.District = dto.District;
            entity.DetailAddr = dto.DetailAddr;

            // 设为默认：先把该用户其它地址取消默认，再置当前为默认
            if (dto.IsDefault == 1)
            {
                var others = await _efCore
                    .Query(d => d.UserId == userId && d.Id != entity.Id && (d.IsDefault ?? 0) == 1)
                    .ToListAsync();
                foreach (var o in others)
                {
                    o.IsDefault = 0;
                    _efCore.Update(o);
                }
                entity.IsDefault = 1;
            }
            else if (isNew)
            {
                // 新增时未指定默认：若该用户没有任何默认地址，则自动设为默认
                var hasDefault = await _efCore
                    .Query(d => d.UserId == userId && (d.IsDefault ?? 0) == 1)
                    .AnyAsync();
                entity.IsDefault = hasDefault ? 0 : 1;
            }

            if (isNew)
            {
                _efCore.Add(entity);
            }
            else
            {
                _efCore.Update(entity);
            }

            var result = _efCore.SaveChanges();
            return Get_Result(result, isNew ? "添加成功" : "保存成功", To_Dto(entity));
        }

        public async Task<Api_Response> RemoveAsync(string userId, string id)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(id))
            {
                return Fail("参数不正确");
            }

            var entity = await _efCore
                .Query(d => d.UserId == userId && d.Id == id)
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                return Fail("地址不存在");
            }

            _efCore.Delete(entity);
            var result = _efCore.SaveChanges();
            return Get_Result(result, "删除成功");
        }

        public async Task<Api_Response> SetDefaultAsync(string userId, string id)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(id))
            {
                return Fail("参数不正确");
            }

            var entity = await _efCore
                .Query(d => d.UserId == userId && d.Id == id)
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                return Fail("地址不存在");
            }

            var others = await _efCore
                .Query(d => d.UserId == userId && d.Id != id && (d.IsDefault ?? 0) == 1)
                .ToListAsync();
            foreach (var o in others)
            {
                o.IsDefault = 0;
                _efCore.Update(o);
            }

            entity.IsDefault = 1;
            _efCore.Update(entity);
            var result = _efCore.SaveChanges();
            return Get_Result(result, "设置成功");
        }

        private MemberAddress_Response_Dto To_Dto(MemberAddress e)
        {
            var full = $"{e.Province ?? string.Empty}{e.City ?? string.Empty}{e.District ?? string.Empty}{e.DetailAddr ?? string.Empty}";
            return new MemberAddress_Response_Dto
            {
                Id = e.Id,
                Name = e.Name,
                Mobile = e.Mobile,
                Province = e.Province,
                City = e.City,
                District = e.District,
                DetailAddr = e.DetailAddr,
                FullAddress = full,
                IsDefault = e.IsDefault
            };
        }
    }
}
