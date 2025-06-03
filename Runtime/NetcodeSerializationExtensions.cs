#if NETCODE_FOR_GAMEOBJECTS
using Unity.Netcode;

namespace BandoWare.GameplayTags
{
   public static class NetcodeSerializationExtensions
   {
      //--------------------------------------------------------------------------------------------------------------
      // GameplayTag
      //--------------------------------------------------------------------------------------------------------------

      public static void SerializeValue<T>(this BufferSerializer<T> serializer, ref GameplayTag tag)
         where T : IReaderWriter
      {
         if (serializer.IsWriter)
         {
            serializer.GetFastBufferWriter().WriteValueSafe(tag);
         }
         else
         {
            serializer.GetFastBufferReader().ReadValueSafe(out tag);
         }
      }

      public static void WriteValueSafe(this FastBufferWriter writer, in GameplayTag tag)
      {
         writer.WriteValueSafe(tag.RuntimeIndex);
      }

      public static void ReadValueSafe(this FastBufferReader reader, out GameplayTag tag)
      {
         reader.ReadValueSafe(out int runtimeIndex);

         if (runtimeIndex != 0)
         {
            GameplayTagDefinition definition = GameplayTagManager.GetDefinitionFromRuntimeIndex(runtimeIndex);
            if (definition != null)
            {
               tag = new GameplayTag(definition.TagName, runtimeIndex);
               return;
            }
         }

         tag = new GameplayTag(null, runtimeIndex);
      }

      //--------------------------------------------------------------------------------------------------------------
      // GameplayTagContainer
      //--------------------------------------------------------------------------------------------------------------

      public static void SerializeValue<T>(this BufferSerializer<T> serializer, ref GameplayTagContainer container)
         where T : IReaderWriter
      {
         if (serializer.IsWriter)
         {
            serializer.GetFastBufferWriter().WriteValueSafe(container);
         }
         else
         {
            serializer.GetFastBufferReader().ReadValueSafe(out container);
         }
      }

      public static void WriteValueSafe(this FastBufferWriter writer, in GameplayTagContainer container)
      {
         writer.WriteValueSafe(container.ExplicitTagCount);

         foreach (GameplayTag tag in container.GetExplicitTags())
         {
            writer.WriteValueSafe(tag);
         }
      }

      public static void ReadValueSafe(this FastBufferReader reader, out GameplayTagContainer container)
      {
         container = new GameplayTagContainer();

         reader.ReadValueSafe(out int explicitTagCount);

         for (int i = 0; i < explicitTagCount; i++)
         {
            reader.ReadValueSafe(out GameplayTag tag);
            container.AddTag(tag);
         }
      }
   }

   //--------------------------------------------------------------------------------------------------------------
}
#endif