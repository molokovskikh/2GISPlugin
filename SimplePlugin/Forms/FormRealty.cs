using SimplePlugin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimplePlugin.Forms
{
    public partial class FormRealty :
        //Form
        Base<Realty>
    {
        
        public FormRealty()
        {
            InitializeComponent();
         
            foreach (var v in Enum.GetValues(typeof(Models.TypeObject)))
                comboBoxType.Items.Add(v);  
        }   

        /// <summary>
        /// Обновить привязки
        /// </summary>
        public override void RefreshBindings()
        {
            this.textId.DataBindings.Add(new Binding("Text", objectBinding, "Id"));
            this.textName.DataBindings.Add(new Binding("Text", objectBinding, "Name"));
            this.longitude.DataBindings.Add(new Binding("Value", objectBinding, "Longitude"));
            this.latitue.DataBindings.Add(new Binding("Value", objectBinding, "Latitude"));
            this.comboBoxType.DataBindings.Add(new Binding("SelectedItem", objectBinding, "Type"));
            this.richTextBoxDesc.DataBindings.Add(new Binding("Text", objectBinding, "Description"));            
        }

        private void FormRealty_Load(object sender, EventArgs e)
        {
                     
        }
    }
}
