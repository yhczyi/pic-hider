import javax.imageio.ImageIO;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;

/**
 * 提取图片中隐藏数据
 *
 * @author lichunming
 * @date 2024/10/27 11:05
 */
public class Decoder {

	@SuppressWarnings("all")
	String maskBinaryString = "00000000"// alpha
			+ "00000111" // red
			+ "00000111" // green
			+ "00000111" // blue
			;
	int mask = Integer.parseInt(maskBinaryString, 2);
	int dataByteCursor = 0;
	int dataBitCursor = 1 << 7;
	// 要存放的数据
	byte[] dataBytes = null;

	public void decode() throws IOException {
		BufferedImage image = ImageIO.read(new File("C:\\Users\\Administrator\\Desktop\\ctf\\test\\8-8-无色-加密后.png"));
		int width = image.getWidth();
		int height = image.getHeight();
		System.out.printf("图片尺寸: width=%d, height=%d\n", width, height);

//		int len = image.getHeight() * image.getWidth();
//		len = len * 3; // number of bits to be extracted
//		len = (len + 7) / 8; // bytes to be extracted
//		extract = new byte[len];
		dataBytes = new byte[30];

		dataByteCursor = 0;
		dataBitCursor = 1 << 7;

		for (int h = 0; h < height; h++) {
			for (int w = 0; w < width; w++) {
				int rgb = image.getRGB(w, h);
				extractBits(rgb);
			}
		}

		System.out.println(new String(dataBytes));
	}

	private void extractBits(int rgb) {
		int bitMask = 1 << 31;
		for (int i = 0; i < 32; i++) {
			if ((mask & bitMask) != 0) {
				addBit((rgb & bitMask) == bitMask);
			}
			bitMask >>>= 1;
		}
	}

	private void addBit(boolean num) {
		if (dataByteCursor >= dataBytes.length) {
			return;
		}
		if (num) {
			dataBytes[dataByteCursor] += (byte) dataBitCursor;
		}
		dataBitCursor >>= 1;
		if (dataBitCursor == 0) {
			dataBitCursor = 1 << 7;
			dataByteCursor++;
		}
	}

}
