import com.beust.jcommander.JCommander;
import com.beust.jcommander.ParameterException;

import java.io.IOException;

/**
 * 图片隐写
 *
 * @author lichunming
 * @date 2024/10/26 21:58
 */
public class PicHider {

	public static void main(String[] args) throws IOException {

		CommandLineArgs cliArgs = new CommandLineArgs();
		JCommander.Builder builder = JCommander.newBuilder().addObject(cliArgs);

		CommandLineArgs.EncodeArgs encode = new CommandLineArgs.EncodeArgs();
		builder.addCommand(encode);
		CommandLineArgs.DecodeArgs decode = new CommandLineArgs.DecodeArgs();
		builder.addCommand(decode);

		JCommander jc = builder.build();
		try {
			jc.parse(args);
		} catch (ParameterException e) {
			e.usage();
			System.out.println("参数解析失败！！！");
			return;
		}

		if (cliArgs.help) {
			jc.usage();
			return;
		}

		if (CommandLineArgs.COMMAND_ENCODE.equals(jc.getParsedCommand())) {
			Encoder encoder = new Encoder();
			encoder.encode(encode);
		} else if (CommandLineArgs.COMMAND_DECODE.equals(jc.getParsedCommand())) {
			Decoder decoder = new Decoder();
			decoder.decode(decode);
		}
	}

}
