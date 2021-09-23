using System;
using System.Collections.Generic;
using Recipes.Models;
using Recipes.ViewModels;
using Recipes.Views;
using Microsoft.Maui.Controls;

namespace Recipes
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(RecipeSearchPage), typeof(RecipeSearchPage));
            Routing.RegisterRoute(nameof(HitDetailPage), typeof(HitDetailPage));
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            Routing.RegisterRoute(nameof(EditItemPage), typeof(EditItemPage));
        }

        public static RecipeData Data { get; set; }

        public static Item[] MyRecipes { get; set; }

    }
}
