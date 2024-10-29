package utils;

import javax.crypto.Cipher;
import javax.crypto.spec.SecretKeySpec;
import java.nio.charset.StandardCharsets;
import java.util.Base64;

/**
 * 加密工具类
 *
 * @author lichunming
 * @date 2024/10/27 21:42
 */
public class SecureEncoder {

	// 密钥必须是16字节（128位），或32字节（256位）
	private static final String SECRET_KEY = "mySecretKey12345";

	public static byte[] encrypt(String strToEncrypt) {
		try {
			Cipher cipher = Cipher.getInstance("AES/ECB/PKCS5Padding");
			SecretKeySpec secretKey = new SecretKeySpec(SECRET_KEY.getBytes(StandardCharsets.UTF_8), "AES");
			cipher.init(Cipher.ENCRYPT_MODE, secretKey);
			return cipher.doFinal(strToEncrypt.getBytes(StandardCharsets.UTF_8));
		} catch (Exception ex) {
			throw new RuntimeException(ex);
		}
	}

	public static String decrypt(byte[] strToDecrypt) {
		try {
			Cipher cipher = Cipher.getInstance("AES/ECB/PKCS5Padding");
			SecretKeySpec secretKey = new SecretKeySpec(SECRET_KEY.getBytes(StandardCharsets.UTF_8), "AES");
			cipher.init(Cipher.DECRYPT_MODE, secretKey);
			return new String(cipher.doFinal(strToDecrypt), StandardCharsets.UTF_8);
		} catch (Exception ex) {
			throw new RuntimeException(ex);
		}
	}

	public static String decrypt(String strToDecrypt) {
		return decrypt(Base64.getDecoder().decode(strToDecrypt));
	}

	public static String encodeToString(String strToEncrypt) {
		return Base64.getEncoder().encodeToString(encrypt(strToEncrypt));
	}
}
