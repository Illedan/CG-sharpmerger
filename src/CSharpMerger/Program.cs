using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeCopier
{
    class Program
    {
        private static string m_config = "config.data";
        private static string m_path = @"",
            m_outputPath = @"";

        static async Task Main()
        {
            var lastUpdated = DateTime.MinValue;

            Console.WriteLine("Write R to reuse last or directory path for new session");
            var command = Console.ReadLine();
            if (command.ToLower() == "r")
            {
                var contents = File.ReadAllLines(m_config);
                m_path = contents[0];
                m_outputPath = contents[1];
            }
            else
            {
                m_path = command;
                Console.WriteLine("Write target destination file");
                m_outputPath = Console.ReadLine();
                File.WriteAllLines(m_config, new string[] { m_path, m_outputPath });
            }


            while (true)
            {
                try
                {
                    await Task.Delay(500);
                    var files = GetFiles().Where(f => f != m_outputPath).ToArray();
                    var lastEdited = FindLastEdited(files);
                    if (lastEdited == lastUpdated) continue;
                    var mergedFile = CreateMergedCsFile(files.Select(f => File.ReadAllLines(f)).SelectMany(f => f).ToList(), lastEdited);
                    File.WriteAllText(m_outputPath, mergedFile);
                    lastUpdated = lastEdited;
                    Console.WriteLine("Updated: " + lastEdited.ToString("MM/dd/yyyy H:mm \n"));
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
            }
        }

        private static string CreateMergedCsFile(List<string> content, DateTime edited)
        {
            var usings = content.Where(c => c.StartsWith("using", StringComparison.Ordinal)).Distinct().ToList();
            content.RemoveAll(c => c.StartsWith("using", StringComparison.Ordinal));

            return string.Join("\n", usings) + "\n\n\n // LastEdited: " + edited.ToString("dd/MM/yyyy H:mm \n\n\n") + string.Join("\n", content);
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(m_path, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.ToLower().Contains("/bin/") && !f.ToLower().Contains("/obj/") && Path.GetFileName(f) != "AssemblyInfo.cs")
                .ToArray();
        }

        private static DateTime FindLastEdited(string[] filePaths)
        {
            return filePaths.Max(f => File.GetLastWriteTime(f));
        }
    }
}