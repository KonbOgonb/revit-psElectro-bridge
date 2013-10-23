
using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PSImport;

namespace RevitImport
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class ImportFromPS : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData,
                                               ref string message,
                                               ElementSet elements)
        {
            if (null == commandData.Application.ActiveUIDocument.Document)
            {
                message = "Active document is null.";
                return Result.Failed;
            }

            try
            {
                var creator = new FamilyInstanceCreator(commandData.Application);

                var importer = new PSDataImporter(creator);

                importer.StartImport();

                return Autodesk.Revit.UI.Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Autodesk.Revit.UI.Result.Failed;
            }
        }
    }
}
