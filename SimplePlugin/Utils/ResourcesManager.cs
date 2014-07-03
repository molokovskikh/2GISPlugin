using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Utils
{
    /// <summary>
    /// Менеджер доступа к ресурсам сборки
    /// </summary>
    public static class ResourcesManager
    {
        //Массив рмэнеджеров ресурсов
        static System.Resources.ResourceManager [] _rms=null;

        /// <summary>
        /// Статический конструктор
        /// </summary>
        static ResourcesManager()
        {
            Type _t = typeof(PluginExample);
            Dictionary<string, System.Resources.ResourceManager> rms = new Dictionary<string, System.Resources.ResourceManager>();
                        
            foreach (string r in _t.Assembly.GetManifestResourceNames())
                if(!rms.ContainsKey(r))
                    rms.Add(r,new System.Resources.ResourceManager(r.Replace(".resources",""), _t.Assembly));

            _rms = rms.Select(s=>s.Value).ToArray();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Получить поток из ресурса
        /// </summary>
        /// <param name="resource_name">Имя ресурса</param>
        /// <param name="resource">Имя файла ресурса в проекте (для этого тестового проекта TestResource.resx, используется соответсвующее имя &quot;TestResource&quot;)</param>
        /// <returns>Вовзвращает указатель на открытый поток, который нужно будет завершить после его использования</returns>
        public static System.IO.Stream streamFromResource(string resource_name, string resource = "TestResource")
        {
            System.IO.Stream result = null;
            if(_rms!=null)
            foreach(System.Resources.ResourceManager mr in _rms)
            {
                if (mr.BaseName.Contains(resource))
                {                   
                    try
                    {
                        result = mr.GetStream(resource_name);
                    }
                    catch
                    {
                    }
                   
                }
            }

            if(result==null)
            foreach (System.Resources.ResourceManager mr in _rms)
            {                
                    try
                    {
                        result = mr.GetStream(resource_name);
                    }
                    catch
                    {
                    }
                    if (result != null)
                        break;                
            }

            return result;
        }



        /// <summary>
        /// Получить изображение из ресурса
        /// </summary>
        /// <param name="resource_name">Имя ресурса</param>
        /// <param name="resource">Имя файла ресурса в проекте (для этого тестового проекта TestResource.resx, используется соответсвующее имя &quot;TestResource&quot;)</param>
        /// <returns>Вовзвращает изображение</returns>
        public static System.Drawing.Image imageFromResource(string resource_name, string resource = "TestResource")
        {
            System.Drawing.Image result = null;
            if (_rms != null)
                foreach (System.Resources.ResourceManager mr in _rms)
                {
                    if (mr.BaseName.Contains(resource))
                    {
                        try
                        {
                            result = mr.GetObject(resource_name) as System.Drawing.Image;
                        }
                        catch
                        {
                        }

                    }
                }

            if (result == null)
                foreach (System.Resources.ResourceManager mr in _rms)
                {
                    try
                    {
                        result = mr.GetObject(resource_name) as System.Drawing.Image;
                    }
                    catch
                    {
                    }
                    if (result != null)
                        break;
                }

            return result;
        }

        /// <summary>
        /// Получает массив байт полученный из ресурса сборки
        /// </summary>
        /// <param name="resource_name">Имя ресурса</param>
        /// <param name="resource">Имя файла ресурса в проекте (для этого тестового проекта TestResource.resx, используется соответсвующее имя &quot;TestResource&quot;)</param>
        /// <returns>Массив байт ресурса</returns>
        public static byte[] bytesFromResource(string resource_name, string resource = "TestResource")
        {
            byte[] result = null;
            //Попытаемся получить ресурс как поток
            using (System.IO.Stream src = streamFromResource(resource_name,resource))
            {
                if(src!=null)                
                using (System.IO.MemoryStream dest = new System.IO.MemoryStream())
                {
                    src.CopyTo(dest);
                    result = dest.ToArray();
                }
            }

            //Если не удалось то как массив байт
            if (result == null)
            {

                if (_rms != null)
                {
                    //Сначала поиск по запрашиваемогу имени ресурсного файла
                    foreach (System.Resources.ResourceManager mr in _rms)
                    {
                        if (mr.BaseName.Contains(resource))
                        {
                            try
                            {
                                result = mr.GetObject(resource_name) as byte[];
                            }
                            catch
                            {
                            }

                        }
                    }

                    //Теперь по всем ресурсам
                    if (result == null)
                        foreach (System.Resources.ResourceManager mr in _rms)
                        {
                            try
                            {
                                result = mr.GetObject(resource_name) as byte[];
                            }
                            catch
                            {
                            }
                            if (result != null)
                                break;
                        }
                }

                //Если так и не найден, то попробуем получить как изображение
                if (result == null)
                {
                    using (System.Drawing.Image img = imageFromResource(resource_name, resource))
                    {
                        //Если ресурс изображение то сохраним его в поток
                        if (img != null)
                        {                            
                           using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                           {
                              img.Save(ms, img.RawFormat);
                              //Сохраним из потока в массив байт
                              result=ms.ToArray();
                           }
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Получает текст из ресурса сборки
        /// </summary>
        /// <param name="resource_name">Имя ресурса</param>
        /// <param name="resource">Имя файла ресурса в проекте (для этого тестового проекта TestResource.resx, используется соответсвующее имя &quot;TestResource&quot;)</param>
        /// <returns>Текст из ресурса</returns>
        public static string stringFromResource(string resource_name, string resource = "TestResource")
        {
            string result = null;
            if (_rms != null)
            {
                //Сначала поиск по запрашиваемогу имени ресурсного файла
                foreach (System.Resources.ResourceManager mr in _rms)
                {
                    if (mr.BaseName.Contains(resource))
                    {
                        try
                        {
                            result = mr.GetObject(resource_name) as string;
                        }
                        catch
                        {
                        }

                    }
                }

                //Теперь по всем ресурсам
                if (result == null)
                    foreach (System.Resources.ResourceManager mr in _rms)
                    {
                        try
                        {
                            result = mr.GetObject(resource_name) as string;
                        }
                        catch
                        {
                        }
                        if (result != null)
                            break;
                    }
            }

            return result;
        }
    }
}
