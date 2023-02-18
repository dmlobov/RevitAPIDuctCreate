using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPILibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPIDuctCreate
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<MEPCurveType> DuctTypes { get; } = new List<MEPCurveType>();

        public List<Level> Levels { get; } = new List<Level>();

        public DelegateCommand SaveCommand { get; }

        public double WallHeight { get; set; }

        public MEPCurveType SelectedDuctType { get; set; }

        public Level SelectedLevel { get; set; }

        public List<XYZ> Points { get; } = new List<XYZ>();

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DuctTypes = DuctUtils.GetMEPSystemType(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            WallHeight = 100;
            Points = SelectionUtils.GetPoints(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);
        }


        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            MEPSystemType systemType = new FilteredElementCollector(doc)
                .OfClass(typeof(MEPSystemType))
                .Cast<MEPSystemType>()
                .FirstOrDefault(m => m.SystemClassification == MEPSystemClassification.SupplyAir);

            if (Points.Count < 2 ||
              SelectedDuctType == null ||
              SelectedLevel == null)
                return;

            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();
                Duct.Create(doc, systemType.Id, SelectedDuctType.Id, SelectedLevel.Id, Points[0], Points[1]);
                ts.Commit();
            }
        }

        public event EventHandler CloseRequest;

        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
