using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Models.MicroORM.DataAnnotations
{
    /// <summary>
    /// Аттрибут уточнения имени таблицы в БД связанной с описываемой сущностью
    /// </summary>
    public class TableNameAttribute : Attribute
    {
        public TableNameAttribute(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }
    }

    /// <summary>
    /// Аттрибут уточнения колонок в БД связанных с описываемой сущностью
    /// </summary>
    public class DisplayColumnAttribute:Attribute
    {
        public DisplayColumnAttribute(string displayColumn)
        {
            DisplayColumn = displayColumn;
        }

        public string DisplayColumn { get; set; }
    }

    /// <summary>
    /// Аттрибут уточнения ключа сущности, (устанавливается в единственном экземпляре на модель сущности)
    /// </summary>
    public class KeyAttribute : Attribute
    {       
    }
}
