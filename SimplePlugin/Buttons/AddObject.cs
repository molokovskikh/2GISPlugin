using GrymCore;
using SimplePlugin.Models.AccessMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimplePlugin.Buttons
{
    /// <summary>
    /// Класс реализации кнопки "Вставить новый объект"
    /// Согласно документации по API 2ГИС должен реализовывать 3 обязательных интефейса    
    /// </summary>
    public class AddObject:Base,      
        ICommandState//Интерфейс предназначен для контроля состояния элемента управления
    {

        #region Внутренние переменные
        public static bool is_add_mode = false;
        #endregion        
      

        public AddObject()
        {
            PlacementCode = string.Format(@"<control_pos><size min_width=""100"" max_width=""200"" height_in_row=""2""/><position column_id=""100"" row_id=""{1}000{0}"" order_in_row=""{1}"" draw_external_caption=""true""/></control_pos>",
               this.GetType().Name, this.Order);
            Icon = null;
        }

       /// <summary>
       /// Обработка клика по карте
       /// </summary>
       /// <param name="loc_point">Локальная координата</param>
       /// <param name="geo_point">Географическая координата</param>
        protected override void OnClickMap(IMapPoint loc_point, IMapPoint geo_point)
        {
            if (is_add_mode)
            {
                int shape_id = Utils.MonitorCursorOfMap.GetOidShape(Layers.TestLayer.tag);
                Models.Realty realty =
                    shape_id<0?
                    new Models.Realty() { Longitude = geo_point.X, Latitude = geo_point.Y }
                    :
                    DbRepository.Realty.Find(shape_id);
                     
                DialogResult r = new Forms.FormRealty().ShowDialog(realty,(IntPtr)Utils.FactoryGrymObjects.BaseView.Frame.HWindow);
                if (r == DialogResult.OK)
                {
                    try
                    {
                        if (shape_id < 0)
                            DbRepository.Realty.Add(realty);
                        else
                            DbRepository.Realty.Edit(realty);
                        /*
                        //Обновление с использованием Entity Framework
                        using (Models.DataBaseContext db = new Models.DataBaseContext())
                        {
                            db.Realty.Add(realty);
                            db.SaveChanges();
                        }
                         */ 
                    }
                    catch (Exception exc)
                    {
                        System.Windows.Forms.MessageBox.Show(exc.ToString(), "Ошибка добавления");
                    }

                }                
                //System.Windows.Forms.MessageBox.Show(string.Format("Local = X:{0}\tY:{1}\nGeo = X:{2}\tY:{3}", loc_point.X, loc_point.Y, geo_point.X, geo_point.Y), "Слежение за позицией");
            }
        }

        #region Base
        
        /// <summary>
        /// Обработчик нажатия кнопки
        /// </summary>
        public override void OnClick()
        {
            //Если кликнули
            is_add_mode = !is_add_mode;
            Buttons.EditObject.is_edit_mode = false;
        }

        #endregion      
      
     
        #region ICommandState

        /// <summary>
        /// Видимость элемента управления
        /// </summary>
        public bool Available
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Cостояние "выбран/не выбран"
        /// </summary>
        public bool Checked
        {
            get { return is_add_mode; }
        }

        /// <summary>
        /// состояние "включен/выключен"
        /// </summary>
        public bool Enabled
        {
            get { return true; }
        }
        #endregion
    }
}
