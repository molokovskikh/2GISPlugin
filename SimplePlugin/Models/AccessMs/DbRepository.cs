using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Models.AccessMs
{
    /// <summary>
    /// Фабрика объектов БД MS Access
    /// </summary>
    public static class DbRepository
    {
        static readonly DbRealty _realty = new DbRealty();

        /// <summary>
        /// Работа с объектом недвижимости
        /// </summary>
        public static DbRealty Realty
        {
            get
            {
                return _realty;
            }
        }
    }
}
