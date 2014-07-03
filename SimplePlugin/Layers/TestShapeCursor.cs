using GrymCore;
using SimplePlugin.Models;
using SimplePlugin.Models.AccessMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlugin.Layers
{
    /// <summary>
    /// Реализация курсора
    /// Суть курсора схожа с  курсорами в T-SQL.
    /// Таким образом данный класс предоставляет возможность итерации по заранее заданному набору объектов
    /// с оторажением в свойства регламентированные интерфейсом IPluginShapeCursor
    /// </summary>
    public class TestShapeCursor : IPluginShapeCursor
    {

        /// <summary>
        /// Текущая выборка
        /// </summary>
        IEnumerator<Realty> _selector=null;

        /// <summary>
        /// Экземпляр для отображения информации об объекте недвижимости
        /// </summary>
        Realty _realty = null;
        
        // Создадим маркеры для отображения изображений на слое двух типов        
        IRasterMarkerSymbol symbol_sale = null;
        IRasterMarkerSymbol symbol_lease = null;

        public TestShapeCursor()
        {
            //Загрузим изображения из ресурсов сборки в контекст 2ГИС
            foreach (var ev in Enum.GetValues(typeof(TypeObject)))//где ev это (Sale,Lease и.т.д)
              Utils.RasterCollection.AddFromResource(ev.ToString(), ev.ToString());
            
            symbol_sale = Utils.FactoryGrymObjects.Factory.CreateRasterMarkerSymbol(Utils.RasterCollection.Collection["Sale"],1);
            symbol_lease =  Utils.FactoryGrymObjects.Factory.CreateRasterMarkerSymbol(Utils.RasterCollection.Collection["Lease"], 1);
        }

        /// <summary>
        /// Подготовим выборку объектов MyObject из базы данных
        /// </summary>
        /// <param name="rc">Координаты области для которой готовить отображение объектов</param>
        public void PrepareRect(IMapRect rc)
        {
            //Получим координаты             
            IMapPoint cornerLeftBottom = Utils.FactoryGrymObjects.Local2Geo(rc.MinX, rc.MinY);
            IMapPoint cornerRightUp = Utils.FactoryGrymObjects.Local2Geo(rc.MaxX, rc.MaxY);

            _selector = DbRepository.Realty.Find(cornerLeftBottom.Y,cornerLeftBottom.X, cornerRightUp.Y, cornerRightUp.X).GetEnumerator();
          /*  
            _selector = DbRepository.Realty.Where(w=>                
                w.Longitude >= cornerLeftBottom.X && w.Longitude <= cornerRightUp.X
                &&
                w.Latitude >= cornerLeftBottom.Y && w.Latitude  <= cornerRightUp.Y
                ).GetEnumerator();
           */ 
           // _selector = DbRepository.Realty.GetEnumerator();
            /*
             //Получение всех объектов недвижимости через Entity Framework
             using (Models.DataBaseContext db = new DataBaseContext())
             {
                 _selector = db.Realty.ToList().GetEnumerator();
             }
             */          
        }


        /// <summary>
        /// Подготовим выборку объекта(ов) по идентификатору
        /// </summary>
        /// <param name="id">Уникальный идентификатор объекта(ов)</param>
        public void GetById(int id)
        {            
            _realty = DbRepository.Realty.Find(id);
        }


      
        #region IPluginShapeCursor


        /// <summary>
        /// В этом методе происходит перебор объектов подготовленых функциями GetById или PrepareRect
        /// Входным параметром принимается shape
        /// Каждый shape соответсвует объекту выборки, поэтому нужно для каждого shape 
        /// выполнить метод AddComponent с указанием типа shape
        /// а также заполнить массив точек(геометрию) для соответствующего объекта (точка, линия, полигон)
        /// </summary>
        /// <param name="pShape"></param>
        /// <returns>Если итерация объектов не закончена то TRUE. Если проход по выборке окончен, то FALSE</returns>
        public bool Next(IShapeFill pShape)
        {                       
            //Если выборка заполнена
            if (_selector != null&&_selector.MoveNext())
            {
                _realty = _selector.Current;
                //Получим следующий объект для отрисовки
                //System.Windows.Forms.MessageBox.Show(_selector.Current.Id.ToString(),cnt.ToString());
                //Присвоим shape-объекту идентификатор
                pShape.SetOID(_realty.Id);
                //Укажем, что компонент задается одной точкой (парой координат)
                pShape.AddComponent(ComponentDimension.ComponentDimensionPoint);
                //Преобразуем гео координаты в локальные
                IMapPoint p = Utils.FactoryGrymObjects.Geo2Local(_realty.Longitude,_realty.Latitude);                
                //Укажем их (координаты) для компонента
                pShape.AddPoint(p.X, p.Y);
                return true;
            }            
            return false; //Если все объекты выбраны то укажем о том что курсор закрылся
        }


        public string Label
        {
            get 
            {
                if(_realty!=null)
                    return _realty.Name;
                return null;
            }
        }

        /// <summary>
        /// Координата центра выноски коллаута, обычно вычисляется автоматически
        /// </summary>
        public IMapPoint LabelAnchor
        {
            get 
            {
                return null;
            }
        }
               
        /// <summary>
        /// Информация в коллауте
        /// </summary>
        public string ShapeInfo
        {
            get {

                if (_realty != null&&                    
                    !((Buttons.AddObject.is_add_mode || Buttons.EditObject.is_edit_mode)&& Utils.MonitorCursorOfMap.IsShape(Layers.TestLayer.tag))                    
                    )
                    return _realty.Description;
                return null; 
            }
        }

        /// <summary>
        /// Реализуется, если в методе Next  подставляется тип ComponentDimension.ComponentDimensionPolygon
        /// В нашем случае не реализован
        /// </summary>
        public IFillSymbol FillSymbol
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Реализуется если shape представляет из себя текст
        /// а также если в методе Next  подставляется тип ComponentDimension.ComponentDimensionPoint
        /// В нашем случае не реализован
        /// </summary>
        public ITextSymbol TextSymbol
        {
            get 
            {
                return null;
            }
        }

        /// <summary>
        /// Реализуется, если в методе Next  подставляется тип ComponentDimension.ComponentDimensionLine
        /// В нашем случае не реализован
        /// </summary>
        public ILineSymbol LineSymbol
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Реализуется, если в методе Next  подставляется тип ComponentDimension.ComponentDimensionPoint        
        /// </summary>
        public IMarkerSymbol MarkerSymbol
        {
            get
            {
                if (_realty != null)
                    return _realty.Type == TypeObject.Lease ? symbol_lease : symbol_sale;                
                return null;
            }
        }

        #endregion
    }
}
