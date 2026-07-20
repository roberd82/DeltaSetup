namespace DeltaPatcherCLI;
using System.Diagnostics;
using System.IO;
using System.Reflection;

public class Apk {
	
	public static void ExtractEmbeddedJar(string jarFileName) {
		string resourceName = $"DeltaPatcherCLI.{jarFileName}"; 
		string outputPath = Path.Combine(Path.GetTempPath(), jarFileName);

		var assembly = Assembly.GetExecutingAssembly();

		using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
			if (stream == null) {
				throw new Exception($"Resource '{resourceName}' not found.");
			}

			using (FileStream fileStream = new FileStream(outputPath, FileMode.Create)) {
				stream.CopyTo(fileStream);
			}
		}
	}
}