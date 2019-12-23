using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EasyUnitOfWork.Extensions;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace EasyUnitOfWork.Uow
{
    /// <summary>
    /// EFCore事务策略默认实现
    /// </summary>
    public class DbContextEfCoreTransactionStrategy : IEfCoreTransactionStrategy
    {
        protected UnitOfWorkOptions Options { get; private set; }

        protected IDictionary<string, ActiveTransactionInfo> ActiveTransactions { get; }

        public DbContextEfCoreTransactionStrategy()
        {
            ActiveTransactions = new Dictionary<string, ActiveTransactionInfo>();
        }

        /// <inheritdoc/>
        public virtual void InitOptions(UnitOfWorkOptions options)
        {
            Options = options;
        }

        /// <inheritdoc/>
        public virtual DbContext CreateDbContext<TDbContext>(string connectionString, IDbContextResolver dbContextResolver)
            where TDbContext : DbContext
        {
            DbContext dbContext;

            var activeTransaction = ActiveTransactions.GetOrDefault(connectionString);
            if (activeTransaction == null)
            {
                dbContext = dbContextResolver.Resolve<TDbContext>(connectionString, null);

                var dbtransaction = dbContext.Database.BeginTransaction((Options.IsolationLevel ?? IsolationLevel.ReadUncommitted).ToSystemDataIsolationLevel());
                activeTransaction = new ActiveTransactionInfo(dbtransaction, dbContext);
                ActiveTransactions[connectionString] = activeTransaction;
            }
            else
            {
                dbContext = dbContextResolver.Resolve<TDbContext>(
                    connectionString,
                    activeTransaction.DbContextTransaction.GetDbTransaction().Connection
                );

                if (dbContext.HasRelationalTransactionManager())
                {
                    dbContext.Database.UseTransaction(activeTransaction.DbContextTransaction.GetDbTransaction());
                }
                else
                {
                    dbContext.Database.BeginTransaction();
                }

                activeTransaction.AttendedDbContexts.Add(dbContext);
            }

            return dbContext;
        }

        /// <inheritdoc/>
        public virtual void Commit()
        {
            foreach (var activeTransaction in ActiveTransactions.Values)
            {
                activeTransaction.DbContextTransaction.Commit();

                foreach (var dbContext in activeTransaction.AttendedDbContexts)
                {
                    if (dbContext.HasRelationalTransactionManager())
                    {
                        continue; // Relational databases use the shared transaction
                    }

                    dbContext.Database.CommitTransaction();
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool state)
        {
            foreach (var activeTransaction in ActiveTransactions.Values)
            {
                activeTransaction.DbContextTransaction.Dispose();

                foreach (var attendedDbContext in activeTransaction.AttendedDbContexts)
                {
                    attendedDbContext?.Dispose();
                }

                activeTransaction.StarterDbContext?.Dispose();
            }

            ActiveTransactions.Clear();
        }

    }
}
