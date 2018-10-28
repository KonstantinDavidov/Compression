using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GZipCompressionEx
{
	public class ApplicationValidator
	{
		public bool IsInputParamsValid(string[] args)
		{
			return args.Length == 3;
		}

		public bool ValidateInputParams(string algType, string pathToInputFile, string pathToOutputFile, out ApplicationParams.Type validAlgType, out List<string> errorMessages)
		{
			errorMessages = new List<string>(3);

			if(!Extensions.TryGetEnumValueFromDescription(algType, out validAlgType))
			{
				errorMessages.Add("Incorrect algorythm type. First argument should define the algorithm type and should be one of type: 'compress' or 'decompress'");
			}

			if (!File.Exists(pathToInputFile))
			{
				errorMessages.Add("Source file is not found. Please, check the path to source file and try again.");
			}
			if (File.Exists(pathToOutputFile))
			{
				errorMessages.Add("File with path " + pathToOutputFile + " is already existed. Please, change the path to output file and try again.");
			}

			return !errorMessages.Any();
		}
	}
}
