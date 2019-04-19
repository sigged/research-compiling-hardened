using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Sigged.CsC.CodeSamples.Parser
{
    internal class SampleParser
    {
        public static IEnumerable<CodeSample> GetSamples()
        {
            var sampleDir = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Samples"));
            if (sampleDir.Exists)
            {
                foreach (var csFile in sampleDir.EnumerateFiles("*.cs"))
                {
                    if (File.Exists($"{csFile.FullName}.json"))
                    {
                        CodeSample sample = null;
                        try
                        {
                            var sampleMetaJson = File.ReadAllText($"{csFile.FullName}.json");
                            sample = JsonConvert.DeserializeObject<CodeSample>(sampleMetaJson);
                            sample.Contents = File.ReadAllText(csFile.FullName);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Sample Error: {ex.Message}");
                        }
                        if(sample != null)
                            yield return sample;
                    }
                    else
                    {
                        Debug.WriteLine($"Sample Error: Meta json file {csFile.FullName}.json not found.");
                    }
                }
            }
            else
            {
                Debug.WriteLine($"Sample Error: Sample directory {sampleDir.FullName} not found.");
            }
            
        }
    }
}
