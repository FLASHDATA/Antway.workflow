﻿using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace OptimaJet.Workflow.Core
{
    public static class HashHelper
    {
        public static string GenerateStringHash(string stringForHashing)
        {
#if NETCOREAPP
            return GenerateStringHash(stringForHashing, string.Empty, MD5.Create());
#else
            return GenerateStringHash(stringForHashing, string.Empty, HashAlgorithm.Create("MD5"));
#endif
        }

        public static string GenerateStringHash(string stringForHashing, string salt, HashAlgorithm hashAlgorithm)
        {
            return Convert.ToBase64String(GenerateBinaryHash(stringForHashing, salt, hashAlgorithm));
        }

        public static byte[] GenerateBinaryHash(string stringForHashing, string salt, HashAlgorithm hashAlgorithm)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(stringForHashing);
            byte[] src = Convert.FromBase64String(salt);
            byte[] inArray;
            if (hashAlgorithm is KeyedHashAlgorithm)
            {
                var keyedHashAlgorithm = (KeyedHashAlgorithm)hashAlgorithm;
                if (keyedHashAlgorithm.Key.Length == src.Length)
                {
                    keyedHashAlgorithm.Key = src;
                }
                else if (keyedHashAlgorithm.Key.Length < src.Length)
                {
                    var dst = new byte[keyedHashAlgorithm.Key.Length];
                    Buffer.BlockCopy(src, 0, dst, 0, dst.Length);
                    keyedHashAlgorithm.Key = dst;
                }
                else
                {
                    int count;
                    var buffer = new byte[keyedHashAlgorithm.Key.Length];
                    for (int i = 0; i < buffer.Length; i += count)
                    {
                        count = Math.Min(src.Length, buffer.Length - i);
                        Buffer.BlockCopy(src, 0, buffer, i, count);
                    }
                    keyedHashAlgorithm.Key = buffer;
                }

                inArray = keyedHashAlgorithm.ComputeHash(bytes);
            }
            else
            {
                var buffer = new byte[src.Length + bytes.Length];
                Buffer.BlockCopy(src, 0, buffer, 0, src.Length);
                Buffer.BlockCopy(bytes, 0, buffer, src.Length, bytes.Length);
                inArray = hashAlgorithm.ComputeHash(buffer);
            }

            return inArray;
        }
    }
}