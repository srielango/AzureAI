using AzureAI.Application.Common.Interfaces;

namespace AzureAI.Infrastructure.Data;
public class FileHandler : IFileHandler
{
    public List<string> GetTextFromFiles()
    {
        var folderPath = Path.GetFullPath("./reviews");
        var folder = new DirectoryInfo(folderPath);

        var response = new List<string>();

        foreach (var file in folder.GetFiles("*.txt"))
        {
            StreamReader sr = file.OpenText();
            var text = sr.ReadToEnd();
            response.Add(text);
            sr.Close();
        }

        return response;
    }
}
