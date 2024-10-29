import utils.SecureEncoder;

import javax.imageio.ImageIO;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.nio.ByteBuffer;

/**
 * 多比特填充
 *
 * @author lichunming
 * @date 2024/10/26 10:46
 */
public class Encoder {

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

	public void encode() throws IOException {
		BufferedImage image = ImageIO.read(new File("C:\\Users\\Administrator\\Desktop\\ctf\\test\\8-8-无色.png"));
		int width = image.getWidth();
		int height = image.getHeight();
		System.out.printf("图片尺寸: width=%d, height=%d\n", width, height);
		String data = "Hello World!------------------";
		// 将字符串转换为字节数组
		byte[] dataBytes0 = data.getBytes();

		String fileName = "测试文件aaa1.txt";
		// 【描述信息】文件长度,文件名
		String profile = String.format("%s,%s", dataBytes0.length, fileName);
		byte[] encryptBytes = SecureEncoder.encrypt(profile);
		System.out.println(new String(encryptBytes));
		System.out.printf("描述信息长度=%s%n", encryptBytes.length);

		// 计算新数组的大小。组成：1个字节魔数“l”, 4个字节描述信息长度，n个字节描述信息，n个字节文件长度
		int newDataLength = (1 + 4 + encryptBytes.length) + dataBytes0.length;
		// 创建新的字节数组
		dataBytes = new byte[newDataLength];

		// 【魔数】。字符“l”
		dataBytes[0] = (byte) 'l';
		// 【描述信息-长度】
		ByteBuffer buffer = ByteBuffer.allocate(4);
		buffer.putInt(encryptBytes.length);
		buffer.flip();
		buffer.get(dataBytes, 1, 4);
		// 【描述信息】
		System.arraycopy(encryptBytes, 0, dataBytes, 5, encryptBytes.length);
		// 【文件内容】
		System.arraycopy(dataBytes0, 0, dataBytes, (1 + 4 + encryptBytes.length), dataBytes0.length);

		dataByteCursor = 0;
		dataBitCursor = 1 << 7;

		for (int h = 0; h < height; h++) {
			for (int w = 0; w < width; w++) {
				int rgb = image.getRGB(w, h);
				rgb = amendBits(rgb);
				image.setRGB(w, h, rgb);
			}
		}

		ImageIO.write(image, "png", new File("C:\\Users\\Administrator\\Desktop\\ctf\\test\\8-8-无色-加密后.png"));
		System.out.println("写入完成！");
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
