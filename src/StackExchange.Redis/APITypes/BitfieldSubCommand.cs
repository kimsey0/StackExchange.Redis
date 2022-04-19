﻿using System;
using System.Collections.Generic;

namespace StackExchange.Redis;

/// <summary>
/// An abstract subcommand for a bitfield.
/// </summary>
public abstract class BitfieldSubCommand
{
    internal abstract int NumArgs { get; }

    internal abstract void AddArgs(IList<RedisValue> args);

    internal virtual bool IsReadonly => false;

    /// <summary>
    /// The encoding of the sub-command. A signed or unsigned integer of a given size.
    /// </summary>
    public BitfieldEncoding Encoding { get; }

    /// <summary>
    /// The offset into the bitfield the subcommand will traverse.
    /// </summary>
    public BitfieldOffset Offset { get; }

    internal BitfieldSubCommand(BitfieldEncoding encoding, BitfieldOffset offset)
    {
        Encoding = encoding;
        Offset = offset;
    }

}

/// <summary>
/// Represents a Bitfield GET, which returns the number stored in the specified offset of a bitfield at the given encoding.
/// </summary>
public sealed class BitfieldGet : BitfieldSubCommand
{
    /// <summary>
    /// Initializes a bitfield GET subcommand
    /// </summary>
    /// <param name="encoding">The encoding of the subcommand.</param>
    /// <param name="offset">The offset into the bitfield of the subcommand</param>
    public BitfieldGet(BitfieldEncoding encoding, BitfieldOffset offset) : base(encoding, offset)
    {
    }

    internal override bool IsReadonly => true;

    internal override int NumArgs => 3;

    internal override void AddArgs(IList<RedisValue> args)
    {
        args.Add(RedisLiterals.GET);
        args.Add(Encoding.AsRedisValue);
        args.Add(Offset.AsRedisValue);
    }
}

/// <summary>
/// Bitfield subcommand which SETs the specified range of bits to the specified value.
/// </summary>
public sealed class BitfieldSet : BitfieldSubCommand
{
    /// <summary>
    /// The value to set.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// Initializes a subcommand for a Bitfield SET.
    /// </summary>
    /// <param name="encoding">The number's encoding.</param>
    /// <param name="offset">The offset into the bitfield to set.</param>
    /// <param name="value">The value to set.</param>
    public BitfieldSet(BitfieldEncoding encoding, BitfieldOffset offset, long value) : base(encoding, offset)
    {
        Value = value;
    }

    internal override int NumArgs => 4;

    internal override void AddArgs(IList<RedisValue> args)
    {
        args.Add(RedisLiterals.SET);
        args.Add(Encoding.AsRedisValue);
        args.Add(Offset.AsRedisValue);
        args.Add(Value);
    }
}

/// <summary>
/// Bitfield subcommand INCRBY, which increments the number at the specified range of bits by the provided value
/// </summary>
public sealed class BitfieldIncrby : BitfieldSubCommand
{
    /// <summary>
    /// The value to increment by.
    /// </summary>
    public long Increment { get; }

    /// <summary>
    /// Determines how overflows are handled for the bitfield.
    /// </summary>
    public BitfieldOverflowHandling OverflowHandling { get; }

    /// <summary>
    /// Initializes a sub-command for a Bitfield INCRBY.
    /// </summary>
    /// <param name="encoding">The number's encoding.</param>
    /// <param name="offset">The offset into the bitfield to set.</param>
    /// <param name="increment">The value to set.</param>
    /// <param name="overflowHandling">How overflows will be handled when incrementing.</param>
    public BitfieldIncrby(BitfieldEncoding encoding, BitfieldOffset offset, long increment, BitfieldOverflowHandling overflowHandling = BitfieldOverflowHandling.Wrap) : base(encoding, offset)
    {
        Increment = increment;
        OverflowHandling = overflowHandling;
    }

    internal override int NumArgs => OverflowHandling == BitfieldOverflowHandling.Wrap ? 4 : 6;

    internal override void AddArgs(IList<RedisValue> args)
    {
        if (OverflowHandling != BitfieldOverflowHandling.Wrap)
        {
            args.Add(RedisLiterals.OVERFLOW);
            args.Add(OverflowHandling.AsRedisValue());
        }
        args.Add(RedisLiterals.INCRBY);
        args.Add(Encoding.AsRedisValue);
        args.Add(Offset.AsRedisValue);
        args.Add(Increment);
    }
}



/// <summary>
/// An offset into a bitfield. This is either a literal offset (number of bits from the beginning of the bitfield) or an
/// encoding based offset, based off the encoding of the sub-command.
/// </summary>
public readonly struct BitfieldOffset
{
    /// <summary>
    /// Returns the BitfieldOffset as a RedisValue
    /// </summary>
    internal RedisValue AsRedisValue => $"{(ByEncoding ? "#" : string.Empty)}{Offset}";

    /// <summary>
    /// Whether or not the BitfieldOffset will work off of the sub-commands integer encoding.
    /// </summary>
    public bool ByEncoding { get; }

    /// <summary>
    /// The number of either bits or encoded integers to offset into the bitfield.
    /// </summary>
    public long Offset { get; }

    /// <summary>
    /// Initializes a bitfield offset
    /// </summary>
    /// <param name="byEncoding">Whether or not the BitfieldOffset will work off of the sub-commands integer encoding.</param>
    /// <param name="offset">The number of either bits or encoded integers to offset into the bitfield.</param>
    public BitfieldOffset(bool byEncoding, long offset)
    {
        ByEncoding = byEncoding;
        Offset = offset;
    }
}

/// <summary>
/// The encoding that a sub-command should use. This is either a signed or unsigned integer of a specified length.
/// </summary>
public readonly struct BitfieldEncoding
{
    internal RedisValue AsRedisValue => $"{Signedness.SignChar()}{Size}";

    /// <summary>
    /// The signedness of the integer.
    /// </summary>
    public Signedness Signedness { get; }

    /// <summary>
    /// The size of the integer.
    /// </summary>
    public byte Size { get; }

    /// <summary>
    /// Initializes the BitfieldEncoding.
    /// </summary>
    /// <param name="signedness">The encoding's <see cref="Signedness"/></param>
    /// <param name="size">The size of the integer.</param>
    public BitfieldEncoding(Signedness signedness, byte size)
    {
        Signedness = signedness;
        Size = size;
    }
}