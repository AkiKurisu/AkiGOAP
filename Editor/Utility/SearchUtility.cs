using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.GOAP.Editor
{
    internal class SearchUtility
    {
        public static string GetName(Type type)
        {
            var array=type.GetCustomAttributes(typeof(GOAPLabelAttribute), false) as GOAPLabelAttribute[];
            if(array.Length>0)return array[0].Title;
            return type.Name;
        }
        public static List<Type> FindSubClassTypes(Type father)
        {
           return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.IsSubclassOf(father) && !t.IsAbstract).ToList();
        }
       public static List<Type> FindSubClassTypes(Type[] fathers)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .Where(t => fathers.Any(f => t.IsSubclassOf(f)))
                            .Where(t => !t.IsAbstract)
                            .ToList();
        }
        const char Span='/';
        public static string[] GetSplittedGroupName (string group)
        {
            var array=group.Split(Span,StringSplitOptions.RemoveEmptyEntries) ;
            return array.Length>0?array: new string[1]{group};
        }
    }
    public static class SearchExtension
    {
        public static IEnumerable<IGrouping<string, Type>> GroupsByGroup(this IEnumerable<Type> types)
        {
            return types.GroupBy(t=>
            {
                var array=t.GetCustomAttributes(typeof(GOAPGroupAttribute), false) as GOAPGroupAttribute[];
                return array.Length>0?SearchUtility.GetSplittedGroupName(array[0].Group)[0]:null;
            }).Where(x=>!string.IsNullOrEmpty(x.Key));
        }
        public static IEnumerable<IGrouping<string, Type>> SubGroups(this IGrouping<string, Type>group,int level)
        {
            return group.GroupBy(t=>
            {
                var array=t.GetCustomAttributes(typeof(GOAPGroupAttribute), false) as GOAPGroupAttribute[];
                var subcategory=SearchUtility.GetSplittedGroupName(array[0].Group);
                return subcategory.Length>level?subcategory[level]:null;
            }).Where(x=>!string.IsNullOrEmpty(x.Key));
        }
        public static void AddEntry(this List<SearchTreeEntry> entries,Type _type,int _level,Texture icon)
        {
            entries.Add(new SearchTreeEntry(new GUIContent(SearchUtility.GetName(_type),icon)) { level = _level, userData = _type });
        }
        public static void AddAllEntries(this List<SearchTreeEntry> entries, IGrouping<string, Type> group,Texture icon,int level,int subCount=1)
        {
            entries.Add(new SearchTreeGroupEntry(new GUIContent($"Select {group.Key}"),level));
            var subGroups=group.SubGroups(subCount);
            var left=group.Except(subGroups.SelectMany(x=>x));
            foreach(var subGroup in subGroups)
            {
                entries.AddAllEntries(subGroup,icon,level+1,subCount+1);
            }
            foreach(Type type in left)
            {
                entries.AddEntry(type,level+1,icon);
            }
        }
    }
}
