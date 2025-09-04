using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WWIII.SideScroller.Editor.Data
{
    public static class CSVUtil
    {
        public static IEnumerable<Dictionary<string,string>> Read(string path)
        {
            if (!File.Exists(path)) yield break;
            var lines = File.ReadAllLines(path).Where(l=>!string.IsNullOrWhiteSpace(l)).ToList();
            if (lines.Count == 0) yield break;
            var header = Split(lines[0]);
            for (int i=1;i<lines.Count;i++)
            {
                var cells = Split(lines[i]);
                var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
                for (int c=0;c<header.Length && c<cells.Length;c++)
                    dict[header[c]] = cells[c];
                yield return dict;
            }
        }

        private static string[] Split(string line)
        {
            // basic CSV split respecting simple quotes
            var res = new List<string>();
            bool q=false; var cur="";
            foreach (var ch in line)
            {
                if (ch=='"') { q=!q; continue; }
                if (ch==',' && !q) { res.Add(cur); cur=""; continue; }
                cur+=ch;
            }
            res.Add(cur);
            return res.Select(s=>s.Trim()).ToArray();
        }
    }
}
