using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GZipTest.GZipCore;

namespace GZipTest
{
	class Program
	{
		static void Main(string[] args)
		{
			var validator = new ApplicationValidator();
			if (!validator.IsInputParamsValid(args))
			{
				Console.WriteLine(
					"Incorrect parameters. There should be 3 input parameters:\n" +
					"1. Algorythm type (compress, decompress).\n" +
					"2. Path to input file.\n" +
					"3. Path to output file. "
				);
				Console.ReadKey();
			}
			else
			{
				var inputAlgType = args[0];
				var pathToInputFile = args[1];
				var pathToOutputFile = args[2];

				OperationType algType;
				List<string> errors;
				validator.ValidateInputParams(inputAlgType, pathToInputFile, pathToOutputFile, out algType, out errors);

				if (errors.Any())
				{
					DisplayErrors(errors);
					Console.ReadKey();
				}
				else
				{
					var sw = new Stopwatch();
					sw.Start();

					var gzipCompressor = algType == OperationType.Compress
						? (GZipTemplateMethod) new GZipCompressor(pathToInputFile, pathToOutputFile, ReportProgress)
						: new GZipDecompressor(pathToInputFile, pathToOutputFile, ReportProgress);

					try
					{
						gzipCompressor.Start();
					}
					catch (Exception e)
					{
						sw.Stop();
						Console.WriteLine(e);
						throw;
					}

					sw.Stop();
					Console.WriteLine("\nExecuted time (milliseconds): " + sw.ElapsedMilliseconds);
					Console.WriteLine("Executed time (seconds): " + sw.Elapsed.Seconds);

					Console.WriteLine("Press any key to exit");
					Console.ReadKey();
				}
			}
		}

		private static void DisplayErrors(IEnumerable<string> errors)
		{
			foreach (var error in errors)
				Console.WriteLine(error);
		}

		private static void ReportProgress(double progressPercentValue)
		{
			Console.Write(string.Format("\rCurrent Progress: {0:N2} %", progressPercentValue));
		}
	}
}
