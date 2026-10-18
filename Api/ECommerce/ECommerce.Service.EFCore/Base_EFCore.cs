using ECommerce.Repository.Interface;
using System.Linq.Expressions;
using System.Transactions;

namespace ECommerce.Service.EFCore
{
    public class Base_EFCore<T> : I_Base_EFCore<T> where T : class, new()
    {
        private readonly I_Repository<T> _repository;

        public Base_EFCore(I_Repository<T> repository)
        {
            _repository = repository;
        }

        public void Add(T t)
        {
            _repository.Add(t);
        }

        public void AddRange(IEnumerable<T> ie)
        {
            _repository.AddRange(ie);
        }

        public void Delete(T t)
        {
            _repository.Delete(t);
        }

        public void DeleteRange(IEnumerable<T> ie)
        {
            _repository.DeleteRange(ie);
        }

        /// <summary>
        /// 查询单个
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T? Find(params object?[]? keys)
        {
            return _repository.Find(keys);
        }

        public IQueryable<T> Query(Expression<Func<T, bool>>? where = null)
        {
            return _repository.Query(where);
        }

        public IQueryable<T> Query<type>(bool isAsc = true, Expression<Func<T, type>>? order = null, Expression<Func<T, bool>>? where = null)
        {
            return _repository.Query<type>(isAsc, order, where);
        }

        public IQueryable<T> Query<type>(out int total, int? page = 1, int? limit = 10, bool isAsc = true, Expression<Func<T, type>>? order = null, Expression<Func<T, bool>>? where = null)
        {
            int p = page ?? 1;
            int l = limit ?? 10;
            return _repository.Query(out total, (p - 1) * l, l, isAsc, order, where);
        }

        public int SaveChanges()
        {
            return _repository.SaveChanges();

        }

        public Task<int> SaveChangesASync()
        {
            return _repository.SaveChangesASync();
        }

        public void Update(T t)
        {
         _repository.Update(t);
        }

        public void UpdateRange(IEnumerable<T> ie)
        {
           _repository.UpdateRange(ie);
        }


        /// <summary>
        /// 分布式事务处理 同时处理2张表
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="bs">上下文对象2</param>
        /// <returns></returns>
        public bool Transactions<T1>(I_Base_EFCore<T1> bs)
           where T1 : class, new()
        {
            try
            {
                //分布式事务 
                using (TransactionScope transactionScope = new TransactionScope())
                {
                    this.SaveChanges();
                    bs.SaveChanges();
                    transactionScope.Complete();//提交事务
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}
