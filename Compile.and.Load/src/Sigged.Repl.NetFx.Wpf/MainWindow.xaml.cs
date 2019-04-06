using AurelienRibon.Ui.SyntaxHighlightBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
namespace Sigged.Repl.NetFx.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeHighlighters();
            txtSource.CurrentHighlighter = HighlighterManager.Instance.Highlighters["CSharp"];

            DataContext = new MainWindowsViewModel();
        }

        private void InitializeHighlighters()
        {
            var xsd = Application.GetResourceStream(new Uri("pack://application:,,,/AurelienRibon.Ui.SyntaxHighlightBox;component/resources/syntax.xsd"));
            var schemaStream = xsd.Stream;
            XmlSchema schema = XmlSchema.Read(schemaStream, (s, e) =>
            {
                Debug.WriteLine("Xml schema validation error : " + e.Message);
            });

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.Schemas.Add(schema);
            readerSettings.ValidationType = ValidationType.Schema;

            foreach (var res in GetResources("resources/(.+?)[.]xml"))
            {
                XDocument xmldoc = null;
                try
                {
                    XmlReader reader = XmlReader.Create(res.Value, readerSettings);
                    xmldoc = XDocument.Load(reader);
                }
                catch (XmlSchemaValidationException ex)
                {
                    Debug.WriteLine("Xml validation error at line " + ex.LineNumber + " for " + res.Key + " :");
                    Debug.WriteLine("Warning : if you cannot find the issue in the xml file, verify the xsd file.");
                    Debug.WriteLine(ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return;
                }

                XElement root = xmldoc.Root;
                String name = root.Attribute("name").Value.Trim();
                HighlighterManager.Instance.Highlighters.Add(name, new XmlHighlighter(root));
            }

        }

        /// <summary>
		/// Returns a dictionary of the assembly resources (not embedded).
		/// Uses a regex filter for the resource paths.
		/// </summary>
		private IDictionary<string, UnmanagedMemoryStream> GetResources(string filter)
        {
            var asm = Assembly.GetCallingAssembly();
            string resName = asm.GetName().Name + ".g.resources";
            Stream manifestStream = asm.GetManifestResourceStream(resName);
            ResourceReader reader = new ResourceReader(manifestStream);

            IDictionary<string, UnmanagedMemoryStream> ret = new Dictionary<string, UnmanagedMemoryStream>();
            foreach (DictionaryEntry res in reader)
            {
                string path = (string)res.Key;
                UnmanagedMemoryStream stream = (UnmanagedMemoryStream)res.Value;
                if (Regex.IsMatch(path, filter))
                    ret.Add(path, stream);
            }
            return ret;
        }
    }
}
