import utils.SecureEncoder;

import javax.imageio.ImageIO;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.FileOutputStream;
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

	int mask = 0;
	int dataByteCursor = 0;
	int dataBitCursor = 1 << 7;
	// 要存放的数据
	byte[] dataBytes = null;
	/**
	 * 解析出的描述信息长度
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

	public void decode(CommandLineArgs.DecodeArgs decode) throws IOException {
		File dataImg = new File(decode.dataImg);
		if (!dataImg.exists()) {
			System.out.printf("文件不存在 %s%n", decode.dataImg);
			return;
		}
		mask = Integer.parseInt(decode.mask, 2);
		BufferedImage image = ImageIO.read(dataImg);
		int width = image.getWidth();
		int height = image.getHeight();
		System.out.printf("图片尺寸: width=%d, height=%d\n", width, height);

		// 1个魔数，4位描述信息长度
		dataBytes = new byte[5];

		dataByteCursor = 0;
		dataBitCursor = 1 << 7;

		for (int h = 0; h < height; h++) {
			for (int w = 0; w < width; w++) {
				int rgb = image.getRGB(w, h);
				extractBits(rgb);
			}
		}

		int dataStartByte = 1 + 4 + profileLength;
		System.out.println(new String(dataBytes, dataStartByte, dataByteLength));

		if (fileName != null && !fileName.isEmpty()) {
			String output = decode.output;
			output = (output == null || output.trim().isEmpty()) ? null : output.trim();
			File file = new File(output, "output_" + fileName);
			boolean flag = file.getParentFile() != null && file.getParentFile().mkdirs();
			try (FileOutputStream fos = new FileOutputStream(file, false)) {
				// 将字节数组写入文件，追加模式
				fos.write(dataBytes, dataStartByte, dataByteLength);
				System.out.println("解析出数据已成功写入到文件: " + file.getAbsolutePath());
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
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
			// 解析出描述信息长度
			if (dataByteCursor == 5) {
				profileLength = ByteBuffer.wrap(dataBytes, 1, 4).getInt();
				System.out.printf("解析出描述信息长度=%s%n", profileLength);
				this.growArray(profileLength);
			}
			// 解析出描述信息
			if (profileLength > 0) {
				if (dataByteCursor == profileLength + 5) {
					byte[] bytes = Arrays.copyOfRange(dataBytes, 5, profileLength + 5);
					String decrypt = SecureEncoder.decrypt(bytes);
					System.out.printf("解析出描述信息=%s%n", decrypt);
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
	 * @param growLength 扩充长度
	 */
	private void growArray(int growLength) {
		byte[] newArray = new byte[dataBytes.length + growLength];
		System.arraycopy(dataBytes, 0, newArray, 0, dataBytes.length);
		dataBytes = newArray;
	}

}
