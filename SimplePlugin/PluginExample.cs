using GrymCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/*
 * Внимание!
 ** Внимание!!
 *** Внимание!!!
 * Для того чтобы этот проект работал, 
 * убедитесь что запустили MS Visual Studio с правами администратора
 */
namespace SimplePlugin
{
   
    /// <summary>
    /// Плагин должен обязательно реализовывать интерфейс IGrymPlugin
    /// обязательно задать объявление пространства имен using GrymCore
    /// Для получения информации о плагине облочкой 2ГИС, необходимо реализовать интерфейс IGrymPluginInfo
    /// </summary>      
    [ClassInterface(ClassInterfaceType.None)] //Автоматическая диспетчеризация интерфейсов COM-объекта отключена
    [ComVisible(true)] //Атрибут для совместимости с технологией COM, используемой в 2ГИС
    [Guid("D7F866C7-1B7A-4278-9CE5-BE97D66BEDFD")]  //Глобальный идентификатор объекта (Необходим для COM технологии)
   // [ProgId("PluginExample2Gis")]  //Программный идентификатор плагина (Необязателен, используется для псевдонима к CLSID COM-объекта плагина)
    //Декларативное задание информации о плагине для интерефейса IGrymPluginInfo
    [Utils.XMLInfo(typeof(PluginExample),
        Name = "Тестовый плагин"//,         
       // Description = "Описание тестового плагина",
       // Tag="TestPlugin",
       // ApiVersion="1.4",//Необязателен, автоопределение
      //  SupportedLanguages=new string[]{"*"}, //Список поддерживаемых языков (ru,en,fr и.т.д.)
        ,Icon="realty32"    
        )]
    public class PluginExample:IGrymPlugin,IGrymPluginInfo
    {

        #region Регистрация плагина
        /// <summary>
        /// Функция регистрации COM-объекта
        /// В этой функции создается файл dgxpi в каталоге Plugins приложения 2gis
        /// </summary>
        /// <param name="t"></param>
        [ComRegisterFunction]
        public static void OnRegister(Type t)
        {
            bool isGrymInstall = true; //предположим, что 2ГИС установлен
            string grym_exe_FullPath = null;

            try
            {

                //Попробуем получить путь к grym.exe из реестра
                try
                {
                    using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"TypeLib\{7AA02C95-0B4A-43AA-92D8-BA40511A7F3F}"))
                    {
                        if (key != null)
                            foreach (string sk in key.GetSubKeyNames())
                                using (Microsoft.Win32.RegistryKey subkey = key.OpenSubKey(sk))
                                {
                                    string lib_module = subkey.GetValue(@"0\win32\") as string;
                                    grym_exe_FullPath = !string.IsNullOrEmpty(lib_module) && System.IO.File.Exists(lib_module) ? lib_module : null;
                                }
                    }
                }
                catch(Exception exc)
                {
                    //Перенаправим исключение
                    throw exc;
                }

                //Если из реестра не удалось получить
                if(string.IsNullOrEmpty(grym_exe_FullPath))
                //Попробуем получить  путь к grym.exe из COM-объекта GrymCore
                try
                {
                    //Получим тип GrymCore по Guid
                    Type tGrym = Type.GetTypeFromCLSID(new Guid("1FE40EA0-BCD0-4235-B5F1-72123E3BA724"));
                    //Создадим экземпляр GrymCore
                    IGrym grymApp = Activator.CreateInstance(tGrym) as IGrym;
                    if (grymApp != null)
                    {
                        //Получим перечислитель
                        IBaseIterator bi = grymApp.BaseCollection.GetBaseIterator();
                        if (bi != null)
                        {
                            //Выбирем первую базу
                            IBaseReference br = bi.GetNext();
                            if (br != null)
                            {
                                grym_exe_FullPath = br.FullPath;
                                //Освободим объект br
                                Marshal.ReleaseComObject(br);
                            }
                            //Освободим объект bi
                            Marshal.ReleaseComObject(bi);
                        }
                        //Освободим объект grymApp
                        Marshal.ReleaseComObject(grymApp);
                    }
                }
                catch (Exception exc)
                {
                    System.Runtime.InteropServices.COMException cexc = exc as System.Runtime.InteropServices.COMException;
                    isGrymInstall = cexc != null && (long)cexc.ErrorCode == 0x80040154 ? false : isGrymInstall;                    
                    
                    //Перенаправим исключение
                    throw exc;
                }

                //Если путь к grym.exe найден 
                if (!string.IsNullOrEmpty(grym_exe_FullPath))
                {
                    //Создадим экземпляр плагина для получения XMLInfo
                    PluginExample plugin = new PluginExample();

                    //Путь к файлу описания dgxpi
                    string FullPathDgxpi = System.IO.Path.Combine(
                            System.IO.Path.GetDirectoryName(grym_exe_FullPath),
                            string.Format(@"Plugins\{0}.dgxpi", System.IO.Path.GetFileNameWithoutExtension(plugin.GetType().Assembly.GetName().Name))
                                              );
                    //Создадим файл описания dgxpi
                    System.IO.File.WriteAllText(
                        FullPathDgxpi,
                        plugin.XMLInfo);
                }

            }
            catch (Exception exc)
            {                
                //Если регистрация не выполнилась, то выведем сообщение исключения
                System.Windows.Forms.MessageBox.Show(isGrymInstall?exc.ToString():"Не установлен 2ГИС!","Ошибка регистрации плагина");
            }
        }
        #endregion


