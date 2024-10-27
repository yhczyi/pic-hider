import java.nio.ByteBuffer;
import java.util.stream.Collectors;
import java.util.stream.Stream;

/**
 * 字节操作工具类
 *
 * @author lichunming
 * @date 2024/10/26 22:50
 */
public class BitUtils {

	/**
	 * 二进制转换字节数组
	 *
	 * @param binaryString 010010000110010101101100011011000110111100100000010101110110111101110010011011000110010000100001
	 * @return
	 */
	public static byte[] binaryStringToByteArray(String binaryString) {
		if (binaryString == null || binaryString.length() % 8 != 0) {
			throw new IllegalArgumentException("Invalid binary string length, must be a multiple of 8.");
		}
		int numberOfBytes = binaryString.length() / 8;
		byte[] bytes = new byte[numberOfBytes];
		for (int i = 0; i < numberOfBytes; ++i) {
			// 获取当前字节的二进制字符串
			String byteString = binaryString.substring(i * 8, (i + 1) * 8);
			// 解析并存储
			bytes[i] = (byte) Integer.parseInt(byteString, 2);
		}
		return bytes;
	}

	/**
	 * 数字转换 2进制字符串，不足指定长度，补充0
	 *
	 * @param binaryNum
	 * @return
	 */
	public static String binaryNum2Str(int binaryNum, int len) {
//		return String.format("%32s", Integer.toBinaryString(binaryNum)).replace(' ', '0');
		return String.format(("%" + len + "s"), Integer.toBinaryString(binaryNum)).replace(' ', '0');
	}

	/**
	 * 数字转换 2进制字符串，不足指定长度，补充0
	 *
	 * @param binaryNum
	 * @return
	 */
	public static String binaryNum2Str2(int binaryNum, int len) {
		String binaryStr = Integer.toBinaryString(binaryNum);
		int leftPadLen = len - (binaryStr.length());
		if (leftPadLen <= 0) {
			return binaryStr;
		}
		String collect = Stream.generate(() -> "0").limit(leftPadLen).collect(Collectors.joining());
		return collect + binaryStr;
	}

	/**
	 * int转换字节数组
	 *
	 * @param value
	 * @return
	 */
	public static byte[] intToByteArray(int value) {
		return ByteBuffer.allocate(4).putInt(value).array();
	}
}
