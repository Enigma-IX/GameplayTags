using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BandoWare.GameplayTags
{
   public static class GameplayTagManager
   {
      private static Dictionary<string, GameplayTagDefinition> s_TagDefinitionsByName = new();
      private static GameplayTagDefinition[] s_TagsDefinitions;
      private static GameplayTag[] s_Tags;
      private static bool s_IsInitialized;

      public static ReadOnlySpan<GameplayTag> GetAllTags()
      {
         InitializeIfNeeded();
         return new ReadOnlySpan<GameplayTag>(s_Tags);
      }

      internal static GameplayTagDefinition GetDefinitionFromRuntimeIndex(int runtimeIndex)
      {
         InitializeIfNeeded();
         return s_TagsDefinitions[runtimeIndex];
      }

      public static GameplayTag RequestTag(string name)
      {
         if (string.IsNullOrEmpty(name))
         {
            return GameplayTag.None;
         }

         if (!TryGetDefinition(name, out GameplayTagDefinition definition))
         {
            Debug.LogWarning($"No tag registered with name \"{name}\".");
            return GameplayTag.None;
         }

         return definition.Tag;
      }

      public static bool RequestTag(string name, out GameplayTag tag)
      {
         if (TryGetDefinition(name, out GameplayTagDefinition definition))
         {
            tag = definition.Tag;
            return true;
         }

         tag = GameplayTag.None;
         return false;
      }

      private static bool TryGetDefinition(string name, out GameplayTagDefinition definition)
      {
         InitializeIfNeeded();
         return s_TagDefinitionsByName.TryGetValue(name, out definition);
      }

      public static void InitializeIfNeeded()
      {
         if (s_IsInitialized)
            return;

         GameplayTagRegistrationContext context = new();
         IReadOnlyList<TagRenameEntry> tagRenameEntries = Array.Empty<TagRenameEntry>();

         // Register all tags and rename entries found in the generated TagConfig class.
         if (GameplayTagConfigBase.TryGetGeneratedGameplayTagConfig(out GameplayTagConfigBase tagConfig))
         {
            foreach (TagDefinitionEntry definitionEntry in tagConfig.GetGameplayTagDefinitionEntries())
            {
               try
               {
                  context.RegisterTag(definitionEntry.tagName, definitionEntry.description);
               }
               catch (Exception exception)
               {
                  Debug.LogError($"Failed to register tag {definitionEntry.tagName} from {tagConfig.GetType().FullName} with error: {exception.Message}");
               }
            }

            // Load all unique TagRenameEntries.
            tagRenameEntries = tagConfig.GetGameplayTagsRenameEntries();
         }

         // Register all tags found in any Assembly via GameplayTagAttribute.
         foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
         {
            foreach (GameplayTagAttribute attribute in assembly.GetCustomAttributes<GameplayTagAttribute>())
            {
               try
               {
                  context.RegisterTag(attribute.TagName, attribute.Description, attribute.Flags);
               }
               catch (Exception exception)
               {
                  Debug.LogError($"Failed to register tag {attribute.TagName} from assembly {assembly.FullName} with error: {exception.Message}");
               }
            }
         }

         s_TagsDefinitions = context.GenerateDefinitions();

         // Skip the first tag definition which is the "None" tag.
         IEnumerable<GameplayTag> tags = s_TagsDefinitions
            .Select(definition => definition.Tag)
            .Skip(1);

         s_Tags = Enumerable.ToArray(tags);
         foreach (GameplayTagDefinition definition in s_TagsDefinitions)
            s_TagDefinitionsByName[definition.TagName] = definition;

         // Remap the OldTagNames registered via TagRenameEntries to point at the TagDefinition of the NewTagName.
         // From newest (last) to oldest (first) rename entry.
         for (int i = tagRenameEntries.Count - 1; i >= 0; i--)
         {
            TagRenameEntry renameEntry = tagRenameEntries[i];
            if (s_TagDefinitionsByName.TryGetValue(renameEntry.newTagName, out GameplayTagDefinition definition))
            {
               // Use TryAdd, so we don't accidentally override an already valid Tag to Definition mapping
               if (!s_TagDefinitionsByName.TryAdd(renameEntry.oldTagName, definition))
               {
                  Debug.LogWarning($"Invalid TagRenameEntry detected: OldTagName '{renameEntry.oldTagName}' was be renamed " +
                                   $"to '{renameEntry.newTagName}', but is already mapped to Definition of '{definition.TagName}'");
               }
            }
         }

         s_IsInitialized = true;
      }
   }
}