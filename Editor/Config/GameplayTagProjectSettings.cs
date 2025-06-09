using UnityEditor;
using UnityEngine;

namespace BandoWare.GameplayTags.Editor.Config
{
   [FilePath("ProjectSettings/GameplayTagProjectSettings.asset", FilePathAttribute.Location.ProjectFolder)]
   public class GameplayTagProjectSettings : ScriptableSingleton<GameplayTagProjectSettings>
   {
      public const string DefaultClassPath = "Assets/Scripts/GameplayTags/" + DefaultClassName + ".cs";
      public const string DefaultClassName = "AllGameplayTags";

      [SerializeField] public string generatedClassPath = DefaultClassPath;
      [SerializeField] public string generatedClassName = DefaultClassName;
      [SerializeField] public string generatedClassNamespace = string.Empty;

      public void Save()
      {
         Save(true);
      }
   }
}