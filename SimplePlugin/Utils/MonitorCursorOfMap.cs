using GrymCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimplePlugin.Utils
{
    /// <summary>
    /// Так как у нас плагин на оболочку просмотра один
    /// То нужно полагать и экземпляр сервисного класса будет один
    /// На основании этого сделаем статический класс, он как раз существует в единственном экземпляре
    /// </summary>
    public static class MonitorCursorOfMap
    {
        #region Константы
        //События манипулятора мышь
        const int WM_MOUSEMOVE = 0x0200;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_LBUTTONDBLCLK = 0x0203;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;
        const int WM_RBUTTONDBLCLK = 0x0206;
        const int WM_MBUTTONDOWN = 0x0207;
        const int WM_MBUTTONUP = 0x0208;
        const int WM_MBUTTONDBLCLK = 0x0209;      
        #endregion

        #region Внутренние вспомогательные классы
      
     
        /// <summary>
        /// Внутренний класс для установки хука на карту
        /// </summary>
        private class MapWindow
        {
            #region Внутренние переменные
            
            const int GWL_WNDPROC = (-4); //Параметр для SetWindowLong, установки собственного обработчика сообщений окна
            delegate int WndProcDelegateType(IntPtr hWnd, int uMsg, int wParam, int lParam);
            IntPtr hWnd = IntPtr.Zero;//Указатель на окно карты
            IntPtr oldWndProc = IntPtr.Zero;//Указатель на стандартную функцию обработки сообщений окна
            WndProcDelegateType newWndProc =  null;//делегат функции окна
            bool bHooked = false; //Признак установки хука на карту
            #endregion

            #region Импорт WinAPI функций для установки хука
            [DllImport("user32.dll")]
            static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, WndProcDelegateType dwNewLong);

            [DllImport("user32.dll")]
            private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex,IntPtr dwNewLong);

            [DllImport("user32.dll")]
            private static extern int CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int Msg, int wParam, int lParam);
            #endregion



            #region Публичные методы

            /// <summary>
            /// Установлен хук
            /// </summary>
            public bool IsHooked
            {
                get
                {
                    return bHooked;
                }
            }

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="HWindow">Указател окна на которое поставить хук</param>            
            public MapWindow(int HWindow)
            {
                hWnd = (IntPtr)HWindow;
                
            }
                        

            /// <summary>
            /// Поставить хук
            /// </summary>
            /// <param name="f_messageWindow"></param>
            public void Hook(Action<Message> f_messageWindow)
            {
                if (f_messageWindow != null)
                {
                    newWndProc = (IntPtr lhWnd, int Msg, int wParam, int lParam) =>
                    {
                        if (f_messageWindow != null)
                            f_messageWindow(new Message() { HWnd = hWnd, Msg = Msg, WParam = (IntPtr)wParam, LParam = (IntPtr)lParam, Result = lhWnd });

                        //Вызовем стандартный обработчик окна карты
                        return CallWindowProc(oldWndProc, lhWnd, Msg, wParam, lParam);
                    };
                    oldWndProc = SetWindowLong(hWnd, GWL_WNDPROC, newWndProc);
                    bHooked = oldWndProc != IntPtr.Zero;
                }
            }

            /// <summary>
            /// Снять хук
            /// </summary>
            public void Unhook()
            {
                if (bHooked) SetWindowLong(hWnd, GWL_WNDPROC, oldWndProc);
            }
            #endregion
        }




        /// <summary>
        /// Внутренний класс для перевода координат устройства в локальные координаты карты
        /// </summary>
        class UtilGraphic : IGraphicCustom
        {
            public string Tag
            {
                get;
                set;
            }

            public void Draw(IMapDevice pDevice)
            {
                //Получим локальную координату курсора
                IMapPoint _loc_current = FactoryGrymObjects.Dev2Local(_currentX, _currentY, pDevice);
                _mapX = _loc_current.X;
                _mapY = _loc_current.Y;


                //Проверим если индикатор нажатия мыши на карте сработал и координаты графического устройства карты определены
                if (_signal_click && _downX >= 0 && _downY >= 0)
                {
                    //Сбросим индикатор нажатия
                    _signal_click = false;

                    //Получим локальные координаты из координать логического устройства
                    IMapPoint _loc_down = FactoryGrymObjects.Dev2Local(_downX, _downY, pDevice);
                    //Локальные координаты
                    IMapPoint loc_point = new FactoryGrymObjects.MapPoint { X = _loc_down.X, Y = _loc_down.Y };
                    //Сбросим координаты устройства
                    _downX = _downY = -1;
                    //Преобразуем локальные координаты в географические
                    IMapPoint geo_point = FactoryGrymObjects.Local2Geo(loc_point);
                    //Если указан обработчик клика по карте то передадим обе точки, 
                    //в локальных и географических координатах соответственно
                    if (_handlerClick != null)
                        _handlerClick(loc_point, geo_point);
                }
            }

            public IDevRect GetBoundRect(IMapDevice pDevice)
            {
                return pDevice.DeviceRect;
            }

            public bool IsMapPointInside(IMapPoint pPos)
            {
                return false;
            }

            public void OnRemove()
            {

            }

        }

        #endregion


        #region Внутренние переменные      
        static MapWindow _map_window = null;

        /// <summary>
        /// Координаты графического устройства карты
        /// </summary>
        static int _currentX = 0;
        static int _currentY = 0;

        /// <summary>
        /// Локальные координаты карты
        /// </summary>
        static double _mapX = 0;
        static double _mapY = 0;


        /// <summary>
        /// Координаты графического устройства карты при отработки собтия Click
        /// </summary>
        static int _downX = -1;
        static int _downY = -1;
        
        //Индикатор возникновения события Click
        static bool _signal_click = false;

        /// <summary>
        /// Обработчик нажатия на карте
        /// </summary>
        static MapDefineCoordinates _handlerClick = null;


        //Вспомогательный график для конвертации координат графического устройства
        //в локальные координаты карты
        static readonly UtilGraphic _utils_map = new UtilGraphic() { Tag = "2000UtilsGraphicsForCursorMonitor" };

        //Словарь для хранения текущего шейпа, по которому было событие клика мышью (пара ключ\значение = TagLayer\ShapeOid)
        static readonly IDictionary<string, int> _shape_layer_clicks = new Dictionary<string, int>();
        #endregion


        #region Внутренние функции
        /// <summary>
        /// Получить позицию X
        /// </summary>
        /// <param name="LParam"></param>
        /// <returns></returns>
        static int _getX(IntPtr LParam)
        {
            return (((int)LParam)) & 0x0FFFF;
        }

        /// <summary>
        /// Получить позицию Y
        /// </summary>
        /// <param name="LParam"></param>
        /// <returns></returns>
        static int _getY(IntPtr LParam)
        {
            return (((int)LParam)>>16) & 0x0FFFF;
        }


        /// <summary>
        /// Обработка сообщения окна
        /// </summary>
        /// <param name="m">Сообщение пришедшее из очереди окну</param>
        static void OnMessage(Message m)
        {
            //запомним текущее положение мыши
            if (m.Msg.Equals(WM_MOUSEMOVE))
            {             
                    _currentX = _getX(m.LParam);
                    _currentY = _getY(m.LParam);             
            }

            //При нажатии кнопки фиксируем координаты
            if (m.Msg.Equals(WM_RBUTTONDOWN) ||
                m.Msg.Equals(WM_LBUTTONDOWN))
            {
                _downX = _getX(m.LParam);
                _downY = _getY(m.LParam);
            }

            //При отжатии кнопки сверяем координаты с еми что были при нажатии
            //Если всё совпало то генерируем событие по цепочке подписчиков, если они есть
            if (m.Msg.Equals(WM_RBUTTONUP) ||
                m.Msg.Equals(WM_LBUTTONUP))
            {
                int _X = _getX(m.LParam);
                int _Y = _getY(m.LParam);
                if (_X == _downX && _downY == _Y)
                {
                    //System.Windows.Forms.MessageBox.Show(string.Format("X:{0}\tY:{1}", _downX, _downY), "Click");

                    //Установим индикатор клика по карте
                    _signal_click = true;

                    //Почистим список шейпов
                    _shape_layer_clicks.Clear();

                    //Вызовем перерисовку графика, который нам пересчитает 
                    //координаты графического устройства down_XY
                    //в локальные координаты _map_XY                    
                    FactoryGrymObjects.Map.Invalidate(true);
                }
            }                           
           
            
        }

        #endregion


        /// <summary>
        /// Делегат для передачи координат курсора
        /// </summary>
        /// <param name="loc">Локальные координаты</param>
        /// <param name="geo">Географические координаты</param>
        public delegate void MapDefineCoordinates(IMapPoint loc, IMapPoint geo);

        /// <summary>
        /// Инициализация переменных
        /// </summary>       
        /// <param name="pBaseView">Оболочка просмотра. Может быть null, если уже вызван метод FactoryGrymObjects.Init</param>
        public static void Init(IBaseViewThread pBaseView=null)
        {
            //Если не инициализирована фабрика доступа к объектам API 2ГИС,
            //то выполним инициализацию
            if (FactoryGrymObjects.Map == null)
                FactoryGrymObjects.Init(pBaseView);
            _map_window = new MapWindow(FactoryGrymObjects.Map.HWindow);
            _map_window.Hook(OnMessage);            
            FactoryGrymObjects.MapGraphics.AddGraphic(_utils_map);
        }

        
     

        /// <summary>
        /// Осовбодим все ресурсы, что захвачены
        /// </summary>
        public static void Done()
        {
            FactoryGrymObjects.MapGraphics.RemoveGraphic(_utils_map);
            _map_window.Unhook();
            _map_window = null;                     
        }

        /// <summary>
        /// Регистрация обработчика клика по карте
        /// </summary>
        /// <param name="handlerClick"></param>
        public static void RegisterClickMap(MapDefineCoordinates handlerClick)
        {
            if(handlerClick!=null)
            _handlerClick = _handlerClick==null ? handlerClick:_handlerClick+handlerClick;            
        }


        /// <summary>
        /// Получить текущее положение курсора мыши
        /// </summary>
        /// <param name="is_geo">Если требуется вернуть географические координаты, то TRUE. Если локальные то FALSE (по умолчанию TRUE)</param>
        /// <returns></returns>
        public static IMapPoint WhereCursor(bool is_geo = true)
        {
            //Вызовем перерисовку графика, который нам пересчитает 
            //координаты графического устройства current_XY
            //в локальные координаты _map_XY
            FactoryGrymObjects.Map.Invalidate(true);
            //Преобразуем локальные коодинаты в географические если установлен is_geo в TRUE
            IMapPoint result = new FactoryGrymObjects.MapPoint() { X = _mapX, Y = _mapY };
            if(is_geo)
                result = FactoryGrymObjects.Local2Geo(result);
            return result;
        }

        /// <summary>
        /// Вызывается из метода QueryShapeById реализации интерфейса IPluginShapeLayer
        /// Используется для регистрации клика по шейпу в момент клика по карте
        /// </summary>
        /// <param name="tagLayer">Тэг слоя</param>
        /// <param name="Oid">Идентификатор шэйпа</param>
        public static void TouchShape(string tagLayer,int Oid)
        {
            if (_shape_layer_clicks.ContainsKey(tagLayer))
                _shape_layer_clicks.Add(tagLayer, Oid);
            else
                _shape_layer_clicks[tagLayer] = Oid;
        }

        /// <summary>
        /// Проверка на то, что клик произошел на шейпе указаного слоя
        /// </summary>
        /// <param name="tagLayer">Тег слоя</param>
        /// <returns></returns>
        public static bool IsShape(string tagLayer)
        {
            return _shape_layer_clicks.ContainsKey(tagLayer);
        }

        /// <summary>
        /// Возвращает Oid шейпа, на котором был клик
        /// </summary>
        /// <param name="tagLayer"></param>
        /// <returns></returns>
        public static int GetOidShape(string tagLayer)
        {
            return _shape_layer_clicks.ContainsKey(tagLayer)?_shape_layer_clicks[tagLayer]:-1;
        }
    }
}
