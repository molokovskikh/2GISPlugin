using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Models
{
    public class DataBaseInitializer:IDatabaseInitializer<DataBaseContext>
    {
        public void InitializeDatabase(DataBaseContext context)
        {            
            if (context.Database.CreateIfNotExists())
            {
                System.Windows.Forms.MessageBox.Show(context.Database.Connection.ConnectionString, "База создана");
            }
            else
                System.Windows.Forms.MessageBox.Show(context.Database.Connection.ConnectionString, "База уже есть");
        }
         
            static readonly DataBaseInitializer _oneToAllStrategy = new DataBaseInitializer();
            public static DataBaseInitializer InitStrategy
            {
                get
                {
                    return _oneToAllStrategy;
                }
            }         
    }
}
