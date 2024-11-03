import utils.SecureEncoder;

import javax.imageio.ImageIO;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;

/**
 * 多比特填充
 *
 * @author lichunming
 * @date 2024/10/26 10:46
 */
public class Encoder {

	int mask = 0;
	int dataByteCursor = 0;
	int dataBitCursor = 1 << 7;
	// 要存放的数据
	byte[] dataBytes = null;

	public void encode(CommandLineArgs.EncodeArgs encode) throws IOException {
		File imgFile = new File(encode.img);
		if (!imgFile.exists()) {
			System.out.printf("文件不存在 %s%n", encode.img);
			return;
		}
		final String maskBinaryStr = encode.mask;
		mask = Integer.parseInt(maskBinaryStr, 2);
		BufferedImage image = ImageIO.read(imgFile);
		int width = image.getWidth();
		int height = image.getHeight();
		System.out.printf("图片尺寸: width=%d, height=%d\n", width, height);
		// 需要隐写的文件
		File file = new File(encode.dataFile);
		if (!file.exists()) {
			System.out.printf("文件不存在 %s%n", encode.dataFile);
			return;
		}
		// 获取文件名
		String fileName = file.getName();
		long fileSize = file.length();

		// 描述信息文件长度,文件名
		String profile = String.format("%s,%s", fileSize, fileName);
		byte[] encryptBytes = SecureEncoder.encrypt(profile);
		System.out.printf("描述信息长度=%s%n", encryptBytes.length);
		System.out.printf("描述信息=%s%n", profile);

		// 组成：1个字节魔数“l”, 4个字节描述信息长度，n个字节描述信息
		int headerByteSize = (1 + 4 + encryptBytes.length);

		// 计算图片可以隐藏的文件大小
		int canHiddenLen = image.getHeight() * image.getWidth();
		//// 掩码中可以分配出多少个bit用于存储数据
		int bitCount = (int) maskBinaryStr.chars().filter(ch -> ch == '1').count();
		canHiddenLen = canHiddenLen * bitCount;
		canHiddenLen = (canHiddenLen + 7) / 8 - headerByteSize;
		System.out.printf("头长度=%s, 掩码数据位数量=%s, 数据文件大小=%s, 可以隐藏文件大小=%s%n", headerByteSize, bitCount, fileSize, canHiddenLen);
		if (canHiddenLen < fileSize) {
			System.err.println("无法进行隐藏");
			System.exit(1);
		}

		// 计算新数组的大小=头长度+n个字节文件长度
		dataBytes = new byte[headerByteSize + (int) fileSize];

		// 【魔数】。字符“l”
		dataBytes[0] = (byte) 'l';
		// 【描述信息-长度】
		int encryptBytesLength = encryptBytes.length;
		dataBytes[1] = (byte) (encryptBytesLength >>> 24);
		dataBytes[2] = (byte) (encryptBytesLength >>> 16);
		dataBytes[3] = (byte) (encryptBytesLength >>> 8);
		dataBytes[4] = (byte) encryptBytesLength;
		// 描述信息
		System.arraycopy(encryptBytes, 0, dataBytes, 5, encryptBytes.length);
		// 【文件内容】
		int offset = headerByteSize;
		try (FileInputStream fis = new FileInputStream(file)) {
			int bytesRead;
			byte[] buffer = new byte[8192];
			while ((bytesRead = fis.read(buffer)) != -1) {
				if (offset + bytesRead > dataBytes.length) {
					throw new IOException("Buffer overflow: dataBytes is not large enough to hold the file content.");
				}
				System.arraycopy(buffer, 0, dataBytes, offset, bytesRead);
				offset += bytesRead;
			}
		}

		dataByteCursor = 0;
		dataBitCursor = 1 << 7;

		for (int h = 0; h < height; h++) {
			for (int w = 0; w < width; w++) {
				int rgb = image.getRGB(w, h);
				rgb = amendBits(rgb);
				image.setRGB(w, h, rgb);
			}
		}

		File output = new File(encode.output);
		boolean flag = output.getParentFile() != null && output.getParentFile().mkdirs();
		ImageIO.write(image, "png", output);
		System.out.println("写入完成！已成功写入到文件: " + output.getAbsolutePath());
	}

	private int amendBits(int rgb) {
		int bitMask = 1 << 31;
		for (int i = 0; i < 32; i++) {
			if ((mask & bitMask) != 0) {
				boolean bit = get1Bit();
				rgb = setBit(rgb, bitMask, bit);
			}
			bitMask >>>= 1;
		}
		return rgb;
	}

	public boolean get1Bit() {
		if (dataByteCursor >= dataBytes.length) {
			return true;
		}
		int num = dataBytes[dataByteCursor];
		int dataBitMask = dataBitCursor;
		boolean v = (num & dataBitMask) != 0;
		dataBitCursor >>>= 1;
		if (dataBitCursor == 0) {
			dataByteCursor++;
			dataBitCursor = 1 << 7;
		}
		return v;
	}

	public int setBit(int num, int mask, boolean newBit) {
		// 清除原来的第pos位
		num &= ~mask;
		if (newBit) {
			num |= mask;
		}
		return num;
	}

}
