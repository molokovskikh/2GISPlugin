using GrymCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Buttons
{

    /// <summary>
    /// Базовый класс для кнопки
    /// </summary>
    public class Base :
        IControlPlacement,  //Интерфейс доступа к коду размещения
        IControlAppearance, //Интерфейс доступа к параметрам отображения
        ICommandAction     //Интерфейс команды
    {

      
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Base()
        {
            ///Регистрируем подписчика на суррогатное событие "клик на карте"
            Utils.MonitorCursorOfMap.RegisterClickMap(OnClickMap);
            Order = 1;
        }


        /// <summary>
        /// Обработка клика по карте
        /// </summary>
        /// <param name="loc_point">Локальная координата</param>
        /// <param name="geo_point">Географическая координата</param>
        protected virtual void OnClickMap(IMapPoint loc_point, IMapPoint geo_point)
        {
        }

        /// <summary>
        /// Обработчик нажатия
        /// </summary>
        public virtual void OnClick()
        {
        }

        /// <summary>
        /// Порядок размещения кнопки в контейнере
        /// </summary>
        public int Order { get; set; }




        #region IControlPlacement

        /// <summary>
        /// Код размещения
        /// </summary>
        public string PlacementCode
        {
            get;
            set;
        }

        #endregion


        #region IControlAppearance

        /// <summary>
        /// Заголовок кнопки
        /// </summary>
        public string Caption
        {
            get;
            set;
        }

        /// <summary>
        /// Описание кнопки (tooltip - Подсказка)
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Иконка кнопки
        /// </summary>
        public dynamic Icon
        {
            get;
            set;
        }

        /// <summary>
        /// Уникальтный тег кнопки
        /// </summary>
        public string Tag
        {
            get
            {
                return this.GetType().FullName;
            }
            set
            {
            }
        }

        #endregion

        #region ICommandAction
        /// <summary>
        /// Обработчик нажатия кнопки
        /// </summary>
        void ICommandAction.OnCommand()
        {
            OnClick();
        }
        #endregion

       
    }
}
