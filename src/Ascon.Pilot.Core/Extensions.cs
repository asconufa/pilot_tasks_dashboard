using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Ascon.Pilot.Core
{
    public static class Extensions
    {
        /// Ignores Order and Duplicates
        public static bool ListEquals<T>(this IEnumerable<T> firstEnumerable, IEnumerable<T> secondEnumerable)
        {
            if (firstEnumerable == null && secondEnumerable == null)
                return true;

            if (firstEnumerable == null || secondEnumerable == null)
                return false;

            return new HashSet<T>(firstEnumerable).SetEquals(secondEnumerable);
        }

        /// Ignores Order but consider Duplicates
        public static bool SequenceEqualIgnoreOrder<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null && second == null)
                return true;

            if (first == null || second == null)
                return false;

            var firstList = first.ToList();
            if (second.Any(item => firstList.Remove(item) == false))
                return false;
            return firstList.Count == 0;
        }
    }

    public static class StringExtensions
    {
        public static bool IsContainWhiteSpace(this string value)
        {
            if (value == null) 
                return false;

            return value.Any(t => Char.IsWhiteSpace(t));
        }

        public static string LimitCharacters(this string text, int length)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // If text in shorter or equal to length, just return it
            if (text.Length <= length)
            {
                return text;
            }

            // Text is longer, so try to find out where to cut
            char[] delimiters = { ' ', '.', ',', ':', ';' };
            int index = text.LastIndexOfAny(delimiters, length - 3);

            if (index > (length / 2))
            {
                return text.Substring(0, index) + "\u2026";
            }
            return text.Substring(0, length - 3) + "\u2026";
        }

        public static string RemoveInvalidFileNameCharacters(this string str)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            string result = str;
            foreach (char c in invalid)
            {
                result = result.Replace(c.ToString(), string.Empty);
            }
            return result;
        }

        public static string GetXamlName(this string value)
        {
            return new String(value.Where(c => Char.IsLetterOrDigit(c)).ToArray());
        }

        public static string TrimQuotes(this string value)
        {
            return value.Replace("\"", "");
        }
    }

    public static class EnumExtensions
    {
        public static long Next<T>(this IEnumerable<T> source, Func<T,long> func)
        {
            if (!source.Any())
                return 1;

            return source.Max(func) + 1;
        }

        public static int Next<T>(this IEnumerable<T> source, Func<T, int> func)
        {
            if (!source.Any())
                return 1;

            return source.Max(func) + 1;
        }
    }

    public static class DChangeExtensions
    {
        public static bool IsObjectPermanentlyDeleted(this DChange change)
        {
            return change.New != null &&
                   change.Old != null &&
                   change.New.IsDeleted &&
                   change.New.Id != DObject.RootId;
        }
    }

    public static class DObjectExtensions
    {
        public static bool InRecycleBin(this DObject obj)
        {
            return obj != null && obj.ParentId == Guid.Empty && obj.IsInRecycleBin && !obj.IsDeleted;
        }

        public static DObject ToForbidden(this DObject obj)
        {
            var result = new DObject { Id = obj.Id, ParentId = obj.ParentId, LastChange = obj.LastChange };
            return result;
        }

        public static void ClearTaskVersions(this DObject obj)
        {
            obj.Children.RemoveAll(x => x.TypeId == obj.TypeId);
        }

        public static string GetTitle(this DObject obj, MType type)
        {
            if (type.IsProjectFileOrFolder())
            {
                DValue name;
                if (obj.Attributes.TryGetValue(SystemAttributes.PROJECT_ITEM_NAME, out name))
                    return (string)name;
                return "unnamed";
            }
            return GetObjectTitle(obj, type);
        }

        #region Tasks

        public static IEnumerable<Guid> TaskVersions(this DObject obj)
        {
            return obj.Children.Where(x => x.TypeId == obj.TypeId).Select(x => x.ObjectId);
        }

        public static Guid TaskVersion(this DObject obj, int index)
        {
            return TaskVersions(obj).ElementAt(index);
        }

        public static int TaskVersionsCount(this DObject obj)
        {
            return TaskVersions(obj).Count();
        }

        public static int GetInitiatorPosition(this DObject obj)
        {
            return int.Parse(obj.Attributes[SystemAttributes.TASK_INITIATOR_POSITION].ToString());
        }

        public static int GetExecutorPosition(this DObject obj)
        {
            return int.Parse(obj.Attributes[SystemAttributes.TASK_EXECUTOR_POSITION].ToString());
        }

        public static string GetTaskTitle(this DObject obj)
        {
            DValue title;
            return obj.Attributes.TryGetValue(SystemAttributes.TASK_TITLE, out title) ? (string) title : string.Empty;
        }

        public static string GetTaskDescription(this DObject obj)
        {
            DValue description;
            return obj.Attributes.TryGetValue(SystemAttributes.TASK_DESCRIPTION, out description) ? (string)description : string.Empty;
        }

        public static DateTime GetTaskDeadline(this DObject obj)
        {
            DValue deadline;
            return obj.Attributes.TryGetValue(SystemAttributes.TASK_DEADLINE_DATE, out deadline) ? (DateTime)deadline : DateTime.MaxValue;
        }

        public static int GetTaskState(this DObject obj)
        {
            return (int)(long)obj.Attributes[SystemAttributes.TASK_STATE];
        }

        public static int GetTaskStageOrder(this DObject obj)
        {
            DValue order;
            return obj.Attributes.TryGetValue(SystemAttributes.TASK_STAGE_ORDER, out order) ? int.Parse(order.ToString()) : 0;
        }

        #endregion

        private static string GetObjectTitle(DObject obj, MType type)
        {
            var sb = new StringBuilder();
            var attibutes = AttributeFormatter.Format(type, obj.Attributes.ToDictionary(attr => attr.Key, attr => (attr.Value)));

            foreach (var displayableAttr in type.GetDisplayAttributes())
            {
                DValue value;
                if (attibutes.TryGetValue(displayableAttr.Name, out value))
                {
                    var strValue = value.ToString();
                    if (sb.Length != 0)
                        sb.Append(Constants.PROJECT_TITLE_ATTRIBUTES_DELIMITER);

                    sb.Append(strValue);
                }
            }
            return sb.ToString();
        }
    }

    public static class NTypeExtensions
    {
        public static bool IsProjectFolder(this MType type)
        {
            return IsProjectFolder(type.Name);
        }

        public static bool IsProjectFolder(string typeName)
        {
            return typeName.Equals(SystemTypes.PROJECT_FOLDER, StringComparison.Ordinal);
        }

        public static bool IsProjectFile(this MType type)
        {
            return IsProjectFile(type.Name);
        }

        public static bool IsProjectFile(string typeName)
        {
            return typeName.Equals(SystemTypes.PROJECT_FILE, StringComparison.Ordinal);
        }

        public static bool IsProjectFileOrFolder(this MType type)
        {
            return IsProjectFileOrFolder(type.Name);
        }
        
        public static bool IsProjectFileOrFolder(string typeName)
        {
            return IsProjectFile(typeName) || IsProjectFolder(typeName);
        }

        public static IEnumerable<MAttribute> GetDisplayAttributes(this MType type)
        {
            return type.Attributes.Where(d => d.ShowInTree).OrderBy(d => d.DisplaySortOrder);
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime ToUniversalDateTime(this DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                return dateTime.ToUniversalTime();
            return dateTime;
        }
    }
}
