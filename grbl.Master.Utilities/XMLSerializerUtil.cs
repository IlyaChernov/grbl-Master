namespace grbl.Master.Utilities
{
    using System.IO;
    using System.Xml.Serialization;

    public static class XMLSerializerUtil
    {
        public static string SerializeToXML<T>(this T toSerialize)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static T XmlDeserialize<T>(this string toDeserialize)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var textReader = new StringReader(toDeserialize))
            {
                return (T)xmlSerializer.Deserialize(textReader);
            }
        }
    }
}
