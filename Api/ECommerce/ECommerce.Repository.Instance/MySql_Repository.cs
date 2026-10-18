using ECommerce.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerce.Repository.Instance
{
    public class MySql_Repository<T> : I_Repository<T> where T : class, new()
    {
        private readonly DbContext _dbContext;

        public MySql_Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 你去做一个调用者 如何调用是一门手段
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public IQueryable<T> Query(Expression<Func<T, bool>>? where = null)
        {
            if (where == null)
                return _dbContext.Set<T>();
            else
                return _dbContext.Set<T>().Where(where);

        }

        /// <summary>
        /// 查询排序
        /// </summary>
        /// <typeparam name="type"></typeparam>
        /// <param name="isAsc"></param>
        /// <param name="order"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public IQueryable<T> Query<type>(bool isAsc = true, Expression<Func<T, type>>? order = null, Expression<Func<T, bool>>? where = null)
        {
            var iq = Query(where);

            if (isAsc)
                return iq.OrderBy(order!);
            else
                return iq.OrderByDescending(order!);

        }
        /// <summary>
        /// 查询 排序 分页
        /// </summary>
        /// <typeparam name="type"></typeparam>
        /// <param name="total"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="isAsc"></param>
        /// <param name="order"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public IQueryable<T> Query<type>(out int total, int? skip = 0, int? take = 10,
            bool isAsc = true,
            Expression<Func<T, type>>? order = null, Expression<Func<T, bool>>? where = null)
        {
            var iq = Query(isAsc, order, where);
            total = iq.Count();
            return iq.Skip(skip ?? 0).Take(take ?? 10);
        }

        /// <summary>
        /// 异步保存
        /// </summary>
        /// <returns></returns>
        public Task<int> SaveChangesASync()
        {
            return _dbContext.SaveChangesAsync();
        }

        void I_Repository<T>.Add(T t)
        {
            _dbContext.Set<T>().Add(t);
        }

        void I_Repository<T>.AddRange(IEnumerable<T> ie)
        {
            _dbContext.Set<T>().AddRange(ie);
        }

        void I_Repository<T>.Delete(T t)
        {
            _dbContext.Set<T>().Remove(t);
        }

        void I_Repository<T>.DeleteRange(IEnumerable<T> ie)
        {
            _dbContext.Set<T>().RemoveRange(ie);
        }

        /// <summary>
        /// 单个查询
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        T? I_Repository<T>.Find(params object?[]? keys)
        {
            return _dbContext.Set<T>().Find(keys);
        }

        /// <summary>
        /// 同步保存
        /// </summary>
        /// <returns></returns>
        int I_Repository<T>.SaveChanges()
        {
            return _dbContext.SaveChanges();
        }

        void I_Repository<T>.Update(T t)
        {
            _dbContext.Set<T>().Update(t);
        }

        void I_Repository<T>.UpdateRange(IEnumerable<T> ie)
        {
            _dbContext.Set<T>().UpdateRange(ie);
        }
    }
}
