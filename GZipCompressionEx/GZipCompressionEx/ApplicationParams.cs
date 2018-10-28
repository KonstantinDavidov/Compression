using System.ComponentModel;

namespace GZipCompressionEx
{
	public class ApplicationParams
	{
		public enum Type
		{
			None = 0,
			[Description("compress")]
			Compress = 1,
			[Description("decompress")]
			Decompress = 2
		}
	}
}
