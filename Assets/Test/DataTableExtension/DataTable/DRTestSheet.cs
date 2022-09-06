﻿//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2022-09-06 21:05:03.797
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;


namespace UGFExtensions
{
    /// <summary>
    /// 测试表格生成。
    /// </summary>
    public class DRTestSheet : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取测试枚举。
        /// </summary>
        public UGFExtensions.Test.TestEnum TestEnum
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取测试枚举1。
        /// </summary>
        public UGFExtensions.Test.TestEnum1 TestEnum1
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取测试枚举list。
        /// </summary>
        public List<UGFExtensions.Test.TestEnum> TestEnumList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取测试枚举Array。
        /// </summary>
        public UGFExtensions.Test.TestEnum[] TestEnumArray
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(UGFExtensions.DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
			TestEnum = DataTableExtension.EnumParse<UGFExtensions.Test.TestEnum>(columnStrings[index++]);
			TestEnum1 = DataTableExtension.EnumParse<UGFExtensions.Test.TestEnum1>(columnStrings[index++]);
			TestEnumList = DataTableExtension.ParseUGFExtensionsTestTestEnumList(columnStrings[index++]);
			TestEnumArray = DataTableExtension.ParseUGFExtensionsTestTestEnumArray(columnStrings[index++]);
            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
					TestEnum = (UGFExtensions.Test.TestEnum)binaryReader.Read7BitEncodedInt32();
					TestEnum1 = (UGFExtensions.Test.TestEnum1)binaryReader.Read7BitEncodedInt32();
					TestEnumList = binaryReader.ReadUGFExtensionsTestTestEnumList();
					TestEnumArray = binaryReader.ReadUGFExtensionsTestTestEnumArray();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private KeyValuePair<int, UGFExtensions.Test.TestEnum1>[] m_TestEnum = null;

        public int TestEnumCount
        {
            get
            {
                return m_TestEnum.Length;
            }
        }

        public UGFExtensions.Test.TestEnum1 GetTestEnum(int id)
        {
            foreach (KeyValuePair<int, UGFExtensions.Test.TestEnum1> i in m_TestEnum)
            {
                if (i.Key == id)
                {
                    return i.Value;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("GetTestEnum with invalid id '{0}'.", id.ToString()));
        }

        public UGFExtensions.Test.TestEnum1 GetTestEnumAt(int index)
        {
            if (index < 0 || index >= m_TestEnum.Length)
            {
                throw new GameFrameworkException(Utility.Text.Format("GetTestEnumAt with invalid index '{0}'.", index.ToString()));
            }

            return m_TestEnum[index].Value;
        }

        private void GeneratePropertyArray()
        {
            m_TestEnum = new KeyValuePair<int, UGFExtensions.Test.TestEnum1>[]
            {
                new KeyValuePair<int, UGFExtensions.Test.TestEnum1>(1, TestEnum1),
            };
        }
    }
}