        #region Внутренние переменные
        IGrym _grymApp=null; //Приложение Grym
        IBaseViewThread _baseView = null;//Оболочка просмотра
        
        IRibbonTab _ribbon_tab = null; //Закладка на ленте
        IRibbonGroup _ribbon_group = null;//Группа в закладке на ленте
        Buttons.AddObject _add_button = null;//кнопка "Добавление объекта" в группе
        Buttons.EditObject _edit_button = null;//кнопка "Редактирование объекта" в группе

        Layers.TestLayer _layer = null;//Пользовательский слой
        #endregion


        #region Работа с элементами управления

        /// <summary>
        /// Создание элементов управления плагином
        /// </summary>
        void CreateControls()
        {
            //Создадим свою закладку на ленте 
            _ribbon_tab = _baseView.Frame.MainRibbonBar.CreateTab("Grym.TestPluginTab", "2000MyRow:2000MyColumn", "Закладка Тестовый плагин");
            //Создадим в закладке группу
            _ribbon_group = _ribbon_tab.CreateGroup(RibbonGroupType.RibbonGroupTypeSimple, "Grym.TestPluginGroup", "2000MyRow:2000MyColumn", "Группа Тестовый плагин");

            ///В документации к API 2ГИС сказанно,
            ///что группа реализует интерфейс IControlSet с методом AddControl
            ///который позволит нам добавить в группу новый элемент управления
            ///в нашем случае добавим две кнопки, которые реализованы в namespace Buttons

            //Убедимся, что явное приведение к интерфейсу IControlSet возможно
            if (_ribbon_group is IControlSet)
            {                
                IControlSet container = _ribbon_group as IControlSet;
                _add_button = new Buttons.AddObject() { Order = 1, Caption = "Вставить новый объект", Description = "Добавляет новый объект на карту" };
                _edit_button = new Buttons.EditObject() { Order = 2, Caption = "Редактировать объект", Description = "Редактирует объект на карте" };
                container.AddControl(_add_button);
                container.AddControl(_edit_button);                
            }
        }

        /// <summary>
        /// Удаление элементов управления
        /// </summary>
        void DestroyControls()
        {
            //Удалим кнопки с группы
            if (_ribbon_group is IControlSet)
            {
                IControlSet container = _ribbon_group as IControlSet;
                if (_add_button != null)
                    container.RemoveControl(_add_button);
                _add_button = null;
                _edit_button = null;
                Marshal.FinalReleaseComObject(container);
            }

            //Внутренние механизмы доступа к объектам COM, требуют уменьшения счетчика ссылок, для каждого из них
            //поэтому выполним это требование с помощью FinalReleaseComObject
            if (_ribbon_group != null)
                Marshal.FinalReleaseComObject(_ribbon_group);
            if (_ribbon_tab != null)
                Marshal.FinalReleaseComObject(_ribbon_tab);

            //Обнулим ссылки тем самым указав мусорщику, что COM объекты освобождены
            //и их wrapОбертки должны быть собраны мусорщиком при первом же обходе
            _ribbon_group = null;
            _ribbon_tab = null;
        }

