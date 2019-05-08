using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Winofy.Extensions
{
    public static class ViewExtension
    {
        internal class ExtIndicator : ActivityIndicator
        {
            public ExtIndicator()
            {
                VerticalOptions = LayoutOptions.Center;
                HorizontalOptions = LayoutOptions.Center;
            }
        }

        public static void ShowLoading(this Grid grid, bool center = true)
        {
            var ind = grid.GetExtensionIndicator() ?? new ExtIndicator();
            ind.IsRunning = true;

            if (center)
            {
                if (grid.ColumnDefinitions.Count > 0)
                    Grid.SetColumnSpan(ind, grid.ColumnDefinitions.Count);

                if (grid.RowDefinitions.Count > 0)
                    Grid.SetRowSpan(ind, grid.RowDefinitions.Count);
            }

            if (!grid.Children.Contains(ind))
            {
                grid.Children.Add(ind);
            }
        }

        public static void HideLoading(this Grid grid)
        {
            var ind = grid.GetExtensionIndicator() ?? new ExtIndicator();
            ind.IsRunning = false;

            if (!grid.Children.Contains(ind))
            {
                grid.Children.Add(ind);
            }
        }

        public static ActivityIndicator GetExtensionIndicator<T>(this IViewContainer<T> container) where T : View
        {
            foreach (var i in container.Children)
            {
                if (i is ExtIndicator ind)
                {
                    return ind;
                }
            }

            return null;
        }
    }
}
