using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Models
{
    /// <summary>
    /// Используем DbContext из ORM EntityFramework
    /// </summary>
    public class DataBaseContext:DbContext
    {
        static DataBaseContext()
        {
            Database.SetInitializer<DataBaseContext>(DataBaseInitializer.InitStrategy);
        }
        public DataBaseContext()
        {            
            this.Database.Connection.ConnectionString = "Data Source=(localdb)\v11.0;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";
        }
        /// <summary>
        /// Таблица в БД с именем Realty
        /// </summary>
        public DbSet<Realty> Realty { get; set; }

        /// <summary>
        /// Уточнение моделей в БД с помощью Fluent API
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Установим ключ Id таблицы Realty в БД
            modelBuilder.Entity<Realty>().HasKey(r => r.Id);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
