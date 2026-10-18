using System.Linq.Expressions;

namespace ECommerce.Repository.Interface
{
    /// <summary>
    /// 定义 数据库中的表的行为 增删改查
    /// </summary>
    public interface I_Repository<T> where T : class, new()
    {

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
        /// 单个查询
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        T? Find(params object?[]? keys);


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
        IQueryable<T> Query<type>(bool isAsc= true, Expression<Func<T, type>>? order = null, Expression<Func<T, bool>>? where = null);

        /// <summary>
        /// 查询+排序+分页
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IQueryable<T> Query<type>(out int total, int? skip = 0, int? take = 10,bool isAsc=true, Expression<Func<T, type>>? order = null, Expression<Func<T, bool>>? where = null);




    }
}
