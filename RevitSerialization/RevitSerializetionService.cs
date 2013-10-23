using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace RevitSerialization
{
    public class RevitSerializetionService 
    {
        public static void ExportToXml(List<ExportedDataType> objects)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (var fileStream = new StreamWriter(saveFileDialog.FileName))
                {
                    var serializer = new XmlSerializer(objects.GetType());
                    serializer.Serialize(fileStream,objects);
                }
            }
        }
        
        public static List<ExportedDataType> LoadFromXml()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (var fs = new StreamReader(openFileDialog.FileName))
                {
                    var serializer = new XmlSerializer(typeof (List<ExportedDataType>));
                    return serializer.Deserialize(fs) as List<ExportedDataType>;
                }
            }
            return null;
        }
    }
}
