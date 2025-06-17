using System;
using System.Diagnostics;
using UnityEditor.UIElements;
using UnityEngine;

namespace BandoWare.GameplayTags
{
   [Serializable]
   [DebuggerDisplay("{m_Name,nq}")]
   public struct GameplayTag : IEquatable<GameplayTag>, ISerializationCallbackReceiver
   {
      /// <summary>
      /// Makes it possible to use <see cref="GameplayTag"/> type with UI Builder.
      /// </summary>
      internal class GameplayTagConverter : UxmlAttributeConverter<GameplayTag>
      {
         public override GameplayTag FromString(string value) => DeserializeFromString(value);
         public override string ToString(GameplayTag value) => SerializeToString(value);
      }
      
      /// <summary>
      /// Represents an invalid tag.
      /// </summary>
      public static readonly GameplayTag None = new() { m_RuntimeIndex = 0 };

      internal readonly int RuntimeIndex => m_RuntimeIndex;

      internal readonly GameplayTagDefinition Definition
      {
         get
         {
            ValidateIsNotNone();
            return GameplayTagManager.GetDefinitionFromRuntimeIndex(m_RuntimeIndex);
         }
      }

      /// <inheritdoc cref="GameplayTagDefinition.ParentTags" />
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public readonly ReadOnlySpan<GameplayTag> ParentTags => Definition.ParentTags;

      /// <inheritdoc cref="GameplayTagDefinition.ChildTags" />
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public readonly ReadOnlySpan<GameplayTag> ChildTags => Definition.ChildTags;

      /// <inheritdoc cref="GameplayTagDefinition.HierarchyTags" />
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public readonly ReadOnlySpan<GameplayTag> HierarchyTags => Definition.HierarchyTags;

      /// <inheritdoc cref="GameplayTagDefinition.Label" />
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public readonly string Label => Definition.Label;

      /// <inheritdoc cref="GameplayTagDefinition.HierarchyLevel" />
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public readonly int HierarchyLevel => Definition.HierarchyLevel;

      /// <inheritdoc cref="GameplayTagDefinition.Description" />
      public readonly string Description => Definition.Description;

      /// <summary>
      /// The parent tag of this tag. If this tag is "A.B.C", the parent tag will be "A.B".
      /// </summary>
      public readonly GameplayTag ParentTag
      {
         get
         {
            GameplayTagDefinition parentDefinition = Definition.ParentTagDefinition;

            if (parentDefinition == null)
               return None;

            return parentDefinition.Tag;
         }
      }

      /// <inheritdoc cref="GameplayTagDefinition.Flags" />
      public readonly GameplayTagFlags Flags => Definition.Flags;

      public readonly string Name
      {
         get
         {
            ValidateIsNotNone();
            return m_Name;
         }
      }

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      [SerializeField]
      private string m_Name;

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private int m_RuntimeIndex;

      internal GameplayTag(string name, int runtimeTagIndex)
      {
         m_Name = name;
         m_RuntimeIndex = runtimeTagIndex;
      }

      /// <inheritdoc cref="GameplayTagDefinition.IsChildOf(GameplayTag)"/>/>
      public readonly bool IsParentOf(in GameplayTag tag)
      {
         ValidateIsNotNone();
         return Definition.IsParentOf(tag);
      }


      /// <inheritdoc cref="GameplayTagDefinition.IsChildOf(GameplayTag)"/>/>
      public readonly bool IsChildOf(in GameplayTag parentTag)
      {
         ValidateIsNotNone();
         return Definition.IsChildOf(parentTag);
      }

      public readonly bool Equals(GameplayTag other)
      {
         return m_RuntimeIndex == other.m_RuntimeIndex;
      }

      public override readonly bool Equals(object obj)
      {
         if (obj is GameplayTag other)
            return other.m_RuntimeIndex == m_RuntimeIndex;

         if (obj is string otherStr)
            return m_Name == otherStr;

         return false;
      }

      public override readonly int GetHashCode()
      {
         return m_RuntimeIndex;
      }

      public override readonly string ToString()
      {
         if (m_RuntimeIndex == 0)
            return "<None>";

         return m_Name;
      }

      void ISerializationCallbackReceiver.OnBeforeSerialize()
      {
         m_Name = SerializeToString(this);
      }

      void ISerializationCallbackReceiver.OnAfterDeserialize()
      {
         this = DeserializeFromString(m_Name);
      }

      public static string SerializeToString(GameplayTag tag)
      {
         if (tag.m_RuntimeIndex == 0)
         {
            return null;
         }

         GameplayTagDefinition definition = GameplayTagManager.GetDefinitionFromRuntimeIndex(tag.m_RuntimeIndex);
         return definition?.TagName;
      }

      public static GameplayTag DeserializeFromString(string tagName)
      {
         if (string.IsNullOrEmpty(tagName))
         {
            return None;
         }

         GameplayTag tag = GameplayTagManager.RequestTag(tagName);
         if (tag == None)
         {
            UnityEngine.Debug.LogWarning($"[GameplayTag.DeserializeFromString] No tag registered with name \"{tagName}\".");
            return None;
         }

         return tag;
      }

      private readonly void ValidateIsNotNone()
      {
         if (m_RuntimeIndex == 0)
            throw new InvalidOperationException("Cannot perform operation on GameplayTag.None.");
      }

      public static implicit operator GameplayTag(string tagName)
      {
         return GameplayTagManager.RequestTag(tagName);
      }

      public static bool operator ==(in GameplayTag lhs, in GameplayTag rhs)
      {
         return lhs.m_RuntimeIndex == rhs.m_RuntimeIndex;
      }

      public static bool operator !=(in GameplayTag lhs, in GameplayTag rhs)
      {
         return lhs.m_RuntimeIndex != rhs.m_RuntimeIndex;
      }
   }
}