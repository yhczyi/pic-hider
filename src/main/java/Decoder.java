import utils.SecureEncoder;

import javax.imageio.ImageIO;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.Arrays;

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
	/**
	 * 解析出的【描述信息】长度
	 */
	int profileLength = 0;
	/**
	 * 数据长度
	 */
	int dataByteLength = 0;
	/**
	 * 文件名
	 */
	String fileName = null;

	public void decode() throws IOException {
		BufferedImage image = ImageIO.read(new File("C:\\Users\\Administrator\\Desktop\\ctf\\test\\8-8-无色-加密后.png"));
		int width = image.getWidth();
		int height = image.getHeight();
		System.out.printf("图片尺寸: width=%d, height=%d\n", width, height);

		// 1个模数，4位【描述信息】长度
		dataBytes = new byte[5];

		dataByteCursor = 0;
		dataBitCursor = 1 << 7;

		for (int h = 0; h < height; h++) {
			for (int w = 0; w < width; w++) {
				int rgb = image.getRGB(w, h);
				extractBits(rgb);
			}
		}

		System.out.println(new String(dataBytes));
		int dataStartByte = 1 + 4 + profileLength;
		System.out.println(new String(dataBytes, dataStartByte, dataByteLength));
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
		// (dataBitCursor == 0) 表示下标 dataByteCursor 的字节已经读取完
		if (dataBitCursor == 0) {
			dataBitCursor = 1 << 7;
			dataByteCursor++;
			// 解析出【描述信息】长度
			if (dataByteCursor == 5) {
				profileLength = ByteBuffer.wrap(dataBytes, 1, 4).getInt();
				System.out.printf("解析出【描述信息】长度=%s%n", profileLength);
				this.growArray(profileLength);
			}
			// 解析出【描述信息】
			if (profileLength > 0) {
				if (dataByteCursor == profileLength + 5) {
					byte[] bytes = Arrays.copyOfRange(dataBytes, 5, profileLength + 5);
					String decrypt = SecureEncoder.decrypt(bytes);
					System.out.printf("解析出【描述信息】=%s%n", decrypt);
					dataByteLength = Integer.parseInt(decrypt.split(",")[0]);
					fileName = decrypt.split(",")[1];
					this.growArray(dataByteLength);
				}
			}
		}
	}

	/**
	 * 扩充 dataBytes 数组长度
	 *
	 * @param growLength
	 */
	private void growArray(int growLength) {
		byte[] newArray = new byte[dataBytes.length + growLength];
		System.arraycopy(dataBytes, 0, newArray, 0, dataBytes.length);
		dataBytes = newArray;
	}

}
