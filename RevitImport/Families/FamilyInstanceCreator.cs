using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using RevitSerialization;
using PSImport;

namespace RevitImport
{
    public class FamilyInstanceCreator
    {
        private UIDocument m_revitDoc;
        
        private List<FamilySymbol> m_familySymbolList = new List<FamilySymbol>();
        private List<String> m_familySymbolNameList = new List<String>();
        private int m_defaultIndex = -1;

        public UIDocument RevitDoc
        {
            get
            {
                return m_revitDoc;
            }
        }

        public Application Application
        {
            get { return m_revitDoc.Application.Application; }
        }

        public List<FamilySymbol> FamilySymbolList
        {
            get
            {
                return m_familySymbolList;
            }
        }

        public List<String> FamilySymbolNameList
        {
            get
            {
                return m_familySymbolNameList;
            }
        }

        public int DefaultFamilySymbolIndex
        {
            get
            {
                return m_defaultIndex;
            }
        }

        public FamilyInstanceCreator(UIApplication app)
        {
            m_revitDoc = app.ActiveUIDocument;
        }

        public List<FamilySymbol> GetSymbolsForCategory(BuiltInCategory category)
        {
            return  new FilteredElementCollector(m_revitDoc.Document).
                        OfCategory(category).
                        OfClass(typeof(FamilySymbol)).OfType<FamilySymbol>().ToList();
        }

        public List<FamilySelectorViewModel> CreateFamilyViewModels(IEnumerable<ExportedDataType> exportedDTs)
        {
            var res = new List<FamilySelectorViewModel>();
            foreach (var exportedDataType in exportedDTs)
            {
                var newModel = new FamilySelectorViewModel();

                newModel.DataType = exportedDataType;
                newModel.FamilySymbolsList = new List<Tuple<FamilySymbol, string>>();
                
                var symbols = GetFamilySymbols(exportedDataType);

                foreach (var familySymbol in symbols)
                {
                    newModel.FamilySymbolsList.Add(new Tuple<FamilySymbol, string>(familySymbol, GetSymbolDisplayName(familySymbol)));
                }
                if (!exportedDataType.IsWireObject)
                    SetSuitableTemplateType(newModel, exportedDataType);
                else
                    SetWireTemplateType(newModel);

                SetFullyEqualSymbolAsSelectedIfExists(newModel, exportedDataType);

                //If no suitable founded than set first
                if (newModel.SelectedSymbol == null)
                    newModel.SelectedSymbol = newModel.FamilySymbolsList.FirstOrDefault();

                res.Add(newModel);
            }
            return res;
        }

        private void SetWireTemplateType(FamilySelectorViewModel newModel)
        {
            var wireTemplate = newModel.FamilySymbolsList.FirstOrDefault(fs => fs.Item1.Name == "WireTemplate");

            if (wireTemplate != null)
                newModel.SelectedSymbol = wireTemplate;

        }

        private static void SetSuitableTemplateType(FamilySelectorViewModel newModel, ExportedDataType exportedDataType)
        {
            foreach (var familySymbol in newModel.FamilySymbolsList)
            {
                var templateParam = familySymbol.Item1.get_Parameter("TemplateType");
                if (templateParam != null && templateParam.AsInteger() != 0)
                {
                    if (CompareFields(exportedDataType.Fields.Where(field => field.IsTemplate), familySymbol))
                    {
                        newModel.SelectedSymbol = familySymbol;
                        newModel.CreateCopy = true;
                        return;
                    }
                }
            }
        }

        private static void SetFullyEqualSymbolAsSelectedIfExists(FamilySelectorViewModel newModel,
                                                                  ExportedDataType exportedDataType)
        {
            foreach (var familySymbol in newModel.FamilySymbolsList.Select(fs => fs))
            {
                if (CompareFields(exportedDataType.Fields, familySymbol))
                {
                    newModel.SelectedSymbol = familySymbol;
                    newModel.CreateCopy = false;
                    return;
                }
            }
        }

        private static bool CompareFields( IEnumerable<Field> fields, Tuple<FamilySymbol, string> familySymbol)
        {
            bool allFieldsAreEqual = true;
            foreach (var templateField in fields)
            {
                var revitParam = familySymbol.Item1.get_Parameter(templateField.Name);
                if (revitParam == null || revitParam.AsString() != templateField.Value)
                {
                    allFieldsAreEqual = false;
                    break;
                }
            }
            return allFieldsAreEqual;
        }

        private List<FamilySymbol> GetFamilySymbols(ExportedDataType exportedDataType)
        {
            List<FamilySymbol> symbols;

            if (Enum.IsDefined(typeof(BuiltInCategory), exportedDataType.RevitFamilyId))
                symbols = GetSymbolsForCategory((BuiltInCategory)exportedDataType.RevitFamilyId);
            else
                symbols = GetSymbolsForCategory(BuiltInCategory.OST_LightingDevices);
            return symbols;
        }

