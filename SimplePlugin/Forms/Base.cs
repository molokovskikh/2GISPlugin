using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimplePlugin.Forms
{
    public class Base<T>:Form     
    {
       
        T _object = default(T);
        /// <summary>
        /// Связанный объект
        /// </summary>
        public T objectBinding
        {
            get
            {
                return _object;
            }
            set
            {
                _object = value;
                if (_object != null) //Если значение не пустое то обновим привязки
                    RefreshBindings();
            }
        }

        /// <summary>
        /// Обновить привязки
        /// </summary>
        public virtual void RefreshBindings()
        {
            if (_object == null) 
                return;            
        }

        private void Form_Load(object sender, EventArgs e)
        {
            //Если объект привязки пустой то инициализируем его
            if(objectBinding==null)
             objectBinding=default(T);
        }

        /// <summary>
        /// Внутренний класс для указания родителя формы
        /// </summary>
        internal class parentForm : IWin32Window
        {
            IntPtr _hwnd;
            public parentForm(IntPtr hwnd)
            {
                _hwnd = hwnd;
            }

            public IntPtr Handle
            {
                get { return _hwnd; }
            }
        }


        /// <summary>
        /// Показать форму с привязкой к модели
        /// </summary>
        /// <param name="objectModel">Объект модели для связывания</param>
        /// <param name="hwndParent">Указатель родительского окна</param>
        /// <returns></returns>
        public new void Show(T objectModel, IntPtr hwndParent)
        {                        
            objectBinding = objectModel;
            this.Show(new parentForm(hwndParent));
        }

        /// <summary>
        /// Показать форму с привязкой к модели
        /// </summary>
        /// <param name="objectModel">Объект модели для связывания</param>
        /// <param name="hwndParent">Указатель родительского окна</param>
        /// <returns></returns>
        public new DialogResult ShowDialog(T objectModel, IntPtr hwndParent)
        {
            objectBinding = objectModel;
            return hwndParent.Equals(IntPtr.Zero) ? ShowDialog() : ShowDialog(new parentForm(hwndParent));
        }
    }
}
