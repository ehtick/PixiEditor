﻿using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using PixiEditor.Helpers.Behaviours;

namespace PixiEditor.Views.Dock;

public partial class ColorPickerDockView : UserControl
{
    public ColorPickerDockView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var textBoxes = this.GetVisualDescendants().OfType<TextBox>().ToArray();

        foreach (var textBox in textBoxes)
        {
            var existingBehaviors = Interaction.GetBehaviors(textBox);
            if(existingBehaviors.Any(x => x is GlobalShortcutFocusBehavior)) continue;
            bool attach = false;
            if (existingBehaviors == null)
            {
                attach = true;
                existingBehaviors = new BehaviorCollection();
            }

            existingBehaviors.Add(new GlobalShortcutFocusBehavior());

            if (attach)
            {
                Interaction.SetBehaviors(textBox, existingBehaviors);
            }
        }
    }
}