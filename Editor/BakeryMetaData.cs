using System;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;

namespace Bakery.Translate
{
    [Serializable]
    [DisplayName("Bakery Metadata")]
    [Metadata(AllowedTypes = MetadataType.StringTableEntry)]
    public class BakeryMetaData : IMetadata
    {
        public string Scope;
        public string Context;
        public int MaxLength;
        public string CommentToTranslator;
        public bool IsFuzzy;
    }
}
