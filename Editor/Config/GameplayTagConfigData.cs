using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BandoWare.GameplayTags.Editor.Config
{
   internal class GameplayTagConfigData
   {
      private bool m_IsDirty;

      public string GeneratedClassPath { get; set; }
      public string GeneratedClassName { get; set; }
      public string GeneratedClassNamespace { get; set; }

      public SortedSet<TagDefinitionEntry> TagDefinitionEntries { get; private set; }
      public List<TagRenameEntry> TagRenameEntries { get; private set; }

      public bool IsDirty() => m_IsDirty;
      public void SetDirty() => m_IsDirty = true;
      public void ClearDirty() => m_IsDirty = false;

      public void LoadFromGeneratedClass()
      {
         ClearDirty();

         GeneratedClassPath = GameplayTagProjectSettings.instance.generatedClassPath;
         GeneratedClassName = GameplayTagProjectSettings.instance.generatedClassName;
         GeneratedClassNamespace = GameplayTagProjectSettings.instance.generatedClassNamespace;

         TagDefinitionEntries = new SortedSet<TagDefinitionEntry>();
         TagRenameEntries = new List<TagRenameEntry>();

         if (GameplayTagConfigBase.TryGetGeneratedGameplayTagConfig(out GameplayTagConfigBase tagConfig))
         {
            TagDefinitionEntry[] definitionEntries = tagConfig.GetGameplayTagDefinitionEntries();
            TagDefinitionEntries = new SortedSet<TagDefinitionEntry>(definitionEntries);
            if (CollectionChangedOnImport(definitionEntries, TagDefinitionEntries))
            {
               SetDirty();
            }

            foreach (TagRenameEntry renameEntry in tagConfig.GetGameplayTagsRenameEntries())
            {
               if (!TagRenameEntries.Contains(renameEntry))
               {
                  // Only load unique entries
                  TagRenameEntries.Add(renameEntry);
               }
               else
               {
                  // Set config dirty if there are duplicate rename entries, that have now been stripped
                  SetDirty();
               }
            }
         }
      }

      private static bool CollectionChangedOnImport<T>(ICollection<T> originalCollection, ICollection<T> sortedCollection)
      {
         if (originalCollection.Count != sortedCollection.Count)
         {
            // Collections have changed when their number of item changed
            return true;
         }

         using IEnumerator<T> sortedCollectionEnumerator = sortedCollection.GetEnumerator();
         foreach (T originalItem in originalCollection)
         {
            if (!sortedCollectionEnumerator.MoveNext() || !originalItem.Equals(sortedCollectionEnumerator.Current))
            {
               // Collections have changed when item at the same location differ
               return true;
            }
         }

         return false;
      }

      public bool AddGameplayTagDefinition(TagDefinitionEntry newEntry)
      {
         if (TagDefinitionEntries.Add(newEntry))
         {
            SetDirty();
            return true;
         }

         return false;
      }

      public bool RenameThisGameplayTag(string oldTagName, string newTagName)
      {
         bool tagDefinitionChanged = false;

         // Create temp copy via "ToArray()", so we can modify the original collection during iteration
         foreach (TagDefinitionEntry definitionEntry in TagDefinitionEntries.ToArray())
         {
            if (definitionEntry.tagName == oldTagName)
            {
               // Use remove and add instead of setting the value on the reference, so that sorting is updated properly
               TagDefinitionEntries.Remove(definitionEntry);
               TagDefinitionEntries.Add(new TagDefinitionEntry(newTagName, definitionEntry.description));
               TagRenameEntries.Add(new TagRenameEntry(definitionEntry.tagName, newTagName));
               tagDefinitionChanged = true;
            }
         }

         if (tagDefinitionChanged)
         {
            SetDirty();
            return true;
         }

         return false;
      }

      /// <summary>
      /// Rename all GameplayTags that contain the old TagName in their definition, child tags as well.
      /// </summary>
      /// <returns>True if any GameplayTag was renamed, False otherwise.</returns>
      public bool RenameThisAndParentGameplayTags(string oldTagName, string newTagName)
      {
         bool tagDefinitionChanged = false;

         // Self and parent tag names for the OldTagName
         List<string> oldTagNameParents = GetThisAndParentTagNames(oldTagName);
         List<string> newTagNameParents = GetThisAndParentTagNames(newTagName);

         // Create temp copy via "ToArray()", so we can modify the original collection during iteration
         foreach (TagDefinitionEntry definitionEntry in TagDefinitionEntries.ToArray())
         {
            for (int i = 0; i < Mathf.Min(oldTagNameParents.Count, newTagNameParents.Count); i++)
            {
               if (oldTagNameParents[i] == newTagNameParents[i])
               {
                  continue;
               }

               string newName;
               if (definitionEntry.tagName == oldTagNameParents[i])
               {
                  newName = newTagNameParents[i];
               }
               else if (definitionEntry.tagName.StartsWith(oldTagNameParents[i] + '.'))
               {
                  // At the start of the string, replace the old TagName with the new one
                  newName = newTagNameParents[i] + definitionEntry.tagName.Remove(0, oldTagNameParents[i].Length);
               }
               else
               {
                  // DefinitionEntry does not contain any part of the OldTagName
                  continue;
               }

               // Use remove and add instead of setting the value on the reference, so that sorting is updated properly
               TagDefinitionEntries.Remove(definitionEntry);
               TagDefinitionEntries.Add(new TagDefinitionEntry(newName, definitionEntry.description));
               TagRenameEntries.Add(new TagRenameEntry(definitionEntry.tagName, newName));
               tagDefinitionChanged = true;
               break;
            }
         }

         if (tagDefinitionChanged)
         {
            SetDirty();
            return true;
         }

         return false;
      }

      private List<string> GetThisAndParentTagNames(string tagName)
      {
         List<string> parentTagNames = new();
         string currentTagName = tagName;

         while (currentTagName.Length > 0)
         {
            parentTagNames.Add(currentTagName);
            int removeStartIndex = Mathf.Max(0, currentTagName.LastIndexOf('.'));
            currentTagName = currentTagName.Remove(removeStartIndex);
         }

         return parentTagNames;
      }

      public void ApplyChanges()
      {
         if (!IsDirty())
         {
            return;
         }

         // Save settings to the asset
         GameplayTagProjectSettings settingsAsset = GameplayTagProjectSettings.instance;
         settingsAsset.generatedClassPath = GeneratedClassPath;
         settingsAsset.generatedClassName = GeneratedClassName;
         settingsAsset.generatedClassNamespace = GeneratedClassNamespace;
         settingsAsset.Save();

         // Write config settings into generated class
         GameplayTagConfigCodeGenerator.WriteGeneratedConfigClass(this);

         ClearDirty();
      }
   }
}