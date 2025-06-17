using System;

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
#endif

namespace BandoWare.GameplayTags
{
   [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
   public class GameplayTagFilterAttribute : Attribute
   {
      public string[] FilterTagNames { get; }

      public GameplayTagFilterAttribute(params string[] filterTagNames)
      {
         FilterTagNames = filterTagNames;
      }

      public GameplayTagFilterAttribute(params Type[] filterTagTypes)
      {
#if UNITY_EDITOR
         List<string> filterTagNames = new();
         foreach (Type type in filterTagTypes)
         {
            MethodInfo method = type.GetMethod("Get", BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
               GameplayTag tag = (GameplayTag)method.Invoke(null, null);
               if (tag != GameplayTag.None)
               {
                  filterTagNames.Add(tag.Name);
               }
            }
         }
         FilterTagNames = filterTagNames.ToArray();
#endif
      }

      public static string[] GetFilterTagNamesFromField(FieldInfo fieldInfo)
      {
         return fieldInfo?.GetCustomAttribute<GameplayTagFilterAttribute>(false)?.FilterTagNames;
      }
   }
}