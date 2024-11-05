import com.beust.jcommander.Parameter;
import com.beust.jcommander.Parameters;

/**
 * 命令行参数
 *
 * @author lichunming
 * @link <a href="https://jcommander.org/">参数使用说明</a>
 * @date 2024/10/31 10:11
 */
public class CommandLineArgs {

	/**
	 * 命令：嵌入
	 */
	public static final String COMMAND_ENCODE = "encode";
	/**
	 * 命令：提取
	 */
	public static final String COMMAND_DECODE = "decode";
	@SuppressWarnings("all")
	public static final String MASK_BINARY_STRING = "00001111" // Alpha Channel Mask
			+ "00001111" // Red Channel Mask
			+ "00001111" // Green Channel Mask
			+ "00001111" // Blue Channel Mask
			;

	@Parameter(names = {"-h", "--help", "help"}, help = true, description = "显示帮助信息", order = Integer.MAX_VALUE)
	boolean help;

	@SuppressWarnings("all")
	@Parameter(names = {"-author"}, description = "作者: 李春风")
	String author;

	@Parameters(commandNames = {COMMAND_ENCODE}, commandDescription = "嵌入（将一个文件或信息隐藏到一张图片）")
	public static class EncodeArgs {
		@Parameter(names = {"--dataFile"}, description = "数据文件（需要隐藏的文件）", required = true)
		public String dataFile;

		@Parameter(names = {"--coverImage"}, description = "载体图片（原始图片）", required = true)
		public String coverImage;

		@Parameter(names = {"--stegoImage"}, description = "输出图片（包含隐藏文件的图片）", required = true)
		public String stegoImage;

		@Parameter(names = {"--mask"}, description = "掩码（用来指定图像中哪些位可以被修改来隐藏信息，由ARGB四个分量组成的，每个分量是8位的二进制数）")
		public String mask = MASK_BINARY_STRING;
	}

	@Parameters(commandNames = {COMMAND_DECODE}, commandDescription = "提取（从隐写图像中恢复出原来被隐藏的文件或信息）")
	public static class DecodeArgs {
		@Parameter(names = {"--stegoImage"}, description = "输入图片（包含隐藏文件的图片）", required = true)
		public String stegoImage;

		@Parameter(names = {"--output"}, description = "输出目录")
		public String output;

		@Parameter(names = {"--mask"}, description = "掩码（用来指定图像中哪些位可以被修改来隐藏信息，由ARGB四个分量组成的，每个分量是8位的二进制数）")
		public String mask = MASK_BINARY_STRING;
	}

}