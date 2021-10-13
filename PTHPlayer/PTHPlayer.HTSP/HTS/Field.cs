using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTHPlayer.HTSP.HTS
{
    /*
 ||Type||1 byte integer||Type of field (see field type IDs above)
 ||Namelength||1 byte integer||Length of name of field. If a field is part of a list message this must be 0
 ||Datalength||4 byte integer||Length of field data
 ||Name||N bytes||Field name, length as specified by Namelength
 ||Data||N bytes||Field payload, for details see below
 */
    public class Field
    {
        public FieldTypes Type { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }

        public byte[] GenerateBinary()
        {
            byte[] TypeBytes = BitConverter.GetBytes((int)Type);
            TypeBytes = new byte[] { TypeBytes[0] };


            byte[] NameBytes = Encoding.UTF8.GetBytes(Name);
            byte[] NameLenght = BitConverter.GetBytes(NameBytes.Length);
            NameLenght = new byte[] { NameLenght[0] };

            byte[] DataBytes = new byte[0];
            byte[] DataLength = new byte[4];

            if (Type == FieldTypes.HMF_S64)
            {
                string hexValue = (23).ToString("x");
                if(hexValue.Length % 1 == 0)
                {
                    hexValue = "0"+ hexValue;
                }
                var listString = Split(hexValue, 2);

                DataBytes = new byte[listString.Count()];
                for(int i = 0; i < DataBytes.Length; i++)
                {
                   //var setterIndex = DataBytes.Length - i - 1;
                    DataBytes[i] = Convert.ToByte(listString.ToArray()[i]);
                }
                DataLength = BitConverter.GetBytes(listString.Count());
            }
            else
            {
                DataBytes = Encoding.UTF8.GetBytes(Data);
                DataLength = BitConverter.GetBytes(Data.Length);                
            }
            return CombineField(TypeBytes, NameLenght, DataLength, NameBytes, DataBytes);
        }

        static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        public static byte[] CombineField(byte[] TypeBytes, byte[] NameLenghtByte, byte[] DataLeghtBytes, byte[] NameBytes, byte[] DataBytes)
        {
            byte[] bytes = new byte[TypeBytes.Length + NameLenghtByte.Length + DataLeghtBytes.Length + NameBytes.Length + DataBytes.Length];
            Buffer.BlockCopy(TypeBytes, 0, bytes, 0, TypeBytes.Length);
            Buffer.BlockCopy(NameLenghtByte, 0, bytes, TypeBytes.Length, NameLenghtByte.Length);
            Buffer.BlockCopy(DataLeghtBytes, 0, bytes, TypeBytes.Length + NameLenghtByte.Length, DataLeghtBytes.Length);
            Buffer.BlockCopy(NameBytes, 0, bytes, TypeBytes.Length + NameLenghtByte.Length + DataLeghtBytes.Length, NameBytes.Length);
            Buffer.BlockCopy(DataBytes, 0, bytes, TypeBytes.Length + NameLenghtByte.Length + DataLeghtBytes.Length + NameBytes.Length, DataBytes.Length);
            return bytes;
        }
    }
}