        #endregion


        #region Реализация IGrymPlugin
        /// <summary>
        /// Инициализация плагина, вызывающий контекст 2ГИС передается в параметрах функциии.
        /// ссылки на который мы сохраним во внутренних переменных плагина, 
        /// чтобы иметь возможность обращаться по мере необходимости к элементам и функционалу согласно API 2ГИС
        /// </summary>
        /// <param name="pRoot"></param>
        /// <param name="pBaseView"></param>
        public void Initialize(Grym pRoot, IBaseViewThread pBaseView)
        {            
            try
            {
                //Заполним ссылки на приложение и оболочку просмотра
                _grymApp = pRoot;
                _baseView = pBaseView;

                //Инициализируем фабрику доступа к объектам 2ГИС
                Utils.FactoryGrymObjects.Init(pBaseView, pRoot);

                //Инициализируем слежение за курсором на карте                
                Utils.MonitorCursorOfMap.Init();

                //Создадим элементы управления
                 CreateControls();
            
                //Добавим пользовательский слой
                _layer = new Layers.TestLayer() { VisibleState = true };
                 Utils.FactoryGrymObjects.Layers.AddLayer(_layer);
                
            }
            catch
            {
                Terminate();
                throw;
            }

        }

        /// <summary>
        /// Завершение работы плагина
        /// Важно понимать отличие управляемых и неуправляемых ресурсов, 
        /// чтобы правильно выполнить действия, по завершению работы плагина        
        /// </summary>
        public void Terminate()
        {

            //Удаление пользовательского слоя
            if(_layer!=null)
            Utils.FactoryGrymObjects.Layers.RemoveLayer(_layer);

            //Удалим пользовательские элементы управления порядке обратном их созданию
            DestroyControls();

            //Завершим слежение за курсором на карте                
            Utils.MonitorCursorOfMap.Done();

            //Освободим объекты фабрики
            Utils.FactoryGrymObjects.Done();

            //Внутренние механизмы доступа к объектам COM, требуют уменьшения счетчика ссылок, для каждого из них
            //поэтому выполним это требование с помощью FinalReleaseComObject
            if (this._grymApp != null)
                Marshal.FinalReleaseComObject(this._grymApp);
            if (this._baseView != null)
                Marshal.FinalReleaseComObject(this._baseView);

            //Обнулим ссылки тем самым указав мусорщику, что COM объекты освобождены
            //и их wrapОбертки должны быть собраны мусорщиком при первом же обходе
            _grymApp = null; 
            _baseView = null;

            //Вызовим чистку мусора
            GC.Collect(); 
            //Ожидаем завершения всех финализаторов
            //внутреннего механизма освобождения управляемых ресуров
            GC.WaitForPendingFinalizers();
        }
        #endregion


        #region Реализация IGrymPluginInfo
        /// <summary>
        /// Информация о плагине
        /// Используем декларативное программирование для этого свойства интерфейса, 
        /// т.е.с помощью кастомного атрибута Utils.XMLInfo у класса плагина
        /// </summary>        
        public string XMLInfo
        {
            get 
            {
                //Попытаемся получить все атрибуты установленные у класса плагина
                //с типом  XMLInfoAttribute определенным в namespace Utils
                 object[] aXMLInfo = this.GetType().GetCustomAttributes(typeof(Utils.XMLInfoAttribute), true);
                //Если такие аттрибуты найдены то берем первый и вызываем у него перегруженный метод ToString
                //Который вернет описание в XML-формате
                if (aXMLInfo != null && aXMLInfo.Length > 0)
                        return (aXMLInfo[0] as Utils.XMLInfoAttribute).ToString();
                
                //Если класс не помечен таким аттрибутом, то фактически не выполняется требование к модулям API 2ГИС
                //Поэтому сгенерируем ошибку
                throw new Exception("Не верно указаны параметры атрибута XMLInfo!");
            }
        }
        #endregion
    }
}
