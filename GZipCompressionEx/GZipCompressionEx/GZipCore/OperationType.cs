using System.ComponentModel;

namespace GZipCompressionEx
{
	public enum OperationType
	{
		None = 0,
		[Description("compress")]
		Compress = 1,
		[Description("decompress")]
		Decompress = 2
	}
}
