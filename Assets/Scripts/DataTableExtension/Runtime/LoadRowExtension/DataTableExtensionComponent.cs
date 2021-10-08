using System;
using System.Collections.Generic;
using System.IO;
using GameFramework;
using GameFramework.DataTable;
using GameFramework.FileSystem;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions
{
    public class DataTableExtensionComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 初始化Buffer长度
        /// </summary>
        [SerializeField] private int m_InitBufferLength = 1024 * 64;

        /// <summary>
        /// 图片加载缓存
        /// </summary>
        private byte[] m_Buffer;

        /// <summary>
        /// 初始化Buffer长度
        /// </summary>
        private Dictionary<Type, DataTableRowConfig> m_DataTableRowConfigs;

        private void Start()
        {
            m_DataTableRowConfigs = new Dictionary<Type, DataTableRowConfig>();
            m_Buffer = new byte[m_InitBufferLength];
        }

        public void LoadDataTableRowConfig<T>(string assetName, bool isCacheFileStream = true)
            where T : class, IDataRow, new()
        {
            if (m_DataTableRowConfigs.TryGetValue(typeof(T), out _))
            {
                return;
            }

            string filePath;
            if (!GameEntry.Base.EditorResourceMode)
            {
                bool isSuccess = GameEntry.Resource.GetBinaryPath(assetName,
                    out var isStorageInReadOnly, out var isStorageInFileSystem,
                    out var relativePath, out var filename);
                if (!isSuccess)
                {
                    throw new Exception("DataTable binary asset is not exist.");
                }

                if (isStorageInFileSystem)
                {
                    throw new Exception("DataTable binary asset can not in filesystem.");
                }

                filePath = Utility.Path.GetRegularPath(isStorageInReadOnly
                    ? Path.Combine(GameEntry.Resource.ReadOnlyPath, relativePath)
                    : Path.Combine(GameEntry.Resource.ReadWritePath, relativePath));
            }
            else
            {
                filePath = assetName;
            }

            IFileStream fileStream = FileStreamHelper.CreateFileStream(filePath);

            DataTableRowConfig rowConfig = new DataTableRowConfig
            {
                Path = filePath,
                FileStream = fileStream,
            };

            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.Read(m_Buffer, 0, 32);
            using (MemoryStream memoryStream = new MemoryStream(m_Buffer, 0, 32))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    int count = binaryReader.Read7BitEncodedInt32(out int length);
                    fileStream.Seek(length, SeekOrigin.Begin);
                    EnsureBufferSize(count);
                    long configLength = fileStream.Read(m_Buffer, 0, count);
                    rowConfig.DeSerialize(m_Buffer, 0, (int)configLength, length + count);
                }
            }

            if (!isCacheFileStream)
            {
                fileStream.Dispose();
                rowConfig.FileStream = null;
            }

            m_DataTableRowConfigs.Add(typeof(T), rowConfig);
            GameEntry.DataTable.CreateDataTable<T>();
        }

        public T GetDataRow<T>(int id) where T : class, IDataRow, new()
        {
            m_DataTableRowConfigs.TryGetValue(typeof(T), out var config);
            if (config == null) return default;
            config.DataTableRowSettings.TryGetValue(id, out var value);
            if (value == null) return default;
            IDataTable<T> dataTableBase = GameEntry.DataTable.GetDataTable<T>();
            if (dataTableBase.HasDataRow(id))
            {
                return dataTableBase.GetDataRow(id);
            }

            if (config.FileStream != null)
            {
                AddDataRow(dataTableBase, config.FileStream, value.StartIndex, value.Length);
            }
            else
            {
                using (IFileStream fileStream = FileStreamHelper.CreateFileStream(config.Path))
                {
                    AddDataRow(dataTableBase, fileStream, value.StartIndex, value.Length);
                }
            }

            return dataTableBase.GetDataRow(id);
        }

        private void AddDataRow<T>(IDataTable<T> dataTable, IFileStream fileStream, int startIndex, int length)
            where T : class, IDataRow, new()
        {
            fileStream.Seek(startIndex, SeekOrigin.Begin);
            EnsureBufferSize(length);
            long readLength = fileStream.Read(m_Buffer, 0, length);
            dataTable.AddDataRow(m_Buffer, 0, (int)readLength, null);
        }

        public T[] GetAllDataRows<T>() where T : class, IDataRow, new()
        {
            m_DataTableRowConfigs.TryGetValue(typeof(T), out var config);
            if (config == null) return default;
            IDataTable<T> dataTableBase = GameEntry.DataTable.GetDataTable<T>();

            IFileStream fileStream = config.FileStream ?? FileStreamHelper.CreateFileStream(config.Path);
            using (fileStream)
            {
                foreach (var dataTableSetting in config.DataTableRowSettings)
                {
                    if (dataTableBase.HasDataRow(dataTableSetting.Key))
                    {
                        continue;
                    }

                    AddDataRow(dataTableBase, fileStream, dataTableSetting.Value.StartIndex,
                        dataTableSetting.Value.Length);
                }

                return dataTableBase.GetAllDataRows();
            }
        }

        public bool DestroyDataTable<T>() where T : IDataRow
        {
            IDataTable<T> dataTable = GameEntry.DataTable.GetDataTable<T>();
            if (dataTable == null)
            {
                return true;
            }

            var result = GameEntry.DataTable.DestroyDataTable(dataTable);
            if (result)
            {
                if (m_DataTableRowConfigs.TryGetValue(typeof(T), out var config))
                {
                    config.FileStream.Dispose();
                    config.FileStream = null;
                    m_DataTableRowConfigs.Remove(typeof(T));
                }
            }

            return result;
        }

        /// <summary>
        /// 保证缓存大小
        /// </summary>
        /// <param name="count">读取文件大小</param>
        private void EnsureBufferSize(int count)
        {
            int length = m_Buffer.Length;
            while (length < count)
            {
                length *= 2;
            }

            if (length != m_Buffer.Length)
            {
                m_Buffer = new byte[length];
            }
        }
    }
}