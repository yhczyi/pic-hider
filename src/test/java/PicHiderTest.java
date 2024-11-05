import org.junit.Test;

import java.io.IOException;

public class PicHiderTest {

	@SuppressWarnings("all")
	public static final String MASK_BINARY_STRING = "00001111" // Alpha Channel Mask
			+ "00001111" // Red Channel Mask
			+ "00001111" // Green Channel Mask
			+ "00001111" // Blue Channel Mask
			;

	@Test
	public void mainTest() throws IOException {

		final String outputDataImg = "C:\\Users\\Administrator\\Desktop\\PicHiderTest\\8-8-无色-嵌入后.png";

		String[] encodeArgs = new String[]{"encode" //
				, "--dataFile", "C:\\Users\\Administrator\\Desktop\\PicHiderTest\\data.txt" //
				, "--coverImage", "C:\\Users\\Administrator\\Desktop\\PicHiderTest\\8-8-无色.png" //
				, "--stegoImage", outputDataImg //
				, "--mask", MASK_BINARY_STRING //
		};
		PicHider.main(encodeArgs);

		String[] decodeArgs = new String[]{"decode" //
				, "--stegoImage", outputDataImg //
				, "--output", "C:\\Users\\Administrator\\Desktop\\PicHiderTest" //
				, "--mask", MASK_BINARY_STRING //
		};
		PicHider.main(decodeArgs);
	}

}
