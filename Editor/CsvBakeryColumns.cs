using System.Collections.Generic;
using CsvHelper;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Bakery.Translate
{
    public class CsvBakeryColumns : CsvColumns
    {
        [SerializeField]
        private LocaleIdentifier _localeIdentifier;
        public LocaleIdentifier LocaleIdentifier
        {
            get => _localeIdentifier;
            set => _localeIdentifier = value;
        }

        int m_ScopeIndex,
            m_ContextIndex,
            m_MaxLengthIndex,
            m_CommentToTranslatorIndex,
            m_CollectionTableIndex,
            m_NeedsReviewIndex;
        StringTable m_ImportTable;

        private const string ScopeFieldName = "Scope";
        private const string ContextFieldName = "Context";
        private const string MaxLengthFieldName = "Max Length";
        private const string CommentToTranslatorFieldName = "Comment To Translator";

        private const string NeedsReview = "Needs Review";

        public override void ReadBegin(StringTableCollection collection, CsvReader reader)
        {
            m_ImportTable = collection.GetTable(_localeIdentifier) as StringTable;
            if (m_ImportTable != null)
            {
                m_ScopeIndex = reader.GetFieldIndex(ScopeFieldName, isTryGet: true);
                m_ContextIndex = reader.GetFieldIndex(ContextFieldName, isTryGet: true);
                m_MaxLengthIndex = reader.GetFieldIndex(MaxLengthFieldName, isTryGet: true);
                m_CommentToTranslatorIndex = reader.GetFieldIndex(
                    CommentToTranslatorFieldName,
                    isTryGet: true
                );
                m_NeedsReviewIndex = reader.GetFieldIndex(NeedsReview, isTryGet: true);
            }
        }

        public override void ReadRow(SharedTableData.SharedTableEntry keyEntry, CsvReader reader)
        {
            if (m_ImportTable == null)
                return;

            // Get the entry or create one
            StringTableEntry entry =
                m_ImportTable.GetEntry(keyEntry.Id)
                ?? m_ImportTable.AddEntry(keyEntry.Id, string.Empty);

            // Get the metadata or add one
            var metadata = entry.GetMetadata<BakeryMetaData>();
            if (metadata == null)
            {
                metadata = new();
                entry.AddMetadata(metadata);
            }

            if (m_ScopeIndex != -1)
            {
                metadata.Scope = reader.GetField(m_ScopeIndex);
            }

            if (m_ContextIndex != -1)
            {
                metadata.Context = reader.GetField(m_ContextIndex);
            }
            if (m_MaxLengthIndex != -1)
            {
                metadata.MaxLength = reader.GetField<int>(m_MaxLengthIndex);
            }
            if (m_CommentToTranslatorIndex != -1)
            {
                metadata.CommentToTranslator = reader.GetField(m_CommentToTranslatorIndex);
            }
            if (m_NeedsReviewIndex != -1)
            {
                metadata.IsFuzzy = bool.Parse(reader.GetField(m_NeedsReviewIndex));
            }
        }

        public override void WriteBegin(StringTableCollection collection, CsvWriter writer)
        {
            // Does the collection contain a string table for our Locale Id?
            var tables = collection.StringTables;
            m_CollectionTableIndex = -1;
            for (int i = 0; i < tables.Count; ++i)
            {
                if (tables[i].LocaleIdentifier == _localeIdentifier)
                {
                    m_CollectionTableIndex = i;
                }
            }

            if (m_CollectionTableIndex != -1)
            {
                writer.WriteField(ScopeFieldName);
                writer.WriteField(ContextFieldName);
                writer.WriteField(MaxLengthFieldName);
                writer.WriteField(CommentToTranslatorFieldName);
                writer.WriteField(NeedsReview);
            }
        }

        public override void WriteRow(
            SharedTableData.SharedTableEntry keyEntry,
            IList<StringTableEntry> tableEntries,
            CsvWriter writer
        )
        {
            if (m_CollectionTableIndex != -1 && tableEntries[m_CollectionTableIndex] != null)
            {
                var entry = tableEntries[m_CollectionTableIndex];
                var metadata = entry.GetMetadata<BakeryMetaData>();
                if (metadata != null)
                {
                    writer.WriteField(metadata.Scope, true);
                    writer.WriteField(metadata.Context, true);
                    writer.WriteField(metadata.MaxLength);
                    writer.WriteField(metadata.CommentToTranslator, true);
                    writer.WriteField(metadata.IsFuzzy);

                    return;
                }
            }

            // Write empty entries
            writer.WriteField(string.Empty);
            writer.WriteField(string.Empty);
            writer.WriteField(0);
            writer.WriteField(string.Empty);
            writer.WriteField(true);
        }
    }
}
