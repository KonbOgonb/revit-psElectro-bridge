using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitImport
{
    internal class FindWallHelper
    {
        public static Tuple<Face, Reference> GetClosestFace(Document document, XYZ p)
        {
            Face resultFace = null;
            Reference resultReference = null;

            double min_distance = double.MaxValue;
            FilteredElementCollector collector = new FilteredElementCollector(document);
            var walls = collector.OfClass(typeof (Wall));
            foreach (Wall wall in walls)
            {
                IList<Reference> sideFaces =
                    HostObjectUtils.GetSideFaces(wall, ShellLayerType.Interior);
                // access the side face
                Face face = document.GetElement(sideFaces[0]).GetGeometryObjectFromReference(sideFaces[0]) as Face;
                var intersection = face.Project(p);

                if (intersection != null)
                {
                    if (intersection.Distance < min_distance)
                    {
                        resultFace = face;
                        resultReference = sideFaces[0];
                        min_distance = intersection.Distance;
                    }
                }
            }
            //resultFace.
            return new Tuple<Face,Reference>( resultFace, resultReference);
        }
    }
}
