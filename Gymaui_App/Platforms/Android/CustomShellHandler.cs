using Google.Android.Material.BottomNavigation;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;

namespace Gymaui_App.Platforms.Android;

public static class ShellIconTintRemover
{
    public static void DisableIconTinting(global::Android.Views.ViewGroup viewGroup)
    {
        FindAllAndDisableTint(viewGroup);
    }

    public static void DisableIconTintingFromActivity()
    {
        var activity = Platform.CurrentActivity;
        if (activity?.Window?.DecorView is global::Android.Views.ViewGroup root)
        {
            FindAllAndDisableTint(root);
        }
    }

    private static void FindAllAndDisableTint(global::Android.Views.ViewGroup viewGroup)
    {
        for (int i = 0; i < viewGroup.ChildCount; i++)
        {
            var child = viewGroup.GetChildAt(i);

            if (child is BottomNavigationView bnv)
            {
                bnv.ItemIconTintList = null;
            }

            if (child is NavigationView nav)
            {
                nav.ItemIconTintList = null;
            }

            if (child is global::Android.Widget.ImageView imageView)
            {
                imageView.ImageTintList = null;
                imageView.SetColorFilter(null);
            }

            if (child is global::Android.Views.ViewGroup childGroup)
            {
                FindAllAndDisableTint(childGroup);
            }
        }
    }
}
