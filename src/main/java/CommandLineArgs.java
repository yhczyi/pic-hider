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

	public static final String COMMAND_ENCODE = "encode";
	public static final String COMMAND_DECODE = "decode";
	@SuppressWarnings("all")
	public static final String MASK_BINARY_STRING = "00001111"// alpha
			+ "00001111" // red
			+ "00001111" // green
			+ "00001111" // blue
			;

	@Parameter(names = {"-h", "--help", "help"}, help = true, description = "查看帮助信息", order = Integer.MAX_VALUE)
	boolean help;

	@SuppressWarnings("all")
	@Parameter(names = {"-author"}, description = "作者：李春风")
	String author;

	@Parameters(commandNames = {COMMAND_ENCODE}, commandDescription = "加密")
	public static class EncodeArgs {
		@Parameter(names = {"--dataFile"}, description = "需要隐藏的数据文件路径", required = true)
		public String dataFile;

		@Parameter(names = {"--img"}, description = "图片路径，数据文件载体", required = true)
		public String img;

		@Parameter(names = {"--output"}, description = "输出图片路径，包含文件名", required = true)
		public String output;

		@Parameter(names = {"--mask"}, description = "二进制图片掩码 32位，argb颜色位")
		public String mask = MASK_BINARY_STRING;
	}

	@Parameters(commandNames = {COMMAND_DECODE}, commandDescription = "解密")
	public static class DecodeArgs {
		@Parameter(names = {"--dataImg"}, description = "图片路径，数据文件载体", required = true)
		public String dataImg;

		@Parameter(names = {"--output"}, description = "输出目录")
		public String output;

		@Parameter(names = {"--mask"}, description = "二进制图片掩码 32位，argb颜色位")
		public String mask = MASK_BINARY_STRING;
	}

}