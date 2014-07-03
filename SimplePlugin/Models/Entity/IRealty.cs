using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Models.Entity
{
    /// <summary>
    /// Фабрика для получения объектов модели недвижимости
    /// </summary>
    public interface IRealty:IEntityFactory<Realty>
    {      
       /// <summary>
       /// Поиск недвижимости по географическим координатам
       /// </summary>
       /// <param name="latitude">Широта</param>
       /// <param name="longitude">Долгота</param>
       /// <returns>Объект недвижимости, если найден</returns>
       Realty Find(double latitude, double longitude);

        /// <summary>
        /// Поиск недвижимости в области заданной географическими координатами
        /// </summary>
        /// <param name="latitudeMin">Минимальная широта(Y-локальная)</param>
        /// <param name="longitudeMin">Минимальная долгота (X-локальная)</param>
        /// <param name="latitudeMax">Максимальная широта (Y-локальная)</param>
        /// <param name="longitudeMax">Максимальная долгота(X-локальная)</param>
        /// <returns></returns>
       IEnumerable<Realty> Find(double latitudeMin, double longitudeMin,
                                double latitudeMax, double longitudeMax);
    }
}
