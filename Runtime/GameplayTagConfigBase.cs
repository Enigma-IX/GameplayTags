using System;

namespace BandoWare.GameplayTags
{
   [Serializable]
   public class TagDefinitionEntry : IComparable<TagDefinitionEntry>, IEquatable<TagDefinitionEntry>
   {
      public string tagName;
      public string description;

      public TagDefinitionEntry(string tagName, string description)
      {
         this.tagName = tagName;
         this.description = description;
      }

      public int CompareTo(TagDefinitionEntry other)
      {
         return string.CompareOrdinal(tagName, other.tagName);
      }

      public bool Equals(TagDefinitionEntry other)
      {
         return other != null
            && string.Equals(tagName, other.tagName, StringComparison.OrdinalIgnoreCase);
      }
   }

   [Serializable]
   public class TagRenameEntry : IComparable<TagRenameEntry>, IEquatable<TagRenameEntry>
   {
      public string oldTagName;
      public string newTagName;

      public TagRenameEntry(string oldTagName, string newTagName)
      {
         this.oldTagName = oldTagName;
         this.newTagName = newTagName;
      }

      public int CompareTo(TagRenameEntry other)
      {
         return string.CompareOrdinal(oldTagName + newTagName, other.oldTagName + other.newTagName);
      }

      public bool Equals(TagRenameEntry other)
      {
         return other != null
            && string.Equals(oldTagName, other.oldTagName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(newTagName, other.newTagName, StringComparison.OrdinalIgnoreCase);
      }
   }

   public abstract class GameplayTagConfigBase
   {
      public abstract TagDefinitionEntry[] GetGameplayTagDefinitionEntries();
      public abstract TagRenameEntry[] GetGameplayTagsRenameEntries();

      public static bool TryGetGeneratedGameplayTagConfig(out GameplayTagConfigBase tagConfig)
      {
#if UNITY_EDITOR
         UnityEditor.TypeCache.TypeCollection configTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<GameplayTagConfigBase>();
         if (configTypes.Count > 0)
         {
            tagConfig = (GameplayTagConfigBase)Activator.CreateInstance(configTypes[0]);
            return true;
         }
         tagConfig = null;
         return false;
#else
         foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
         {
            foreach (Type type in assembly.GetTypes())
            {
               if (type.IsSubclassOf(typeof(GameplayTagConfigBase)))
               {
                  tagConfig = (GameplayTagConfigBase)Activator.CreateInstance(type);
                  return true;
               }
            }
         }
         tagConfig = null;
         return false;
#endif
      }
   }
}