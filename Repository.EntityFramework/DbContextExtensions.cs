using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Samples.EntityDataReader;
using NUnit.Framework;

namespace Repository.EntityFramework
{
    // Taken from http://www.codeproject.com/Articles/350135/Entity-Framework-Get-mapped-table-name-from-an-ent
    public static class ContextExtensions
    {
        //===============================================================
        public static string GetTableName<T>(this DbContext context) where T : class
        {
            var adapter = (IObjectContextAdapter)context;
            var objectContext = adapter.ObjectContext;
            return objectContext.GetTableName<T>();
        }

        //===============================================================
        public static string GetTableName<T>(this ObjectContext context) where T : class
        {
            string sql = context.CreateObjectSet<T>().ToTraceString();
            Regex regex = new Regex("FROM (?<table>.*) AS");
            Match match = regex.Match(sql);

            string table = match.Groups["table"].Value;
            return table;
        }
        //===============================================================
        public static void BulkAdd<T>(this DbContext context, IEnumerable<T> items) where T : class
        {
            var tableName = GetTableName<T>(context);
            var dataTable = items.ToDataTable();        
            var connection = (SqlConnection)context.Database.Connection;

            var isClosed = connection.State != ConnectionState.Open;
            if (isClosed)
                connection.Open();

            var bulkCopy = new SqlBulkCopy(connection);
            bulkCopy.DestinationTableName = tableName;
            bulkCopy.WriteToServer(dataTable);

            if (isClosed)
                connection.Close();
        }
        //===============================================================
        public static Func<TValue, Object[]> GetKeySelector<TValue>(this DbContext context) where TValue : class
        {
            var keyNames = context.GetKeyNames<TValue>();
            return x => keyNames.Select(name => typeof(TValue).GetProperty(name).GetValue(x)).ToArray();
        }
        //===============================================================
        public static IEnumerable<String> GetKeyNames<TValue>(this DbContext context) where TValue : class
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            var set = objectContext.CreateObjectSet<TValue>();
            var keyNames = set.EntitySet.ElementType.KeyMembers.Select(x => x.Name).ToList();

            return keyNames;
        }
        //===============================================================
    }
}
