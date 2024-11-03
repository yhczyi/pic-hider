import org.junit.Test;

import java.io.IOException;

public class PicHiderTest {

	@SuppressWarnings("all")
	public static final String MASK_BINARY_STRING = "00001111"// alpha
			+ "00001111" // red
			+ "00001111" // green
			+ "00001111" // blue
			;

	@Test
	public void mainTest() throws IOException {

		final String outputDataImg = "C:\\Users\\Administrator\\Desktop\\tmp\\PicHiderTest\\8-8-无色-加密后.png";

		String[] encodeArgs = new String[]{"encode" //
				, "--dataFile", "C:\\Users\\Administrator\\Desktop\\tmp\\PicHiderTest\\data.txt" //
				, "--img", "C:\\Users\\Administrator\\Desktop\\tmp\\PicHiderTest\\8-8-无色.png" //
				, "--output", outputDataImg //
				, "--mask", MASK_BINARY_STRING //
		};
		PicHider.main(encodeArgs);

		String[] decodeArgs = new String[]{"decode" //
				, "--dataImg", outputDataImg //
				, "--output", "C:\\Users\\Administrator\\Desktop\\tmp\\PicHiderTest" //
				, "--mask", MASK_BINARY_STRING //
		};
		PicHider.main(decodeArgs);
	}

}
