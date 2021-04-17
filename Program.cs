using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace XMLTest
{
    class Program
    {
        static void Main(string[] args)
        {
           
            //Create the XmlDocument.
            XmlDocument doc = new XmlDocument();

            //doc.LoadXml("<?xml version='1.0' ?>" +
            //            "<book genre='novel' ISBN='1-861001-57-5'>" +
            //            "<title>Pride And Prejudice</title>" +
            //            "</book>");

            //
            // XmlTextReader Properties Test
            //

            using (XmlTextReader textReader = new XmlTextReader("C:\\temp\\RowsData.xml"))
            {
                textReader.Read();
                //If the node has value
                while (textReader.Read())
                {
                    // Move to fist element  
                    textReader.MoveToElement();
                    Console.WriteLine("XmlTextReader Properties Test");
                    Console.WriteLine("===================");
                    // Read this element's properties and display them on console  
                    Console.WriteLine("Name:" + textReader.Name);
                    Console.WriteLine("Base URI:" + textReader.BaseURI);
                    Console.WriteLine("Local Name:" + textReader.LocalName);
                    Console.WriteLine("Attribute Count:" + textReader.AttributeCount.ToString());
                    Console.WriteLine("Depth:" + textReader.Depth.ToString());
                    Console.WriteLine("Line Number:" + textReader.LineNumber.ToString());
                    Console.WriteLine("Node Type:" + textReader.NodeType.ToString());
                    Console.WriteLine("Attribute Count:" + textReader.Value.ToString());
                }
            }

            Console.WriteLine("===================");
            Console.WriteLine("XmlTextReader Element and attribute test");
            Console.WriteLine("===================");

            using (XmlTextReader reader = new XmlTextReader("C:\\temp\\RowsData.xml"))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            Console.Write("<" + reader.Name);

                            while (reader.MoveToNextAttribute()) // Read the attributes.
                                Console.Write(" " + reader.Name + "='" + reader.Value + "'");
                            Console.Write(">");
                            Console.WriteLine("");
                            break;
                        case XmlNodeType.Text: //Display the text in each element.
                            Console.WriteLine(reader.Value);
                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                            Console.Write("</" + reader.Name);
                            Console.WriteLine(">");
                            break;
                    }
                }
            }

                //Display the document element.
                //Console.WriteLine(doc.DocumentElement.OuterXml);

                // load and validate with schema

                doc = LoadDocumentWithSchemaValidation(false, false);
            XmlNodeList elemList = doc.GetElementsByTagName("title");


            for (int i = 0; i < elemList.Count; i++)
            {
                Console.WriteLine(elemList[i].InnerText);
            }

        }
        //************************************************************************************
        //
        //  Associate the schema with XML. Then, load the XML and validate it against
        //  the schema.
        //
        //************************************************************************************
        public static XmlDocument LoadDocumentWithSchemaValidation(bool generateXML, bool generateSchema)
        {
            XmlReader reader;

            XmlReaderSettings settings = new XmlReaderSettings();

            // Helper method to retrieve schema.
            XmlSchema schema = getSchema(generateSchema);

            if (schema == null)
            {
                return null;
            }
            
            settings.Schemas.Add(schema);

            settings.ValidationEventHandler += settings_ValidationEventHandler;
            settings.ValidationFlags =
                settings.ValidationFlags | XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationType = ValidationType.Schema;

            try
            {
                reader = XmlReader.Create("C:\\temp\\RowsData.xml", settings);
            }
            catch (System.IO.FileNotFoundException)
            {
                if (generateXML)
                {
                    string xml = " "; //= generateXMLString();
                    byte[] byteArray = Encoding.UTF8.GetBytes(xml);
                    MemoryStream stream = new MemoryStream(byteArray);
                    reader = XmlReader.Create(stream, settings);
                }
                else
                {
                    return null;
                }
            }

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;

            doc.Load(reader);

            reader.Close();

            return doc;
        }

        //************************************************************************************
        //
        //  Helper method that generates an XML Schema.
        //
        //************************************************************************************
        private static string generateXMLSchema()
        {
            string xmlSchema =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?> " +
                "<xs:schema attributeFormDefault=\"unqualified\" " +
                "elementFormDefault=\"qualified\" targetNamespace=\"http://www.contoso.com/books\" " +
                "xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"> " +
                "<xs:element name=\"books\"> " +
                "<xs:complexType> " +
                "<xs:sequence> " +
                "<xs:element maxOccurs=\"unbounded\" name=\"book\"> " +
                "<xs:complexType> " +
                "<xs:sequence> " +
                "<xs:element name=\"title\" type=\"xs:string\" /> " +
                "<xs:element name=\"price\" type=\"xs:decimal\" /> " +
                "</xs:sequence> " +
                "<xs:attribute name=\"genre\" type=\"xs:string\" use=\"required\" /> " +
                "<xs:attribute name=\"publicationdate\" type=\"xs:date\" use=\"required\" /> " +
                "<xs:attribute name=\"ISBN\" type=\"xs:string\" use=\"required\" /> " +
                "</xs:complexType> " +
                "</xs:element> " +
                "</xs:sequence> " +
                "</xs:complexType> " +
                "</xs:element> " +
                "</xs:schema> ";
            return xmlSchema;
        }

        //************************************************************************************
        //
        //  Helper method that gets a schema
        //
        //************************************************************************************
        private static XmlSchema getSchema(bool generateSchema)
        {
            XmlSchemaSet xs = new XmlSchemaSet();
            XmlSchema schema;
            try
            {
                schema = xs.Add("http://www.contoso.com/books", "C:\\temp\\RowsData.xsd");
            }
            catch (System.IO.FileNotFoundException)
            {
                if (generateSchema)
                {
                    string xmlSchemaString = generateXMLSchema();
                    byte[] byteArray = Encoding.UTF8.GetBytes(xmlSchemaString);
                    MemoryStream stream = new MemoryStream(byteArray);
                    XmlReader reader = XmlReader.Create(stream);
                    schema = xs.Add("http://www.contoso.com/books", reader);
                }
                else
                {
                    return null;
                }
            }
            return schema;
        }

        //************************************************************************************
        //
        //  Helper method to validate the XML against the schema.
        //
        //************************************************************************************
        private static void validateXML(bool generateSchema, XmlDocument doc)
        {
            if (doc.Schemas.Count == 0)
            {
                // Helper method to retrieve schema.
                XmlSchema schema = getSchema(generateSchema);
                doc.Schemas.Add(schema);
            }

            // Use an event handler to validate the XML node against the schema.
            doc.Validate(settings_ValidationEventHandler);
        }

        //************************************************************************************
        //
        //  Event handler that is raised when XML doesn't validate against the schema.
        //
        //************************************************************************************
        static void settings_ValidationEventHandler(object sender,
            System.Xml.Schema.ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                Console.WriteLine("The following validation warning occurred: " + e.Message);
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                Console.WriteLine("The following critical validation errors occurred: " + e.Message);
            }
        }
    }
}
