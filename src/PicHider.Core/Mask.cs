using System.Numerics;

namespace PicHider.Core;

/// <summary>32 位掩码，按 bit31→bit0 对应 Alpha/Red/Green/Blue 各 8 位。</summary>
public readonly struct Mask
{
    public const int BitWidth = 32;
    public const string DefaultBinary = "00001111000011110000111100001111";

    public uint Bits { get; }

    private Mask(uint bits) => Bits = bits;

    public bool IsEmpty => Bits == 0;

    public int PopCount => BitOperations.PopCount(Bits);

    public static Mask Default => Parse(DefaultBinary);

    public static Mask FromBits(uint bits) => new(bits);

    public static Mask Parse(string binary)
    {
        if (binary is null || binary.Length != BitWidth || binary.Any(ch => ch != '0' && ch != '1'))
        {
            throw new StegoException(StegoError.InvalidMask, "掩码必须是 32 位由 0 和 1 组成的二进制字符串。");
        }

        uint bits = 0;
        foreach (char ch in binary)
        {
            bits = (bits << 1) | (ch == '1' ? 1u : 0u);
        }
        return new Mask(bits);
    }

    public string ToBinaryString()
    {
        Span<char> chars = stackalloc char[BitWidth];
        for (int i = 0; i < BitWidth; i++)
        {
            chars[i] = (Bits & (1u << (BitWidth - 1 - i))) != 0 ? '1' : '0';
        }
        return new string(chars);
    }

    public override string ToString() => ToBinaryString();
}
