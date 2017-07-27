using System;
using System.Linq;

namespace Ascon.Pilot.Core
{
    public static class Constants
    {
        public const string PROJECT_TITLE_ATTRIBUTES_DELIMITER = " - ";
        public const int MAX_PROJECT_ITEM_FOLDER_NAME_LENGTH = 40;
        public const string TRUNCATED_NAME_MARK = "~";
        public const string ANNOTATIONS_DEFINITION = "Annotation";
        public const string ANNOTATION_CHAT_MESSAGE = "Note_Chat_Message";
        public const string DIGITAL_SIGNATURE = "PilotDigitalSignature";
        public const string THUMBNAIL_FILE_NAME_POSTFIX = "PilotThumbnail";
        public const string TEXT_LABELS_DEFINITION = "PilotTextLabels";
        public const string BARCODE_DEFINITION = "PilotBarcode";        
        public static readonly char[] Separators = { ' ', '.', ',', '"', '\'', '!', '?', ':', ';' };
        public const int MAX_ITEMS_LOAD_PER_PAGE = 250;
    }
}
