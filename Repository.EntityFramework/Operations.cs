using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.EntityFramework
{
    internal class EFInsert<TValue> : Insert<TValue> where TValue : class
    {
        //===============================================================
        public EFInsert(IEnumerable<Object> keys, TValue value, DbSet<TValue> set)
            : base(keys, value)
        {
            DbSet = set;
        }
        //===============================================================
        private DbSet<TValue> DbSet { get; set; }
        //===============================================================
        public override void Apply()
        {
            DbSet.Add(Value);
        }
        //===============================================================
    }

    internal class EFRemove<TValue> : Remove where TValue : class
    {
        //===============================================================
        public EFRemove(IEnumerable<Object> keys, DbSet<TValue> dbSet)
            : base(keys)
        {
            DbSet = dbSet;
        }
        //===============================================================
        private DbSet<TValue> DbSet { get; set; }
        //===============================================================
        public override void Apply()
        {
            var obj = DbSet.Find(Keys.ToArray());
            DbSet.Remove(obj);
        }
        //===============================================================
    }

    internal class EFRemoveAll<TValue, TContext> : IOperation
        where TContext : DbContext
        where TValue : class
    {
        //===============================================================
        public EFRemoveAll(TContext context)
        {
            Context = context;
        }
        //===============================================================
        private TContext Context { get; set; }
        //===============================================================
        public void Apply()
        {
            var tableName = Context.GetTableName<TValue>();
            tableName = tableName.Replace("[dbo].", "").Replace("[", "").Replace("]", "");
            var query = "DELETE FROM " + tableName;

            Context.Database.ExecuteSqlCommand(query);
        }
        //===============================================================
    }

    internal class EFModify<TValue> : Modify<TValue> where TValue : class
    {
        //===============================================================

        public EFModify(IEnumerable<object> keys, TValue value, Action<TValue> modifier)
            : base(keys, value, modifier)
        { }
        //===============================================================
        public override void Apply()
        {
            Modifier(Value);
        }
        //===============================================================
    }
}
