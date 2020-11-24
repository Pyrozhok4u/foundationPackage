#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1
{
    public class DerInteger
        : Asn1Object
    {
        public const string AllowUnsafeProperty = "BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.AllowUnsafeInteger";

        internal static bool AllowUnsafe()
        {
            string allowUnsafeValue = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.GetEnvironmentVariable(AllowUnsafeProperty);
            return allowUnsafeValue != null && BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.EqualsIgnoreCase("true", allowUnsafeValue);
        }

        internal const int SignExtSigned = -1;
        internal const int SignExtUnsigned = 0xFF;

        private readonly byte[] bytes;
        private readonly int start;

        /**
         * return an integer from the passed in object
         *
         * @exception ArgumentException if the object cannot be converted.
         */
        public static DerInteger GetInstance(
            object obj)
        {
            if (obj == null || obj is DerInteger)
            {
                return (DerInteger)obj;
            }

            throw new ArgumentException("illegal object in GetInstance: " + BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.GetTypeName(obj));
        }

        /**
         * return an Integer from a tagged object.
         *
         * @param obj the tagged object holding the object we want
         * @param isExplicit true if the object is meant to be explicitly
         *              tagged false otherwise.
         * @exception ArgumentException if the tagged object cannot
         *               be converted.
         */
        public static DerInteger GetInstance(
            Asn1TaggedObject	obj,
            bool				isExplicit)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

			Asn1Object o = obj.GetObject();

			if (isExplicit || o is DerInteger)
			{
				return GetInstance(o);
			}

			return new DerInteger(Asn1OctetString.GetInstance(o).GetOctets());
        }

		public DerInteger(int value)
        {
            this.bytes = BigInteger.ValueOf(value).ToByteArray();
            this.start = 0;
        }

        public DerInteger(long value)
        {
            this.bytes = BigInteger.ValueOf(value).ToByteArray();
            this.start = 0;
        }

		public DerInteger(BigInteger value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

			this.bytes = value.ToByteArray();
            this.start = 0;
        }

        public DerInteger(byte[] bytes)
            : this(bytes, true)
        {
        }

        internal DerInteger(byte[] bytes, bool clone)
        {
            if (IsMalformed(bytes))
                throw new ArgumentException("malformed integer", "bytes");

            this.bytes = clone ? Arrays.Clone(bytes) : bytes;
            this.start = SignBytesToSkip(bytes);
        }

        /**
         * in some cases positive values Get crammed into a space,
         * that's not quite big enough...
         */
        public BigInteger PositiveValue
        {
            get { return new BigInteger(1, bytes); }
        }

        public BigInteger Value
        {
            get { return new BigInteger(bytes); }
        }

        public bool HasValue(BigInteger x)
        {
            return null != x
                // Fast check to avoid allocation
                && IntValue(bytes, start, SignExtSigned) == x.IntValue
                && Value.Equals(x);
        }

        public int IntPositiveValueExact
        {
            get
            {
                int count = bytes.Length - start;
                if (count > 4 || (count == 4 && 0 != (bytes[start] & 0x80)))
                    throw new ArithmeticException("ASN.1 Integer out of positive int range");

                return IntValue(bytes, start, SignExtUnsigned);
            }
        }

        public int IntValueExact
        {
            get
            {
                int count = bytes.Length - start;
                if (count > 4)
                    throw new ArithmeticException("ASN.1 Integer out of int range");

                return IntValue(bytes, start, SignExtSigned);
            }
        }

        internal override void Encode(DerOutputStream derOut)
        {
            derOut.WriteEncoded(Asn1Tags.Integer, bytes);
        }

		protected override int Asn1GetHashCode()
		{
			return Arrays.GetHashCode(bytes);
        }

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerInteger other = asn1Object as DerInteger;
			if (other == null)
				return false;

            return Arrays.AreEqual(this.bytes, other.bytes);
        }

		public override string ToString()
		{
			return Value.ToString();
		}

        internal static int IntValue(byte[] bytes, int start, int signExt)
        {
            int length = bytes.Length;
            int pos = System.Math.Max(start, length - 4);

            int val = (sbyte)bytes[pos] & signExt;
            while (++pos < length)
            {
                val = (val << 8) | bytes[pos];
            }
            return val;
        }

        /**
         * Apply the correct validation for an INTEGER primitive following the BER rules.
         *
         * @param bytes The raw encoding of the integer.
         * @return true if the (in)put fails this validation.
         */
        internal static bool IsMalformed(byte[] bytes)
        {
            switch (bytes.Length)
            {
            case 0:
                return true;
            case 1:
                return false;
            default:
                return (sbyte)bytes[0] == ((sbyte)bytes[1] >> 7) && !AllowUnsafe();
            }
        }

        internal static int SignBytesToSkip(byte[] bytes)
        {
            int pos = 0, last = bytes.Length - 1;
            while (pos < last
                && (sbyte)bytes[pos] == ((sbyte)bytes[pos + 1] >> 7))
            {
                ++pos;
            }
            return pos;
        }
    }
}
#pragma warning restore
#endif