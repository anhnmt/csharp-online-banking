using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.BLL.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, new()
    {
        private readonly ApplicationDbContext cnn;

        private readonly DbSet<T> tbl;
        public Repository()
        {
            cnn = new ApplicationDbContext();
            tbl = cnn.Set<T>();
        }
        public bool Add(T e)
        {
            try
            {
                tbl.Add(e);
                cnn.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CheckDuplicate(Expression<Func<T, bool>> predicate)
        {
            return tbl.AsNoTracking().Any(predicate);
        }

        public bool Delete(object id)
        {
            try
            {
                var entity = Get(id);
                tbl.Remove(entity);
                cnn.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Delete(T e)
        {
            try
            {
                tbl.Remove(e);
                cnn.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Edit(T e)
        {
            try
            {
                cnn.Entry(e).State = EntityState.Modified;
                cnn.SaveChanges();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IEnumerable<T> Get()
        {
            return tbl.AsEnumerable();
        }

        public IEnumerable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return tbl.Where(predicate).AsEnumerable();
        }

        public T Get(object id)
        {
            return tbl.Find(id);
        }

        public bool Remove(object id)
        {
            try
            {
                var entity = Get(id);
                if (entity == null) return false;
                tbl.Remove(entity);
                cnn.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Update(T entity)
        {
            try
            {
                cnn.Entry(entity).State = EntityState.Modified;
                cnn.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
