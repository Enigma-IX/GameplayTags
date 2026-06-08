using UnityEditor;
using UnityEngine;

namespace BandoWare.GameplayTags.Editor
{
   public class MenuItems
   {
      [MenuItem("BW/Reload Tags")]
      public static void ReloadTags()
      {
         GameplayTagManager.ReloadTags();
         Debug.Log("[Gameplay Tags] Tags reloaded.");
      }
   }
}