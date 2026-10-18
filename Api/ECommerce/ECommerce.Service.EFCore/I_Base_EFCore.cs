using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Service.EFCore
{
    /// <summary>
    /// T 代表着 你等会要操作的那一张表的类型
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public interface I_Base_EFCore<T> where T : class,new()
    {
        /// <summary>
        /// 单个查询
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        T? Find(params object?[]? keys);



        /// <summary>
        /// 单个新增
        /// </summary>
        /// <param name="t"></param>
        void Add(T t);

        /// <summary>
        /// 多个新增
        /// </summary>
        /// <param name="t"></param>
        void AddRange(IEnumerable<T> ie);

        /// <summary>
        /// 单个修改
        /// </summary>
        /// <param name="t"></param>
        void Update(T t);

        /// <summary>
        /// 多个修个
        /// </summary>
        /// <param name="t"></param>
        void UpdateRange(IEnumerable<T> ie);

        /// <summary>
        /// 单个删除
        /// </summary>
        /// <param name="t"></param>
        void Delete(T t);

        /// <summary>
        /// 多个删除
        /// </summary>
        /// <param name="t"></param>
        void DeleteRange(IEnumerable<T> ie);


        /// <summary>
        /// 保存操作
        /// </summary>
        /// <returns></returns>
        int SaveChanges();

        /// <summary>
        /// 异步保存操作
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesASync();

   


        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IQueryable<T> Query(Expression<Func<T, bool>>? where = null);


        /// <summary>
        /// 查询并排序
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IQueryable<T> Query<type>(bool isAsc = true, Expression<Func<T, type>>? order = null, Expression<Func<T, bool>>? where = null);

        /// <summary>
        /// 查询+排序+分页
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IQueryable<T> Query<type>(out int total, int? page = 1, int? limit = 10, bool isAsc = true, Expression<Func<T, type>>? order = null, Expression<Func<T, bool>>? where = null);



    }
}
