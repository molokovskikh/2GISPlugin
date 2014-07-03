using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Utils
{
    /// <summary>
    /// Атрибут для декларативного задания описания плагина
    /// </summary>
    public class XMLInfoAttribute:Attribute
    {
        //Версия API 2ГИС по умолчанию
        const string default_version_api = "1.1";
        #region Статические методы
        /// <summary>
        /// Словарь для транслита русских букв в английские (для формирования Tag, когда его вдруг забыли указать)
        /// </summary>
        static readonly Dictionary<char, string> _dict = new Dictionary<char, string>();
        /// <summary>
        /// Статический конструктор
        /// </summary>
        static XMLInfoAttribute()
        {
            //Заполняем словарь _dict            
            _dict.Add('а', "a");
            _dict.Add('б', "b");
            _dict.Add('в', "v");
            _dict.Add('г', "g");
            _dict.Add('д', "d");
            _dict.Add('е', "e");
            _dict.Add('ё', "yo");
            _dict.Add('ж', "zh");
            _dict.Add('з', "z");
            _dict.Add('и', "i");
            _dict.Add('й', "j");
            _dict.Add('к', "k");
            _dict.Add('л', "l");
            _dict.Add('м', "m");
            _dict.Add('н', "n");
            _dict.Add('о', "o");
            _dict.Add('п', "p");
            _dict.Add('р', "r");
            _dict.Add('с', "s");
            _dict.Add('т', "t");
            _dict.Add('у', "u");
            _dict.Add('ф', "f");
            _dict.Add('х', "x");
            _dict.Add('ц', "c");
            _dict.Add('ч', "ch");
            _dict.Add('ш', "sh");
            _dict.Add('щ', "shh");
            _dict.Add('ъ', "");
            _dict.Add('ы', "y");
            _dict.Add('ь', "");
            _dict.Add('э', "e");
            _dict.Add('ю', "yu");
            _dict.Add('я', "ya");
            _dict.Add('А', "A");
            _dict.Add('Б', "B");
            _dict.Add('В', "V");
            _dict.Add('Г', "G");
            _dict.Add('Д', "D");
            _dict.Add('Е', "E");
            _dict.Add('Ё', "Yo");
            _dict.Add('Ж', "Zh");
            _dict.Add('З', "Z");
            _dict.Add('И', "I");
            _dict.Add('Й', "J");
            _dict.Add('К', "K");
            _dict.Add('Л', "L");
            _dict.Add('М', "M");
            _dict.Add('Н', "N");
            _dict.Add('О', "O");
            _dict.Add('П', "P");
            _dict.Add('Р', "R");
            _dict.Add('С', "S");
            _dict.Add('Т', "T");
            _dict.Add('У', "U");
            _dict.Add('Ф', "F");
            _dict.Add('Х', "X");
            _dict.Add('Ц', "C");
            _dict.Add('Ч', "Ch");
            _dict.Add('Ш', "Sh");
            _dict.Add('Щ', "Shh");            
            _dict.Add('Ы', "E");            
            _dict.Add('Э', "E");
            _dict.Add('Ю', "yu");
            _dict.Add('Я', "Ya");
        }

        /// <summary>
        /// Транслит с русского в английский
        /// </summary>
        /// <param name="ru_text">Входной текст</param>
        /// <returns>Результат в English</returns>
       static string _transEn(string ru_text)
       {           
           return ru_text.Aggregate(string.Empty, (d, s) => d += _dict.ContainsKey(s)? _dict[s] : new string(s,1) );
       }

        /// <summary>
        /// Возвращает возможную версию текущего API 2ГИС
        /// </summary>
        /// <returns>Версия в формате 1.X</returns>
       static string _apiGrymPsyVersion()
       {           
           string result = default_version_api;
           string _fullpathGrym = null;
           try
           {
               using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"Interface\{6792950C-4F45-489E-9325-9C3F2FCAD953}"))
                   result = key != null && "ISelection2".Equals(key.GetValue("")) ? "1.4" : result;

               if (result.Equals(default_version_api))
               {
                   if (string.IsNullOrEmpty(_fullpathGrym))
                       using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"TypeLib\{7AA02C95-0B4A-43AA-92D8-BA40511A7F3F}"))
                       {
                           if (key != null)
                               foreach (string sk in key.GetSubKeyNames())
                                   using (Microsoft.Win32.RegistryKey subkey = key.OpenSubKey(sk))
                                   {
                                       string lib_module = subkey.GetValue(@"0\win32\") as string;
                                       _fullpathGrym = !string.IsNullOrEmpty(lib_module) && System.IO.File.Exists(lib_module) ? lib_module : null;
                                       if (!string.IsNullOrEmpty(_fullpathGrym)) break;
                                   }

                           if (!string.IsNullOrEmpty(_fullpathGrym))
                           {
                               System.Diagnostics.FileVersionInfo _2gis_ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(_fullpathGrym);
                               result =
                                _2gis_ver.ProductMajorPart > 3 ||
                               (_2gis_ver.ProductMajorPart == 3 && _2gis_ver.ProductMinorPart > 5) ||
                               (_2gis_ver.ProductMajorPart == 3 && _2gis_ver.ProductMinorPart == 5 && _2gis_ver.ProductBuildPart == 2)
                               ? "1.3"
                               : result;
                           }
                       }

               }


               if (result.Equals(default_version_api))
                   using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"Interface\{B8B128C8-8D17-41ef-9C2A-65C5A319E999}"))
                       result = key != null && "IGrymProxyOptions".Equals(key.GetValue("")) ? "1.2" : result;
           }
           catch
           {
               result = default_version_api;
           }
           return result;
       }
        #endregion


       /// <summary>
       /// Конструктор, в параметрах передается тип плагина
       /// </summary>
       /// <param name="tGrymPlugin"></param>
       public XMLInfoAttribute(Type tGrymPlugin)
       {
           object[] aProgId = tGrymPlugin.GetCustomAttributes(typeof(System.Runtime.InteropServices.ProgIdAttribute), true);
           if (aProgId != null && aProgId.Length > 0)
               ProgId = (aProgId[0] as System.Runtime.InteropServices.ProgIdAttribute).Value;
           object[] aCLSID = tGrymPlugin.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), true);
           if (aCLSID != null && aCLSID.Length > 0)
               CLSID = (aCLSID[0] as System.Runtime.InteropServices.GuidAttribute).Value;
       }

       /// <summary>
       /// Глобальный идентификатор CLSID COM-обекта [обязателен]
       /// </summary>
       public string CLSID { get; private set; }
       /// <summary>
       /// Идентификатор ProgID COM-обекта [опционально]
       /// </summary>
        public string ProgId { get; private set; }
        /// <summary>
        /// Название плагина [обязательно]
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Тэг плагина, обязательно уникальный среди других (Если не указан формируется автоматически) [опционально]
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// Описание плагина [опционально]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Копирайт [опционально]
        /// </summary>
        public string Copyright { get; set; }
        /// <summary>
        /// Иконка (base64,  путь к файлу иконки, или имя ресурса из текущей сборки) [опционально]
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// Номер версии API 2ГИС (по  уомолчанию 1.1) [опционально]
        /// </summary>
        public string ApiVersion { get; set; }
        /// <summary>
        /// Поддерживаемые языки (по умолчанию не подставляется) [опционально]
        /// </summary>
        public string [] SupportedLanguages { get; set; }        

        public override string ToString()
        {
            string _iconData = string.Empty;
            //Если задана иконка или путь к файлу
            if(!string.IsNullOrEmpty(Icon))
            {
                //Если указан путь к файлу, и файл существует, то загрузим из него данные
                if (Icon.Length < 300 && !Icon.EndsWith("="))
                {

                    try
                    {
                    
                    //Если файл не найден, предположим что это ресурс
                    if (!System.IO.File.Exists(Icon))
                    {
                        Type _t = this.GetType();
                        
                        //using (System.IO.Stream stream = ResourcesManager.streamFromResource(Icon))
                        {
                            using (System.Drawing.Image img = ResourcesManager.imageFromResource(Icon))
                           // using (System.Drawing.Image img = System.Drawing.Image.FromStream(stream))
                            {
                                if(img!=null)
                                //Сохраним в поток
                                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                                {
                                    img.Save(ms, img.RawFormat);
                                    //Преобразуем из буфера потока в формат base64
                                    _iconData = System.Convert.ToBase64String(ms.ToArray());
                                }
                            }
                        }
                    }
                    else
                    //Попытаемся прочитать файл
                        using (System.Drawing.Image img = System.Drawing.Image.FromFile(Icon))
                        {
                            //Сохраним в поток
                            using (System.IO.MemoryStream ms= new System.IO.MemoryStream())
                            {
                                img.Save(ms, img.RawFormat);
                                //Преобразуем из буфера потока в формат base64
                                _iconData = System.Convert.ToBase64String(ms.ToArray());
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                else
                    _iconData = Icon; //Скорее всего это base64, то что и должно быть
            }
            string _apiDetected = _apiGrymPsyVersion(); //Автоопределение версии API 2ГИС

            //В версии 1.4 нужно обязательно объявлять SupportedLanguages (требование к описанию XML плагинов)
            if ((SupportedLanguages == null || SupportedLanguages.Length == 0)&&
                _apiDetected.Equals("1.4"))
                SupportedLanguages = new string[] { "*" };            
            return string.Format("<grym_plugin_info>{0}{1}{2}{3}{4}{5}{6}<requirements><requirement_api>API-{7}</requirement_api></requirements></grym_plugin_info>",
                !string.IsNullOrEmpty(ProgId) ? string.Format(@"<module_com progid=""{0}""/>", ProgId) : (!string.IsNullOrEmpty(CLSID) ? string.Format(@"<module_com clsid=""{{{0}}}""/>", CLSID) : string.Empty),
                SupportedLanguages == null || SupportedLanguages.Length == 0 ? string.Empty : string.Format("<supported_languages>{0}</supported_languages>", SupportedLanguages.Aggregate(string.Empty, (ll, l) => ll += string.Format("<language>{0}</language>", l))),
                !string.IsNullOrEmpty(Name) ? string.Format(@"<name>{0}</name>", Name) : string.Empty,
                !string.IsNullOrEmpty(Tag) ? string.Format(@"<tag>{0}</tag>", Tag) : (!string.IsNullOrEmpty(Name)? string.Format(@"<tag>{0}</tag>",_transEn(Name)):string.Empty),
                !string.IsNullOrEmpty(Description) ? string.Format(@"<description>{0}</description>", Description) : string.Empty,
                !string.IsNullOrEmpty(Copyright) ? string.Format(@"<copyright>{0}</copyright>", Copyright) : string.Empty,
                !string.IsNullOrEmpty(_iconData) ? string.Format(@"<icon><icon_data>{0}</icon_data></icon>", _iconData) : string.Empty,
                !string.IsNullOrEmpty(ApiVersion)?ApiVersion:(SupportedLanguages != null && SupportedLanguages.Length > 0?"1.4":_apiDetected)
                );
        }
    }
}
