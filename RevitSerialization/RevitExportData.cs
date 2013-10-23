using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace RevitSerialization
{
    public class ExportedDataType
    {
        [XmlIgnore]
        public int DbId { get; set; }
        [XmlIgnore]
        public Type DbType { get; set; }
        public string TypeName { get; set; }
        public string InstanceName { get; set; }
        public int RevitFamilyId { get; set; }
        public List<Field> Fields { get; set; }
        public List<ExportedInstance> Instances { get; set; }
        public List<ExportedCableInstance> CabelInstances { get; set; }

        [XmlIgnore]
        public bool IsWireObject
        {
            get { return CabelInstances != null && CabelInstances.Any(); }
        }
    }

    public class Field
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public bool IsTemplate { get; set; }
    }

    public class ExportedInstance
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public class ExportedCableInstance : ExportedInstance
    {
        public double Length { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
