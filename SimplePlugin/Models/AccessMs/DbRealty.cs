using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimplePlugin.Models.Entity;

namespace SimplePlugin.Models.AccessMs
{
    /// <summary>
    /// Реализация фабрики для работы с таблицой недвижимости в БД  MS Access
    /// </summary>
    public class DbRealty:IRealty
    {
        /// <summary>
        /// Поиск недвижимости по широте и долготе
        /// </summary>
        /// <param name="latitude">широта</param>
        /// <param name="longitude">долгота</param>
        /// <returns></returns>
        public Realty Find(double latitude, double longitude)
        {
            return 
            MicroORM.DbORM.GetEntities<Realty>(DbConnection.Instance, 
                string.Format(@"select * from Realty where Latitude={0} and Longitude={1}",latitude,longitude)
                ).FirstOrDefault();
        }

        /// <summary>
        /// Поиск недвижимости по ключу
        /// </summary>
        /// <param name="id">Id ключ</param>
        /// <returns></returns>
        public Realty Find(int id)
        {
            return
            MicroORM.DbORM.GetEntities<Realty>(DbConnection.Instance,
                string.Format(@"select * from Realty where Id={0}",id)
                ).FirstOrDefault();
        }


      
        /// <summary>
        /// Поиск недвижимости в области заданной географическими координатами
        /// </summary>
        /// <param name="latitudeMin">Минимальная широта(Y-локальная)</param>
        /// <param name="longitudeMin">Минимальная долгота (X-локальная)</param>
        /// <param name="latitudeMax">Максимальная широта (Y-локальная)</param>
        /// <param name="longitudeMax">Максимальная долгота(X-локальная)</param>
        /// <returns></returns>
        public IEnumerable<Realty> Find(double latitudeMin, double longitudeMin, double latitudeMax, double longitudeMax)
        {
            return
           MicroORM.DbORM.GetEntities<Realty>(DbConnection.Instance,
               string.Format(
               new System.Globalization.NumberFormatInfo() { CurrencyDecimalSeparator = "." }
               ,@"select * from Realty where Longitude>={0} and Longitude<={1} and Latitude>={2} and Latitude<={3}", longitudeMin,longitudeMax,latitudeMin,latitudeMax)
               );
        }

        /// <summary>
        /// Добавление недвижимости в БД
        /// </summary>
        /// <param name="entity">Новый объект недвижимости</param>
        /// <returns>Возвращает добавленный объект, но с уже заполненным Id</returns>
        public Realty Add(Realty entity)
        {            
            return
            MicroORM.DbORM.PutEntities<Realty>(new Realty[] { entity}, DbConnection.Instance).FirstOrDefault();
        }


        /// <summary>
        /// Изменение объекта недвижимости
        /// </summary>
        /// <param name="entity">Объект невижимости</param>
        public void Edit(Realty entity)
        {            
            MicroORM.DbORM.PutEntities<Realty>(new Realty[] { entity }, DbConnection.Instance).FirstOrDefault();
        }

        /// <summary>
        /// Уаление объекта недвижимости
        /// </summary>
        /// <param name="entity">Объект недвижимости</param>
        public void Remove(Realty entity)
        {            
            MicroORM.DbORM.PutEntities<Realty>(new Realty[] { entity }, DbConnection.Instance).FirstOrDefault();
        }

        #region IEnumerable
        public IEnumerator<Realty> GetEnumerator()
        {
           return MicroORM.DbORM.GetEntities<Realty>(DbConnection.Instance).AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return MicroORM.DbORM.GetEntities<Realty>(DbConnection.Instance).AsEnumerable().GetEnumerator();
        }
        #endregion

    }
}
