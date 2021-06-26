using System;
using System.Collections.Generic;

namespace Hashing {
    class HashFuncs {
        public delegate string HashFunction(string str);//tex makes testing different hash types a bit easier by outputing the hash as a string.

        //tex string hash function name to HashFunc
        //add to this if you add new HashFuncs
        //hash function name should be lowercase
        private static Dictionary<string, HashFunction> hashFuncs = new Dictionary<string, HashFunction> {
            {"strcode32", StrCode32Str},
            {"strcode64", StrCode64Str},
            {"strcode32hex", StrCode32HexStr},
            {"strcode64hex", StrCode64HexStr},
            {"pathfilenamecode32", PathFileNameCode32Str},
            {"pathfilenamecode64", PathFileNameCode64Str},//tex for want of a better name, to match PathFileNameCode32 which named from lua function (and outputs decimal to match), QuickHash calls it PathCode32/64 with Extension (as hex)
            {"pathfilenamecode32hex", PathFileNameCode32HexStr},//tex KLUDGE, should specify or detect whether hash representation is hex or decimal
            {"pathfilenamecode64hex", PathFileNameCode64HexStr},
            {"pathcode64", PathCode64Str},//tex HashFileName as hex hashes (used by GzsTool as qar_dictionary)
            {"pathcode64gz", PathCode64GzStr},//tex equivalent to StrCode Hex.
            {"extensioncode64", ExtensionCode64Str },
            {"md5hashtext", Md5HashTextStr },//tex fpk entry.md5hash for 'encrypted hashes'/ filepath
            //tex used in wwise 
            {"fnv1hash32", FNV1Hash32Str },
            {"fnv1hash64", FNV1Hash64Str },
            {"fnv1ahash32", FNV1aHash32Str },
            {"fnv1ahash64", FNV1aHash64Str },
            {"unknown32", Unknown32Str },//tex for unknown hash types, just to put it in a category so tools can chew through stuff without having to exclude, should always match false
            {"unknown64", Unknown64Str },//tex 
        };

        public static HashFunction GetHashFuncByName(string funcName) {
            HashFunction hashFunc = null;
            try {
                hashFunc = hashFuncs[funcName.ToLower()];
            } catch (KeyNotFoundException) {
                hashFunc = null;
            }

            return hashFunc;
        }//GetHashFuncByName

        //Hashfuncs
        public static string StrCode32Str(string text) {
            var hash = (uint)Hashing.FoxEngine.StrCode(text);
            return hash.ToString();
        }
        public static string StrCode32HexStr(string text)
        {
            var hash = (uint)Hashing.FoxEngine.StrCode(text);
            return hash.ToString("x");
        }
        public static string StrCode64Str(string text) {
            var hash = Hashing.FoxEngine.StrCode(text);
            return hash.ToString();
        }
        public static string StrCode64HexStr(string text)
        {
            var hash = Hashing.FoxEngine.StrCode(text);
            return hash.ToString("x");
        }
        //TODO: verify output matches lua PathFileNameCode32 (it was missing in some cases? see mockfox pathfilename note?)
        public static string PathFileNameCode32Str(string text) {
            var hash = (uint)Hashing.FoxEngine.HashFileNameWithExtension(text);
            return hash.ToString();
        }
        public static string PathFileNameCode32HexStr(string text)
        {
            var hash = (uint)Hashing.FoxEngine.HashFileNameWithExtension(text);
            return hash.ToString("x");
        }
        /// <summary>
        /// for want of a better name, to match PathFileNameCode32 which named from lua function
        /// </summary>
        public static string PathFileNameCode64Str(string text) {
            ulong hash = Hashing.FoxEngine.HashFileNameWithExtension(text);
            return hash.ToString();
        }
        public static string PathFileNameCode64HexStr(string text)
        {
            ulong hash = Hashing.FoxEngine.HashFileNameWithExtension(text);
            return hash.ToString("x");
        }
        //tex DEBUGNOW TODO name, this is more specific to gzstool dictionary implementation than a general Fox implementation?
        /// <summary>
        /// HashFileName as hex hashes (used by GzsTool as qar_dictionary)
        /// </summary>
        public static string PathCode64Str(string text) {
            ulong hash = Hashing.FoxEngine.HashFileName(text) & 0x3FFFFFFFFFFFF;
            return hash.ToString("x");
        }

        /// <summary>
        /// equivalent to StrCode Hex.
        /// </summary>
        public static string PathCode64GzStr(string text)
        {
            ulong hash = Hashing.FoxEngine.StrCode(text, false);//GOTCHA: removeExtension, see GOTCHA in Program.TestStrings
            return hash.ToString("x");
        }

        public static string ExtensionCode64Str(string text) {
            ulong hash = Hashing.FoxEngine.HashFileExtension(text);
            return hash.ToString();
        }

        public static string Md5HashTextStr(string text)
        {
            byte[] hash = Md5HashText(text);

            string hashStr = BitConverter.ToString(hash);//tex converts to hex pairs seperated by -
            hashStr = hashStr.Replace("-", "");//tex remove seperators
            return hashStr;
        }

        //tex fnvhash from https://gist.github.com/RobThree/25d764ea6d4849fdd0c79d15cda27d61 check.cs
        public static string FNV1Hash32Str(string text)
        {
            var fnvHash = new FNV1Hash32();
            var value = fnvHash.ComputeHash(Encoding.UTF8.GetBytes(text));//DEBUGNOW encoding? -v-
            var hash = fnvHash.HashSize == 32 ? BitConverter.ToUInt32(value, 0) : BitConverter.ToUInt64(value, 0);
            return hash.ToString();
        }//FNV1Hash32Str
        public static string FNV1Hash64Str(string text)
        {
            var fnvHash = new FNV1Hash64();
            var value = fnvHash.ComputeHash(Encoding.UTF8.GetBytes(text));
            var hash = fnvHash.HashSize == 32 ? BitConverter.ToUInt32(value, 0) : BitConverter.ToUInt64(value, 0);
            return hash.ToString();
        }//FNV1Hash64Str
        public static string FNV1aHash32Str(string text)
        {
            var fnvHash = new FNV1aHash32();
            var value = fnvHash.ComputeHash(Encoding.UTF8.GetBytes(text));
            var hash = fnvHash.HashSize == 32 ? BitConverter.ToUInt32(value, 0) : BitConverter.ToUInt64(value, 0);
            return hash.ToString();
        }//FNV1Hash32Str
        public static string FNV1aHash64Str(string text)
        {
            var fnvHash = new FNV1aHash64();
            var value = fnvHash.ComputeHash(Encoding.UTF8.GetBytes(text));
            var hash = fnvHash.HashSize == 32 ? BitConverter.ToUInt32(value, 0) : BitConverter.ToUInt64(value, 0);
            return hash.ToString();
        }//FNV1Hash64Str

        public static string Unknown32Str(string text)
        {
            return "";
        }

        public static string Unknown64Str(string text)
        {
            return "";
        }
    }//HashFuncs
}//BruteGen
