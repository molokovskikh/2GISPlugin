using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Models.Entity
{
    /// <summary>
    /// Сущность &lt;&gt; БД (шаблон)
    /// </summary>
    /// <typeparam name="T">Модель сущности</typeparam>
    public interface IEntityFactory<T>:IEnumerable<T>
    {

        /// <summary>
        /// Поиск сущности по ключу
        /// </summary>
        /// <param name="id">Ключ для поиска</param>
        /// <returns></returns>
        T Find(int id);

        /// <summary>
        /// Добавить сущность
        /// </summary>
        /// <param name="entity"></param>
        T Add(T entity);
        
        /// <summary>
        /// Изменить сущность
        /// </summary>
        /// <param name="entity"></param>
        void Edit(T entity);

        /// <summary>
        /// Удалить сущность
        /// </summary>
        /// <param name="entity"></param>
        void Remove(T entity);        

    }
}
