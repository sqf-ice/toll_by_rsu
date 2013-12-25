using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu
{
    /// <summary>
    /// memo:
    ///     tools
    /// 
    /// author: name(ViaHere), fan reserved ,from 2006
    /// </summary>
    public class ViaHere
    {
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            s = s.Replace("\t", "");
            s = s.Replace("-", "");

            byte[] buffer = new byte[s.Length / 2];

            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);

            return buffer;
        }

        public static string ByteArraryToHexString(byte[] data)
        {
            if (data == null)
            {
                return "";
            }
            if (data.Length == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                //sb.Append(Convert.ToString(b,16).PadLeft(2,'0').PadRight(3,' '));
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString().ToUpper();
        }

        #region DES


        /// <summary>
        /// DES加密模式
        /// 
        /// author:liujing 2012 qiuchuanpiaoyizhang
        /// </summary>
        public enum DESType
        {
            /// <summary>
            /// 加密
            /// </summary>
            Encrypt,
            /// <summary>
            /// 解密
            /// </summary>
            Decrypt
        }

        /// <summary>
        /// DES计算
        /// </summary>
        /// <param name="dt">计算模式：加密或解密</param>
        /// <param name="singleDESKey">计算密钥，8字节</param>
        /// <param name="sourDataLen">源数据长度</param>
        /// <param name="sourData">源数据</param>
        /// <param name="destDataLen">结果数据长度</param>
        /// <param name="destData">结果数据</param>
        /// <returns>是否成功</returns>
        public static bool SingleDES(DESType dt,
                             byte[] singleDESKey,
                             int sourDataLen,
                             byte[] sourData,
                             ref int destDataLen,
                             ref byte[] destData)
        {
            try
            {
                byte[] temp = null;
                byte[] IV = new byte[8];
                for (int i = 0; i < IV.Length; i++)
                {
                    IV[i] = 0;
                }
                if (singleDESKey == null)
                {
                    throw new Exception("DES密钥为空");
                }
                if (singleDESKey.Length != 8)
                {
                    throw new Exception("DES密钥长度不正确，当前长度为：" + singleDESKey.Length);
                }
                if (sourData == null)
                {
                    throw new Exception("DES数据为空");
                }
                if (sourData.Length < sourDataLen)
                {
                    throw new Exception("DES待计算数据长度不正确，当前长度为：" + sourData.Length);
                }
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                dCSP.Padding = PaddingMode.None;
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = null;
                switch (dt)
                {
                    case DESType.Encrypt:
                        cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(singleDESKey, IV), CryptoStreamMode.Write);
                        break;
                    case DESType.Decrypt:
                        cStream = new CryptoStream(mStream, dCSP.CreateDecryptor(singleDESKey, IV), CryptoStreamMode.Write);
                        break;
                    default:
                        throw new Exception("加密模式不存在");
                }
                mStream.Flush();
                cStream.Write(sourData, 0, (int)sourDataLen);
                cStream.FlushFinalBlock();
                temp = mStream.ToArray();
                if (temp == null)
                {
                    throw new Exception("DES计算结果为空");
                }
                destDataLen = temp.Length;
                if (destData != null && destData.Length >= temp.Length)
                {
                    Array.ConstrainedCopy(temp, 0, destData, 0, temp.Length);
                }
                else
                {
                    destData = temp;
                }
                return true;
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// 3DES计算
        /// </summary>
        /// <param name="dt">计算模式：加密或解密</param>
        /// <param name="tripleDESKey">计算密钥，16字节</param>
        /// <param name="sourDataLen">源数据长度</param>
        /// <param name="sourData">源数据</param>
        /// <param name="destDataLen">结果数据长度</param>
        /// <param name="destData">结果数据</param>
        /// <returns>是否成功</returns>
        public static bool TripleDES(DESType dt,
                                     byte[] tripleDESKey,
                                     int sourDataLen,
                                     byte[] sourData,
                                     ref int destDataLen,
                                     ref byte[] destData)
        {
            try
            {
                byte[] temp = null;
                byte[] IV = new byte[8];
                for (int i = 0; i < IV.Length; i++)
                {
                    IV[i] = 0;
                }

                if (tripleDESKey == null)
                {
                    throw new Exception("3DES密钥为空");
                }
                if (tripleDESKey.Length != 16)
                {
                    throw new Exception("3DES密钥长度不正确，当前长度为：" + tripleDESKey.Length);
                }
                if (sourData == null)
                {
                    throw new Exception("3DES数据为空");
                }
                if (sourData.Length < sourDataLen)
                {
                    throw new Exception("3DES待计算数据长度不正确，当前长度为：" + sourData.Length);
                }
                TripleDESCryptoServiceProvider tdCSP = new TripleDESCryptoServiceProvider();
                tdCSP.Padding = PaddingMode.None;
                tdCSP.Mode = CipherMode.ECB;    //ETC行业的MODE与.net内置的默认不一致，这里调整。fanshiming
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = null;
                switch (dt)
                {
                    case DESType.Encrypt:
                        cStream = new CryptoStream(mStream, tdCSP.CreateEncryptor(tripleDESKey, IV), CryptoStreamMode.Write);
                        break;
                    case DESType.Decrypt:
                        cStream = new CryptoStream(mStream, tdCSP.CreateDecryptor(tripleDESKey, IV), CryptoStreamMode.Write);
                        break;
                    default:
                        throw new Exception("加密模式不存在");
                }
                mStream.Flush();
                cStream.Write(sourData, 0, (int)sourDataLen);
                cStream.FlushFinalBlock();
                temp = mStream.ToArray();
                if (temp == null)
                {
                    throw new Exception("3DES计算结果为空");
                }
                destDataLen = temp.Length;
                if (destData != null && destData.Length >= temp.Length)
                {
                    Array.ConstrainedCopy(temp, 0, destData, 0, temp.Length);
                }
                else
                {
                    destData = temp;
                }
                return true;
            }
            catch
            {
            }
            return false;
        }

        #endregion


        #region tac modify LIUJING,fanshiming

        //测试主密钥  一版
        //private static readonly byte[] mtk = ViaHere.HexStringToByteArray("9FC68A4FA69A7456D8DA9EA4E251805C");

        /// <summary>
        /// 计算TAC
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="tacDataLen">TAC数据长度</param>
        /// <param name="tacData">TAC数据</param>
        /// <param name="tac">TAC</param>
        /// <returns>是否成功</returns>
        public static bool TheTAC(
                                byte[] mtk,
                                byte[] divFactor,
                               int tacDataLen,
                               byte[] tacData,
                               ref byte[] tac)
        {
            try
            {
                if (mtk == null)
                {
                    throw new Exception("密钥为空");
                }
                if (mtk.Length != 16)
                {
                    throw new Exception("密钥长度不正确，当前长度为：" + mtk.Length);
                }
                if (tacData == null)
                {
                    throw new Exception("TAC计算数据为空");
                }
                if (tacData.Length < tacDataLen)
                {
                    throw new Exception("TAC计算数据长度不正确，当前长度为：" + tacData.Length);
                }


                #region key 分散后的TAC密钥

                byte[] key = new byte[16];  //分散后的TAC密钥

                //分散密钥
                byte[] dtk = new byte[16];
                byte[] lDk = new byte[8];
                byte[] rDk = new byte[8];
                //uint lLen = TripleDES((byte)1, mk, 8, divFactor, lDk);
                int lLen = 0;
                TripleDES(DESType.Encrypt, mtk, 8, divFactor, ref lLen, ref lDk);

                byte[] tfSrc = new byte[8];
                uint x = 0;
                for (int ii = 0; ii < 8; ii++)
                {
                    x = (uint)divFactor[ii];
                    tfSrc[ii] = (byte)(~x);
                }

                int rLen = 0;
                TripleDES(DESType.Encrypt, mtk, 8, tfSrc, ref rLen, ref rDk);

                for (int ii = 0; ii < 8; ii++)
                {
                    dtk[ii] = lDk[ii];
                    dtk[ii + 8] = rDk[ii];
                }

                key = dtk;

                #endregion

                #region TAC计算

                byte[] TempData = new byte[8];
                byte[] InitialVector = new byte[8];
                byte[] TempVector = new byte[8];
                byte[] TempKey = new byte[8];
                int i = 0;
                int j = 0;
                int len = 0;

                //根据左右异或运算结果获得可用子密钥
                for (i = 0; i < 8; i++)
                {
                    TempKey[i] = key[i];
                    TempKey[i] ^= key[i + 8];
                }

                //生成TAC
                for (i = 0; i < 8; i++)
                {
                    InitialVector[i] = 0x00;
                }
                //8字节整块数
                int a = tacDataLen / 8;
                //8字节整块分割剩余数
                int b = tacDataLen % 8;
                for (i = 0; i < a; i++)
                {
                    for (j = 0; j < 8; j++)
                    {
                        TempVector[j] = tacData[i * 8 + j];
                        InitialVector[j] ^= TempVector[j];
                    }
                    SingleDES(DESType.Encrypt, TempKey, 8, InitialVector, ref len, ref TempData);
                    if (len != 8)
                    {
                        throw new Exception("TAC生成失败");
                    }
                    for (j = 0; j < 8; j++)
                        InitialVector[j] = TempData[j];
                }
                //块补全
                for (j = 0; j < 8; j++)
                {
                    if (j < b)
                    {
                        TempVector[j] = tacData[a * 8 + j];
                    }
                    else if (j == b)
                    {
                        TempVector[j] = 0x80;
                    }
                    else
                    {
                        TempVector[j] = 0x00;
                    }
                    InitialVector[j] ^= TempVector[j];
                }
                SingleDES(DESType.Encrypt, TempKey, 8, InitialVector, ref len, ref TempData);
                if (len != 8)
                {
                    throw new Exception("TAC生成失败");
                }
                if (tac == null || tac.Length < 4)
                {
                    tac = new byte[4];
                }
                Array.ConstrainedCopy(TempData, 0, tac, 0, 4);

                return true;

                #endregion
            }
            catch
            {
            }


            return false;
        }


        #endregion

        #region 秘钥分散  一级
        /// <summary>
        /// 秘钥分散 16bytes
        /// </summary>
        /// <param name="key_src">主密钥</param>
        /// <param name="divFactor">分散因子</param>
        /// <param name="key">分散后秘钥</param>
        public static void KeyDiv(
            byte[] key_src,
            byte[] divFactor,
            out byte[] key
            )
        {
            if (key_src == null || divFactor == null) throw new ArgumentNullException();
            if (key_src.Length != 16 || divFactor.Length != 8)
                throw new Exception("参数长度异常");

            byte[] mtk = key_src;

            key = new byte[16];  //分散后的TAC密钥

            //分散密钥
            byte[] dtk = new byte[16];
            byte[] lDk = new byte[8];
            byte[] rDk = new byte[8];
            //uint lLen = TripleDES((byte)1, mk, 8, divFactor, lDk);
            int lLen = 0;
            TripleDES(DESType.Encrypt, mtk, 8, divFactor, ref lLen, ref lDk);

            byte[] tfSrc = new byte[8];
            uint x = 0;
            for (int ii = 0; ii < 8; ii++)
            {
                x = (uint)divFactor[ii];
                tfSrc[ii] = (byte)(~x);
            }

            int rLen = 0;
            TripleDES(DESType.Encrypt, mtk, 8, tfSrc, ref rLen, ref rDk);

            for (int ii = 0; ii < 8; ii++)
            {
                dtk[ii] = lDk[ii];
                dtk[ii + 8] = rDk[ii];
            }

            key = dtk;
        }

        #endregion


        #region BCC

        public static byte BccCalc(byte[] src, int offset, int count)
        {
            byte checkSum = 0;
            for (int i = offset; i < offset + count; i++)
            {
                checkSum ^= src[i];
            }

            return checkSum;
        }

        public static byte ConverterHL(byte b)
        {
            return (byte)(((b & 0x0F) << 4) + ((b & 0xF0) >> 4));
        }

        #endregion

        /// <summary>
        /// 适用于从XML等文件中读取并处理为byte数组。根据min(dst,s)长度决定读取最小长度的数据。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="dst"></param>
        public static void HexStringToByteArray_MinLength(string s, byte[] dst)
        {
            s = s.Replace(" ", "");
            s = s.Replace("\t", "");
            s = s.Replace("-", "");

            int len = dst.Length; ;
            if (s.Length < len * 2)
            {
                len = s.Length / 2;
            }

            for (int i = 0; i < len; i++)
                dst[i] = (byte)Convert.ToByte(s.Substring(i * 2, 2), 16);
        }

    }
}
