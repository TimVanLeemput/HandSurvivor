using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HandSurvivor.Editor.Tools
{
    /// <summary>
    /// Batch script to wrap all Debug.Log* calls with debug flag checks
    /// </summary>
    public static class ApplyDebugWrappersToAll
    {
        private const string DEBUG_CHECK_LOG = "if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)";
        private const string DEBUG_CHECK_WARNING = "if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogWarning)";
        private const string DEBUG_CHECK_ERROR = "if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogError)";

        private const string DEBUG_FIELD = "[Header(\"Debug\")]\n        [SerializeField] private bool showDebugLogs = true;\n\n        ";
        private const string DEBUG_FIELD_STATIC = "private static bool showDebugLogs = true;\n\n        ";

        [MenuItem("Tools/Debug Logs/Apply Debug Wrappers to All Scripts")]
        private static void ApplyToAllScripts()
        {
            string scriptsPath = Path.Combine(Application.dataPath, "Scripts");

            if (!Directory.Exists(scriptsPath))
            {
                UnityEngine.Debug.LogError($"[ApplyDebugWrappersToAll] Scripts directory not found: {scriptsPath}");
                return;
            }

            string[] csFiles = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);
            int processedCount = 0;
            int modifiedCount = 0;

            foreach (string filePath in csFiles)
            {
                // Skip editor scripts and this tool itself
                if (filePath.Contains("\\Editor\\") || filePath.EndsWith("ApplyDebugWrappersToAll.cs"))
                {
                    continue;
                }

                if (ProcessFile(filePath))
                {
                    modifiedCount++;
                }
                processedCount++;
            }

            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"[ApplyDebugWrappersToAll] Processed {processedCount} files, modified {modifiedCount} files");
        }

        private static bool ProcessFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            string originalContent = content;

            // Check if file has any Debug.Log calls
            if (!Regex.IsMatch(content, @"Debug\.(Log|LogWarning|LogError)"))
            {
                return false; // No debug calls, skip
            }

            // Check if already processed (has showDebugLogs field)
            if (content.Contains("bool showDebugLogs"))
            {
                return false; // Already processed
            }

            bool isMonoBehaviour = content.Contains(": MonoBehaviour");
            bool isScriptableObject = content.Contains(": ScriptableObject");
            bool isStaticClass = content.Contains("static class");

            // Check if inherits from a base class that might already have showDebugLogs
            // (e.g., ": ActiveSkillBase", ": SomeCustomBase")
            bool inheritsFromCustomBase = Regex.IsMatch(content, @":\s+(?!MonoBehaviour|ScriptableObject)\w+");

            // Only process MonoBehaviours, ScriptableObjects, or static classes
            if (!isMonoBehaviour && !isScriptableObject && !isStaticClass)
            {
                return false;
            }

            // Don't add debug field if inheriting from custom base (might have it already)
            // But still wrap the debug calls
            if (!inheritsFromCustomBase)
            {
                if (isStaticClass)
                {
                    content = AddStaticDebugField(content);
                }
                else
                {
                    content = AddDebugField(content);
                }
            }

            // Wrap all Debug.Log* calls
            content = WrapDebugCalls(content);

            if (content != originalContent)
            {
                File.WriteAllText(filePath, content);
                return true;
            }

            return false;
        }

        private static string AddDebugField(string content)
        {
            // Find first [Header or [SerializeField after class declaration
            Match classMatch = Regex.Match(content, @"(class\s+\w+[^{]*\{)\s*(\r?\n\s*)");

            if (classMatch.Success)
            {
                int insertPos = classMatch.Index + classMatch.Groups[1].Length + classMatch.Groups[2].Length;
                content = content.Insert(insertPos, DEBUG_FIELD);
            }

            return content;
        }

        private static string AddStaticDebugField(string content)
        {
            // Find first line after static class declaration
            Match classMatch = Regex.Match(content, @"(static\s+class\s+\w+[^{]*\{)\s*(\r?\n\s*)");

            if (classMatch.Success)
            {
                int insertPos = classMatch.Index + classMatch.Groups[1].Length + classMatch.Groups[2].Length;
                content = content.Insert(insertPos, DEBUG_FIELD_STATIC);
            }

            return content;
        }

        private static string WrapDebugCalls(string content)
        {
            // Match Debug.Log/LogWarning/LogError calls that aren't already wrapped
            string patternLog = @"(?<!if \(showDebugLogs[^\r\n]*\r?\n\s{0,20})(\s*)(Debug\.Log\([^;]+\);)";
            string patternWarning = @"(?<!if \(showDebugLogs[^\r\n]*\r?\n\s{0,20})(\s*)(Debug\.LogWarning\([^;]+\);)";
            string patternError = @"(?<!if \(showDebugLogs[^\r\n]*\r?\n\s{0,20})(\s*)(Debug\.LogError\([^;]+\);)";

            // Wrap Debug.Log
            content = Regex.Replace(content, patternLog, match => WrapDebugCall(content, match, DEBUG_CHECK_LOG));

            // Wrap Debug.LogWarning
            content = Regex.Replace(content, patternWarning, match => WrapDebugCall(content, match, DEBUG_CHECK_WARNING));

            // Wrap Debug.LogError
            content = Regex.Replace(content, patternError, match => WrapDebugCall(content, match, DEBUG_CHECK_ERROR));

            return content;
        }

        private static string WrapDebugCall(string content, Match match, string debugCheck)
        {
            string indent = match.Groups[1].Value;
            string debugCall = match.Groups[2].Value;

            // Check if this is already part of an if statement (avoid double wrapping)
            int lineStart = content.LastIndexOf('\n', match.Index) + 1;
            string linePrefix = content.Substring(lineStart, match.Index - lineStart).Trim();

            // Skip if already wrapped
            if (linePrefix.StartsWith("if (showDebugLogs"))
            {
                return match.Value; // Already wrapped
            }

            // Skip if commented out (single-line or multi-line comments)
            if (linePrefix.StartsWith("//") || linePrefix.Contains("/*") || IsInsideComment(content, match.Index))
            {
                return match.Value; // Skip commented lines
            }

            // Single-line wrap with type-specific check
            return $"{indent}{debugCheck}\n{indent}    {debugCall}";
        }

        private static bool IsInsideComment(string content, int index)
        {
            // Check if index is inside a multi-line comment /* */
            int lastCommentStart = content.LastIndexOf("/*", index);
            int lastCommentEnd = content.LastIndexOf("*/", index);

            if (lastCommentStart > lastCommentEnd)
            {
                return true; // Inside multi-line comment
            }

            // Check if on a line that starts with //
            int lineStart = content.LastIndexOf('\n', index);
            if (lineStart == -1) lineStart = 0;

            string linePrefix = content.Substring(lineStart, index - lineStart).TrimStart();
            return linePrefix.StartsWith("//");
        }
    }
}