        private string GetSymbolDisplayName(FamilySymbol symbol)
        {
            String familyCategoryname = String.Empty;
            if (null != symbol.Family.FamilyCategory)
            {
                familyCategoryname = symbol.Family.FamilyCategory.Name + " : ";
            }
            return String.Format("{0}{1} : {2}", familyCategoryname, symbol.Family.Name, symbol.Name);
        }

        #region family type modification
        public bool CreateAndBindParam( DefinitionFile myDefinitionFile, Category cat, string paramName)
        {
            var myGroup = myDefinitionFile.Groups.get_Item("PsParameters");
            if (myGroup == null)
                myGroup = myDefinitionFile.Groups.Create("PsParameters");

            Definition myDefinition = myGroup.Definitions.Create(paramName, ParameterType.Text, true);

            CategorySet myCategories = Application.Create.NewCategorySet();
            myCategories.Insert(cat);

            TypeBinding typeBinding = Application.Create.NewTypeBinding(myCategories);

            BindingMap bindingMap = m_revitDoc.Document.ParameterBindings;

            bool typeBindOK = bindingMap.Insert(myDefinition, typeBinding, BuiltInParameterGroup.PG_GENERAL);
            return typeBindOK;
        }

        public void UpdateFamilySymbolType(FamilySymbol symbol, ExportedDataType dataType)
        {
            var defFile = CreateOrGetSharedOptionsFile();
            using (var trans = new Transaction(m_revitDoc.Document,"addParams"))
            {
                trans.Start();
                
                foreach (var field in dataType.Fields)
                {
                    var param = symbol.get_Parameter(field.Name);
                    if (param == null)
                    {
                        CreateAndBindParam(defFile,symbol.Category, field.Name);
                        param = symbol.get_Parameter(field.Name);
                    }
                    param.Set(field.Value);
                }
                trans.Commit();
            }
        }

        private DefinitionFile CreateOrGetSharedOptionsFile()
        {
            DefinitionFile defFile = m_revitDoc.Application.Application.OpenSharedParameterFile();
            if (defFile == null)
            {
                System.IO.FileStream fileStream = System.IO.File.Create("sharedParams.txt");
                fileStream.Close();
                Application.SharedParametersFilename = "sharedParams.txt";
                defFile = m_revitDoc.Application.Application.OpenSharedParameterFile();
            }
            return defFile;
        }

        public FamilySymbol CreateNewType(FamilySymbol symbol, string newTypeName)
        {
            Family family = symbol.Family;
            Document familyDoc = m_revitDoc.Document.EditFamily(family);
            if (null != familyDoc)
            {
                FamilyManager familyManager = familyDoc.FamilyManager;

                string newTypeNameFinal = newTypeName;

                using (var trans = new Transaction(familyDoc, "createNewFamilyType"))
                {
                    trans.Start();
                    familyDoc.Regenerate();

                    if (familyManager.Types.OfType<FamilyType>().Any(t => t.Name == newTypeName))
                        newTypeNameFinal += Guid.NewGuid().ToString();

                    FamilyType newFamilyType = familyManager.NewType(newTypeNameFinal);
                    trans.Commit();
                }
                family = familyDoc.LoadFamily(m_revitDoc.Document, new FamilyOption());

                FamilySymbolSetIterator symbolsItor = family.Symbols.ForwardIterator();
                symbolsItor.Reset();
                while (symbolsItor.MoveNext())
                {
                    FamilySymbol familySymbol = symbolsItor.Current as FamilySymbol;
                    if (familySymbol.Name == newTypeNameFinal)
                    {
                        return familySymbol;
                    }
                }
            }
            return null;
        }
        #endregion
        #region instance creation
        
        public FamilyInstance CreateInstanceInPlace(XYZ locationP, FamilySymbol fs)
        {
            FamilyInstance instance = m_revitDoc.Document.Create.NewFamilyInstance(
                locationP, fs, StructuralType.NonStructural);
            m_revitDoc.Selection.Elements.Clear();
            m_revitDoc.Selection.Elements.Add(instance);
            return instance;
        }
        // currently not used
        private bool CreateInstanceOnTheFace(XYZ locationP, FamilySymbol fs, Tuple<Face, Reference> t)
        {
            var projection = t.Item1.Project(locationP);

            if (projection != null)
            {
                FamilyInstance instance = m_revitDoc.Document.Create.NewFamilyInstance(
                    t.Item2, projection.XYZPoint, new XYZ(0, 0, 0), fs);

                m_revitDoc.Selection.Elements.Clear();
                m_revitDoc.Selection.Elements.Add(instance);
                return true;
            }
            return false;
        }
        #endregion

    }
}
