using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

#if UNITY_6000_2_OR_NEWER
using TreeView = UnityEditor.IMGUI.Controls.TreeView<int>;
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
#else
using TreeView = UnityEditor.IMGUI.Controls.TreeView;
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState;
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem;
#endif

namespace BandoWare.GameplayTags.Editor
{
   public static class TreeViewMethodExtensions
   {
      public static void ShowPopupWindow(this TreeViewPopupContent.TreeViewBase treeView, Rect activatorRect, float maxHeight)
      {
         TreeViewPopupContent treeViewPopupContent = new(activatorRect.width, maxHeight, treeView);
         PopupWindow.Show(activatorRect, treeViewPopupContent);
      }
   }

   public class TreeViewPopupContent : PopupWindowContent
   {
      public abstract class TreeViewBase : TreeView
      {
         public TreeViewBase(TreeViewState state) : base(state)
         {
         }

         public virtual float GetTotalHeight()
         {
            return totalHeight;
         }
      }

      private TreeView m_TreeView;
      private float m_Width;
      private float m_MaxHeight;

      public TreeViewPopupContent(float width, float maxHeight, TreeViewBase tagTreeView)
      {
         m_Width = width;
         m_MaxHeight = maxHeight;
         m_TreeView = tagTreeView;
      }

      public override void OnGUI(Rect rect)
      {
         m_TreeView.OnGUI(rect);
      }

      public override Vector2 GetWindowSize()
      {
         return new Vector2(m_Width, m_MaxHeight);
      }
   }
}

