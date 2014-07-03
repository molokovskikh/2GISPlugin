using GrymCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Utils
{
    /// <summary>
    /// Коллекция изображений используемых в плагине
    /// </summary>
    public static class RasterCollection
    {        
        static readonly IDictionary<string, IRaster> _collection = new Dictionary<string, IRaster>();
       
        /// <summary>
        /// Словарь для получение изображения по его имени
        /// </summary>
        public static IDictionary<string, IRaster> Collection
        {
            get
            {
                return _collection;
            }
        }

       /// <summary>
       /// Добавляет изображение из ресурсов сборки к коллекции изображений
       /// </summary>
       /// <param name="tagRaster">Уникальный тег изображения</param>
       /// <param name="resource_name">Имя ресурса</param>
        public static void AddFromResource(string tagRaster,string resource_name)
        {
            try
            {
                if (!_collection.ContainsKey(tagRaster) && FactoryGrymObjects.Factory != null)
                {
                    byte[] bytes = ResourcesManager.bytesFromResource(resource_name);                    
                    if (bytes != null)
                    {                       
                        _collection.Add(tagRaster, FactoryGrymObjects.Factory.CreateRasterFromMemory(bytes));
                    }
                }
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show(exc.ToString(), "Добавление ресурса в коллекцию");
            }
        }

    }
}
