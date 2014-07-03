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
    /// Класс реализации кнопки "Редактировать объект"
    /// Согласно документации по API 2ГИС должен реализовывать 3 обязательных интефейса    
    /// </summary>
    public class EditObject:Base,ICommandState
    {
        #region Внутренние переменные
        public static bool is_edit_mode = false;
        #endregion       

        public EditObject()
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
            if (is_edit_mode)
            {
                int shape_id = Utils.MonitorCursorOfMap.GetOidShape(Layers.TestLayer.tag);
                if (shape_id > 0)
                {
                    Models.Realty realty = DbRepository.Realty.Find(shape_id);

                    DialogResult r = new Forms.FormRealty().ShowDialog(realty, (IntPtr)Utils.FactoryGrymObjects.BaseView.Frame.HWindow);
                    if (r == DialogResult.OK)
                    {
                        try
                        {
                            DbRepository.Realty.Edit(realty);
                        }
                        catch (Exception exc)
                        {
                            System.Windows.Forms.MessageBox.Show(exc.ToString(), "Ошибка изменения");
                        }
                    }
                }
            }
        }

        #region Base        
        /// <summary>
        /// Обработчик нажатия кнопки
        /// </summary>
        public override void OnClick()
        {

            is_edit_mode = !is_edit_mode;
            Buttons.AddObject.is_add_mode = false;
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
            get { return is_edit_mode; }
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
