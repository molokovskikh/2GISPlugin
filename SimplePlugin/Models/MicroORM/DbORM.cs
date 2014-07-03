using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Models.MicroORM
{
    /// <summary>
    /// Статический класс обслуживания получения данных и запроса 
    /// и преобразования их в список объектов модели
    /// </summary>
    public static class DbORM
    {
        #region Внутрений функционал

        /// <summary>
        /// Сформируем имя таблицы в БД
        /// </summary>
        /// <param name="t_entity"></param>
        /// <returns></returns>
        static string _genTableName(Type t_entity)
        {            
            if (t_entity == null) return null;
            var result = t_entity.GetCustomAttributes(typeof(DataAnnotations.TableNameAttribute), true).Select(s => ((DataAnnotations.TableNameAttribute)s).TableName);
            return result.Count() > 0 ? result.First() : t_entity.Name;
        }

        /// <summary>
        /// Получить имя ключа таблицы в БД из типа сущности либо её экземпляра
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        static string _genKeyTable(object entity)
        {
            Type t_entity = (entity is Type) ? entity as Type : entity.GetType();
            System.Reflection.PropertyInfo[] pis = t_entity.GetProperties();
            System.Reflection.PropertyInfo keyProperty = pis.Where(pi => pi.GetCustomAttributes(typeof(DataAnnotations.KeyAttribute), true).Count() > 0).FirstOrDefault();
            keyProperty = keyProperty ?? pis.Where(pi => "id".Equals(pi.Name.ToLower())).FirstOrDefault();
            keyProperty = string.IsNullOrEmpty(keyProperty.Name) ? null : keyProperty;
            return keyProperty != null ? keyProperty.Name : string.Empty;
        }

        /// <summary>
        /// Получить значение ключа таблицы БД из сущности
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        static int _getKeyTableEntity(object entity)
        {
            if (entity is Type) 
                return 0;
            Type t_entity = (entity is Type) ? entity as Type : entity.GetType();
            System.Reflection.PropertyInfo[] pis = t_entity.GetProperties();
            System.Reflection.PropertyInfo keyProperty = pis.Where(pi => pi.GetCustomAttributes(typeof(DataAnnotations.KeyAttribute), true).Count() > 0).FirstOrDefault();
            keyProperty = keyProperty ?? pis.Where(pi => "id".Equals(pi.Name.ToLower())).FirstOrDefault();
            keyProperty = string.IsNullOrEmpty(keyProperty.Name) ? null : keyProperty;
            
            return keyProperty != null ? (int)keyProperty.GetValue(entity,null) : 0;
        }

        /// <summary>
        /// Обрамляет значение слева и справа для SQL запроса, в зависимости от его типа
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        static string _left_right_ByType(object val)
        {
            string result = val.ToString();
            System.Globalization.NumberFormatInfo ni = new System.Globalization.NumberFormatInfo()
            {
                CurrencyDecimalSeparator = "." //разделитель дробной и целой части
            };
            
            if (val is string)
            {
                result = string.Format("'{0}'", val);
            }

            if (val is decimal || val is double)
            {
                result = string.Format(ni,"{0}", val);
            }
            if (val is Enum)
            {
                result = string.Format("{0}",(int)val);
            }
            return result;
        }

        /// <summary>
        /// Генерация текста запроса, для получения сущностей
        /// </summary>
        /// <param name="t_entity"></param>
        /// <returns></returns>
        static string _genQuerySelect(Type t_entity, string tableName = null)
        {           
            tableName = tableName ??  _genTableName(t_entity);
            return string.Format("SELECT * FROM {0}",tableName);
        }

        /// <summary>
        /// Генерация запроса, для получения сущности по ключу
        /// </summary>
        /// <param name="entity">Ключевая сущность (у которой, необходимо и достаточно заполнить ключевое поле, помеченное аттрибутом Key)</param>
        /// <param name="tableName">Имя таблицы. Не обязательно</param>
        /// <returns>Текст запроса SQL для выполнения</returns>
        static string _genQuerySelectBy(object entity, string tableName = null)
        {
            tableName = tableName ?? _genTableName(entity.GetType());
            string _keyName = _genKeyTable(entity);
            int _key=_getKeyTableEntity(entity);
            return string.Format("SELECT * FROM {0} WHERE {1}={2}", tableName,_keyName,_key);
        }

        /// <summary>
        /// Генерация запроса, для вставки сущности в соответсвующую таблицу БД
        /// </summary>
        /// <param name="entity">Сущность которую будем вставлять</param>
        /// <param name="tableName">Имя таблицы. Не обязательно</param>
        /// <returns>Текст запроса SQL для выполнения</returns>
        static string _genQueryInsert(object entity,string tableName=null)
        {
            tableName = tableName ?? _genTableName(entity.GetType());
            string keyName = _genKeyTable(entity);
            System.Reflection.PropertyInfo[] pis = entity.GetType().GetProperties();
            string columnName = null;
            IDictionary<string, object> kv = new Dictionary<string, object>();
            pis.Aggregate(0, (a, b) =>
                {
                    columnName = b.GetCustomAttributes(typeof(DataAnnotations.DisplayColumnAttribute),true).Select(s=>((DataAnnotations.DisplayColumnAttribute)s).DisplayColumn).FirstOrDefault();
                    columnName = string.IsNullOrEmpty(columnName)?b.Name:columnName;
                    if (!keyName.Equals(columnName))
                        kv.Add(columnName, b.GetValue(entity,null));                    
                    return a;
                });
            string _columns = string.Empty;
            string _values = string.Empty;
            foreach(var _kv in kv)
            {
                _columns+=_kv.Key   + (!_kv.Key.Equals(kv.Last().Key) ? "," : string.Empty);
                _values +=_left_right_ByType(_kv.Value) + (!_kv.Key.Equals(kv.Last().Key) ? "," : string.Empty);
            }
                       
            return string.Format("INSERT INTO {0} ({1}) VALUES ({2})",tableName,_columns,_values);                     
        }


        /// <summary>
        /// Генерация запроса, для обновления параметров сущности в соответсвующей таблице БД
        /// </summary>
        /// <param name="entity">Сущность которую будем обновлять</param>
        /// <param name="tableName">Имя таблицы. Не обязательно</param>
        /// <returns>Текст запроса SQL для выполнения</returns>
        static string _genQueryUpdate(object entity, string tableName = null)
        {
            tableName = tableName ?? _genTableName(entity.GetType());
            string keyName = _genKeyTable(entity);
            int key = _getKeyTableEntity(entity);
            System.Reflection.PropertyInfo[] pis = entity.GetType().GetProperties();
            string columnName = null;
            IDictionary<string, object> kv = new Dictionary<string, object>();
            pis.Aggregate(0, (a, b) =>
            {
                columnName = b.GetCustomAttributes(typeof(DataAnnotations.DisplayColumnAttribute), true).Select(s => ((DataAnnotations.DisplayColumnAttribute)s).DisplayColumn).FirstOrDefault();
                columnName = string.IsNullOrEmpty(columnName) ? b.Name : columnName;
                if (!keyName.Equals(columnName))
                    kv.Add(columnName, b.GetValue(entity,null));
                return a;
            });
            string _data = string.Empty;            
            foreach (var _kv in kv)            
                _data += string.Format("{0}={1}", _kv.Key, _left_right_ByType(_kv.Value)) + (!_kv.Key.Equals(kv.Last().Key) ? "," : string.Empty);
            
            string result = string.Format("UPDATE {0} SET {1} WHERE {2}={3}", tableName, _data, keyName,key);
            System.Windows.Forms.MessageBox.Show(result,"Проверка подготовленного запроса SQL");
            return result;
        }



        /// <summary>
        /// Генерация запроса, для удалениясущности из соответсвующей таблицы БД
        /// </summary>
        /// <param name="entity">Сущность которую будем удалять</param>
        /// <param name="tableName">Имя таблицы. Не обязательно</param>
        /// <returns>Текст запроса SQL для выполнения</returns>
        static string _genQueryDelete(object entity, string tableName = null)
        {
            tableName = tableName ?? _genTableName(entity.GetType());
            string keyName = _genKeyTable(entity);
            int key = _getKeyTableEntity(entity);
            return string.Format("DELETE FROM {0} WHERE {1}={2}", tableName,keyName,key);
        }
        #endregion


        /// <summary>
        /// Генерация скрипта на основе шаблона
        /// </summary>
        /// <param name="Template">Шаблон для отражения</param>
        /// <param name="entity">Сущность</param>
        /// <param name="null_value">Значение для замены нулевых значений</param>
        /// <returns>Текст запроса SQL для выполнения</returns>
        static string ApplyTemplate(string Template, object entity,string null_value="NULL")
        {
            /*
              template = "select r.*R,d.* Realty r 
                          inner join RealtyDetail d 
                          where d.Realty_Id = {Id} and r.NameR = {Name}";
             */
            string result = string.Empty;
            int prev = 0;

            int key = _getKeyTableEntity(entity);
            string keyName = _genKeyTable(entity);

            System.Reflection.PropertyInfo[] pis = entity.GetType().GetProperties();

            //С помощью регулярного выражения получим все позиции входждения шаблона {ИмяСвойстваСущности}
            //и заменим их на реальные значения из объекта entity (если они конечно там есть, если нет то будет заменено на NULL)
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\{(?<val>[^}]*)\}");
            foreach (System.Text.RegularExpressions.Match m in r.Matches(Template))
            {
                if (m.Success)
                {
                    result += Template.Substring(prev, m.Index);
                    var pe = pis.Where(w => w.Name.Equals(m.Groups["val"].Value));
                    if (pe.Count() > 0)
                        result += _left_right_ByType(pe.First().GetValue(entity,null));
                    else
                        result += null_value;
                    prev = m.Index + m.Length;
                }
            }
            if (prev > 0 && prev < Template.Length)
                result += Template.Substring(prev);
            return result;
        }

      /// <summary>
      /// Маппинг результатов выполнения запроса к БД в модель сущности
      /// </summary>
      /// <typeparam name="T">Модель сущности для маппинга</typeparam>
      /// <param name="conn">Соединение с БД</param>
      /// <param name="QueryText">Текст SQL-запроса. Не обязателен (вычисляется на основании модели)</param>
      /// <param name="entityKey">Сущность-Ключ, если необходимо получить единственную сущность</param>
      /// <returns>Список сущностей</returns>
        public static IList<T> GetEntities<T>(IDbConnection conn,string QueryText=null,T entityKey=default(T))
        {
            List<T> result = new List<T>();
            using (conn)//Учтем неуправляемые ресурсы
            {
                IDbCommand cmd = conn.CreateCommand();
                //Если текст запроса пустой то сгенерируем его
                cmd.CommandText = !string.IsNullOrEmpty(QueryText)?QueryText:(entityKey!=null?_genQuerySelectBy(entityKey):_genQuerySelect(typeof(T)));
                if (conn.State != ConnectionState.Open)
                    conn.Open();                
                System.Reflection.PropertyInfo [] pis = typeof(T).GetProperties();
                using (IDataReader rd = cmd.ExecuteReader()) //Закроем после использования reader
                {
                    string columnName = null;
                    while (rd.Read()) //Получим все результаты выборки
                    {
                        T entity = default(T);
                        //Вернем из текущей записи выборки все поля результата
                        for (int f = 0; f < rd.FieldCount; f++)
                        {
                            columnName = rd.GetName(f);
                            //Если значение пустое, то его незачем обрабатывать
                            if (rd.IsDBNull(f)) continue;

                            var pi = pis.Where(w => w.Name.Equals(columnName));

                            if (pi.Count() > 0) //Если имя поля результата совпало со свойством модели
                            {
                                entity = entity == null ? ((T)Activator.CreateInstance(typeof(T))) : entity;
                                pi.First().SetValue(entity, rd.GetValue(f),null);
                            }
                            else
                            {
                                //Если есть уточнение колонки с помощью аттрибута DisplayColumn
                                //в модели сущности совпадающее с выборкой то установим значение
                                pi = pis.Where(w =>
                                        w.GetCustomAttributes(typeof(DataAnnotations.DisplayColumnAttribute), true)
                                          .Where(o => ((DataAnnotations.DisplayColumnAttribute)o).DisplayColumn.Equals(columnName)).Count() > 0
                                    );
                                if (pi.Count() > 0)
                                {
                                    entity = entity == null ? ((T)Activator.CreateInstance(typeof(T))) : entity;
                                    pi.First().SetValue(entity, rd.GetValue(f),null);
                                }
                            }
                        }
                        
                        if(entity!=null)//Добавим к коллекции, если есть результат
                            result.Add(entity);
                    }                     
                }
            }
            return result;
        }


        /// <summary>
        /// Обновляет записи в БД
        /// </summary>
        /// <typeparam name="T">Модель сущности для маппинга</typeparam>
        /// <param name="entities">Список обновляемы объектов</param>
        /// <param name="conn">Подключение к БД</param>
        /// <param name="InsertOrUpdateText">SQL запрос на добавление или обновление данных(если не указан, то формируется ORM, на основании декларации модели и текущего состояния каждого объекта коллекции)</param>
        /// <returns>Список вставленных и обновленых сущностей</returns>
         public static IList<T> PutEntities<T>(ICollection<T> entities,IDbConnection conn,string InsertOrUpdateOrDeleteTemplate=null)
         {
            List<T> result = new List<T>();
            if (entities == null && entities.Count == 0) return result;

            string keyName = _genKeyTable(typeof(T));
            string tableName = _genTableName(typeof(T));
            string columnName = null;
            IDbCommand cmd = conn.CreateCommand();
            using (conn)//Учтем неуправляемые ресурсы
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                foreach (T _entity in entities)
                {
                    if (_entity == null) continue;
                    int key = _getKeyTableEntity(_entity);
                    InsertOrUpdateOrDeleteTemplate =
                        string.IsNullOrEmpty(InsertOrUpdateOrDeleteTemplate) ?
                        (
                        key > 0 ?
                                _genQueryUpdate(_entity,tableName) :
                                key < 0 ? _genQueryDelete(_entity,tableName) : _genQueryInsert(_entity,tableName)
                        ) :
                        InsertOrUpdateOrDeleteTemplate;

                    //Если текст запроса пустой то сгенерируем его
                    cmd.CommandText = InsertOrUpdateOrDeleteTemplate;
                    int _RowAffected =cmd.ExecuteNonQuery();

                    //Если обновление или вставка
                    if (_RowAffected > 0 && key>=0)
                    {

                        if (key == 0)//Если вставка данных                        
                            cmd.CommandText = string.Format("SELECT TOP 1 * FROM {0} ORDER BY {1} DESC",tableName,keyName);
                        else 
                            if (key > 0)//Иначе обновление                        
                            cmd.CommandText = string.Format("SELECT * FROM {0} WHERE {1}={2}", tableName, keyName,key);

                        System.Reflection.PropertyInfo[] pis = _entity.GetType().GetProperties();

                        using (IDataReader rd = cmd.ExecuteReader()) //Закроем после использования reader
                        {                            
                            while (rd.Read()) //Получим все результаты выборки
                            {
                                T entity = default(T);
                                //Вернем из текущей записи выборки все поля результата
                                for (int f = 0; f < rd.FieldCount; f++)
                                {
                                    columnName = rd.GetName(f);
                                    //Если значение пустое, то его незачем обрабатывать
                                    if (rd.IsDBNull(f)) continue;

                                    var pi = pis.Where(w => w.Name.Equals(columnName));

                                    if (pi.Count() > 0) //Если имя поля результата совпало со свойством модели
                                    {
                                        entity = entity == null ? ((T)Activator.CreateInstance(typeof(T))) : entity;
                                        pi.First().SetValue(entity, rd.GetValue(f),null);
                                    }
                                    else
                                    {
                                        //Если есть уточнение колонки с помощью аттрибута DisplayColumn
                                        //в модели сущности совпадающее с выборкой то установим значение
                                        pi = pis.Where(w =>
                                                w.GetCustomAttributes(typeof(DataAnnotations.DisplayColumnAttribute), true)
                                                  .Where(o => ((DataAnnotations.DisplayColumnAttribute)o).DisplayColumn.Equals(columnName)).Count() > 0
                                            );
                                        if (pi.Count() > 0)
                                        {
                                            entity = entity == null ? ((T)Activator.CreateInstance(typeof(T))) : entity;
                                            pi.First().SetValue(entity, rd.GetValue(f),null);
                                        }
                                    }
                                }

                                if (entity != null)//Добавим к коллекции, если есть результат
                                    result.Add(entity);
                            }
                        }
                    }
                }


            }
          
             return result;
         }
    }
}
