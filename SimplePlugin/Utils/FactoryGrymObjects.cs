using GrymCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Utils
{

    /// <summary>
    /// Фабрика объектов Grym 
    /// для удобства доступа из других классов плагина
    /// чтобы не перегружать конструкторы реализаций интерфейсов
    /// </summary>
    public static class FactoryGrymObjects
    {
        static IGrym _grym = null;//Приложение Grym
        static IBaseViewThread _baseView = null;//Оболочка просмотра
        static IDatabase _database = null;//Доступ к базе данных
        static IGrymObjectFactory _factory = null;//Фабрика для создания внутреннийх объектов 2ГИС
        static IBaseViewFrame _frame = null;//
        static IMap _map = null;//Карта
        static IMapGraphics _map_graphics = null;//Грфические элементы карты
        static ILayerCollection _layers = null;//Слои
        static IDirectoryCollection _directories = null;//Справочники
        static IRibbonBar _ribbon_bar = null;//Панель управления
        static IMapCoordinateTransformationGeo _geo_trans = null;//Интерфейс преобразования координат
        /// <summary>
        /// Метод для инициализации свойств
        /// </summary>        
        /// <param name="pBaseView">Оболочка просмотра</param>
        /// <param name="grym">Приложение Grym. Пока не используется (по умолчанию null)</param>
        public static void Init(IBaseViewThread pBaseView, IGrym grym=null)
        {
            _grym = grym;
            _baseView = pBaseView;
            _database = _baseView.Database;
            _factory = _baseView.Factory;
            _frame = _baseView.Frame;
            _map = _frame.Map;            
            _layers = _map.Layers;
            _directories= _frame.DirectoryCollection;
            _ribbon_bar = _frame.MainRibbonBar;
            _geo_trans = _map.CoordinateTransformation as IMapCoordinateTransformationGeo;
            _map_graphics = _map as IMapGraphics;
        }

        public static void Done()
        {
            //Внутренние механизмы доступа к объектам COM, требуют уменьшения счетчика ссылок, для каждого из них
            //поэтому выполним это требование с помощью FinalReleaseComObject
            
            if (_grym != null)
                Marshal.FinalReleaseComObject(_grym);
            if (_baseView != null)
                Marshal.FinalReleaseComObject(_baseView);

            //Обнулим ссылки тем самым указав мусорщику, что COM объекты освобождены
            //и их wrapОбертки должны быть собраны мусорщиком при первом же обходе
           
            _baseView = null;
            _grym = null;

            //Вызовим чистку мусора
            GC.Collect();
            //Ожидаем завершения всех финализаторов
            //внутреннего механизма освобождения управляемых ресуров
            GC.WaitForPendingFinalizers();
        }


        #region Публичные свойства доступа
        
        public static IBaseViewThread BaseView
        {
            get
            {
                return _baseView;
            }           
        }

        public static IDatabase Database
        {
            get
            {
                return _database;
            }            
        }

        public static IGrymObjectFactory Factory
        {
            get
            {
                return _factory;
            }            
        }


        public static IMap Map
        {
            get
            {
                return _map;
            }           
        }

        public static IDirectoryCollection Directories
        {
            get
            {
                return _directories;
            }
        }

        public static ILayerCollection Layers
        {
            get
            {
                return _layers;
            }
        }

        
        public static IRibbonBar RibbonBar
        {
            get
            {
                return _ribbon_bar;
            }
        }

        public static IMapGraphics MapGraphics
        {
            get
            {
                return _map_graphics;
            }
        }
        #endregion

        /// <summary>
        /// Внутренний класс, реализация IMapPoint
        /// </summary>
        internal struct MapPoint:IMapPoint
        {            
           

            public void Set(double nX, double nY)
            {
                X = nX;
                Y = nY;
            }
            public double X
            {
                get;set;                                    
            }
            public double Y
            {
                get;set;
            }
        }

        /// <summary>
        /// Внутренний класс, реализация IDevPoint
        /// </summary>
        internal struct DevPoint : IDevPoint
        {
            public void Set(int nX, int nY)
            {
                X = nX;
                Y = nY;
            }

            public int X
            {
                get;set;                
            }

            public int Y
            {
                get;set;                
            }            
        }

        /// <summary>
        /// Преобразование координат графического устройства карты в локальные координаты
        /// </summary>
        /// <param name="X">координата X графического устройства карты</param>
        /// <param name="Y">координата Y графического устройства карты</param>
        /// <param name="pDevice">Графическое устройство карты</param>
        /// <returns>Локальная координата</returns>
        public static IMapPoint Dev2Local(int X, int Y,IMapDevice pDevice)
        {
           if (pDevice == null)
                return null;
           return pDevice.DeviceToMap(new DevPoint() { X = X, Y = Y });
        }
       
        /// <summary>
        /// Преобразование локальных координат  в географические
        /// </summary>
        /// <param name="p">Локальные координаты</param>
        /// <returns>Географические координаты</returns>
        public static IMapPoint Local2Geo(IMapPoint p)
        {
            return _geo_trans.LocalToGeo(p);
        }

        /// <summary>
        /// Преобразование локальных координат  в географические
        /// </summary>
        /// <param name="X">Локальная координата X</param>
        /// <param name="Н">Локальная координата Н</param>
        /// <returns>Географические координаты</returns>
        public static IMapPoint Local2Geo(double X, double Y)
        {
            return Local2Geo(new MapPoint { X=X, Y=Y});
        }


        /// <summary>
        /// Преобразование географических координат в локальные
        /// </summary>
        /// <param name="p">Географические координаты</param>
        /// <returns>Локальные координаты</returns>
        public static IMapPoint Geo2Local(IMapPoint p)
        {
            return _geo_trans.GeoToLocal(p);
        }

        /// <summary>
        /// Преобразование географических координат в локальные
        /// </summary>
        /// <param name="latitude">Широта</param>
        /// <param name="longitude">Долгота</param>
        /// <returns>Локальные координаты</returns>
        public static IMapPoint Geo2Local(double longitude, double latitude)
        {
            return Geo2Local(new MapPoint { X = longitude, Y = latitude });
        }

    }
}
