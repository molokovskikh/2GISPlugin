using GrymCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Models
{
    /// <summary>
    /// Тип недвижимости
    /// </summary>
    public enum TypeObject
    {
        /// <summary>
        /// Продажа
        /// </summary>
        Sale,
        /// <summary>
        /// Аренда
        /// </summary>
        Lease           
    }
    /// <summary>
    /// Тестовая модель.  В нашем примере будет Недвижимость
    /// </summary>
    [MicroORM.DataAnnotations.TableName("Realty")] //Уточним имя таблицы в БД
    public class Realty
    {
        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        [MicroORM.DataAnnotations.Key] //Укажем ключ в БД
        public int Id { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        [MicroORM.DataAnnotations.DisplayColumn("NameR")] //Уточним имя колонки в БД
        public string Name { get; set; }

        /// <summary>
        /// Подробное описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Тип недвижимости
        /// </summary>
        public TypeObject Type { get; set; }

        /// <summary>
        /// Месторасположение. Широта
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Месторасположение. Долгота
        /// </summary>
        public double Longitude { get; set; }        

    }
}
