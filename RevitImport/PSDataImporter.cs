using System;
using System.Collections.Generic;
using System.Linq;
using RevitImport;
using RevitSerialization;
using PSImport.UI;
using Autodesk.Revit.DB;

namespace PSImport
{
    class PSDataImporter
    {
        private FamilyInstanceCreator _creator;
        private Dictionary<ExportedDataType, List<ExportedInstance>> _importedData;

        public PSDataImporter(FamilyInstanceCreator creator)
        {
            this._creator = creator;
        }

        public void StartImport()
        {
            var loadedData = RevitSerializetionService.LoadFromXml();
            if (loadedData != null)
                ProcessData(loadedData);

        }

        private void ProcessData(List<ExportedDataType> loadedData)
        {
            var control = new FamilySelector();
            var ds = _creator.CreateFamilyViewModels(loadedData);

            control.DataContext = ds;
            control.ShowDialog();
            if(control.DialogResult == true)
                CreateInstances(ds);
        }

        private void CreateInstances(List<FamilySelectorViewModel> ds)
        {
            UpdateFamilySymbolTypes(ds);
            
            using( var transaction = new Transaction(_creator.RevitDoc.Document, "CreateFamilyInstance"))
            {
                transaction.Start();
                foreach (var vm in ds)
                {
                    foreach (var instanse in vm.DataType.Instances)
                    {
                        var convertedCoords = GetConvertedCoords(instanse);

                        _creator.CreateInstanceInPlace(convertedCoords, vm.SelectedSymbol.Item1);
                    }

                    foreach (var cabel in vm.DataType.CabelInstances)
                    {
                        var convertedCoords = GetConvertedCoords(cabel);
                        var cabelInstance = _creator.CreateInstanceInPlace(convertedCoords, vm.SelectedSymbol.Item1);

                        var lengthParam = cabelInstance.get_Parameter("Length");
                        if (lengthParam != null)
                            lengthParam.Set(Utils.MetersToFeet(cabel.Length));

                        var fromParam = cabelInstance.get_Parameter("From");
                        if (fromParam != null)
                            fromParam.Set(cabel.From);
                    }
                }
                transaction.Commit();
            }
        }

        private static XYZ GetConvertedCoords(ExportedInstance instanse)
        {
            var convertedCoords = new XYZ(
                Utils.MillimetersToFeet(instanse.X),
                Utils.MillimetersToFeet(instanse.Y),
                Utils.MillimetersToFeet(instanse.Z));
            return convertedCoords;
        }

        private void UpdateFamilySymbolTypes(List<FamilySelectorViewModel> ds)
        {
            foreach (var vm in ds)
            {
                var symbol = vm.SelectedSymbol.Item1;
                if (vm.CreateCopy)
                    symbol = _creator.CreateNewType(vm.SelectedSymbol.Item1, vm.DataType.InstanceName);

                _creator.UpdateFamilySymbolType(symbol, vm.DataType);
            }
        }
    }
}
