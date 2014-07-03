using GrymCore;
using SimplePlugin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Layers
{
    /// <summary>
    /// Реализация тестового слоя, который использует для отображения объектов или форм (так называемых shape)
    /// встроенный инструмент API 2ГИС
    /// </summary>
    public class TestLayer: 
        IControlPlacement, 
        IControlAppearance,
        ILayer, 
        IPluginShapeLayer
    {

        #region Внутренние переменные

        public static string tag = "Grym.TestLayer";

        TestShapeCursor _shape_cursor = new TestShapeCursor();       
        #endregion 
       
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public TestLayer()
        {
            Utils.MonitorCursorOfMap.RegisterClickMap((l, l2) => { 
             
            
            
            });
        }
       


        #region IControlPlacement

        public string PlacementCode
        {
            get {
                return "2000:200"; 
            }
        }
        #endregion


        #region IControlAppearance
        public string Caption
        {
            get { return "Тестовый слой"; }
        }

        public string Description
        {
            get { return "Описание тестового слоя"; }
        }

        public dynamic Icon
        {
            get { return null; }
        }

        public string Tag
        {
            get { return tag; }
        }
        #endregion


        #region ILayer       
        public bool CheckVisible(int nScale, DeviceType nType)
        {
            return true;// nType == DeviceType.DeviceTypeWindow;
        }

        public bool VisibleState
        {
            get;set;            
        }
        #endregion


        #region IPluginShapeLayer
        public bool CheckLabelVisible(int lScale, DeviceType devType)
        {
            return true;// devType == DeviceType.DeviceTypeWindow;
        }

        public IPluginShapeCursor QueryShapeById(int nOID)
        {           
            Utils.MonitorCursorOfMap.TouchShape(this.Tag, nOID);
            _shape_cursor.GetById(nOID);
            return _shape_cursor;
        }


        /// <summary>
        /// Запрос объектов (shape) в заданной координатами pRect отображаемой области
        /// </summary>
        /// <param name="pRect"></param>
        /// <returns></returns>
        public IPluginShapeCursor QueryShapes(IMapRect pRect)
        {
            _shape_cursor.PrepareRect(pRect);
            return _shape_cursor;
        }

        /// <summary>
        /// Получение коэффициента масштабирования текстовых символов слоя
        /// </summary>
        public int ReferenceScale
        {
            get { 
                return 1; 
            }
        }

       /// <summary>
        /// Масштабировать ли текстовые символы слоя
       /// </summary>
        public bool ScalableSymbol
        {
            get { 
                return true; 
            }
        }

        /// <summary>
        /// Возможность выделения объектов слоя и отображения информации о них по щелчку мыши
        /// </summary>
        public bool Selectable
        {
            get 
            { 
                return true;
            }
        }
        #endregion



        
    }
}
