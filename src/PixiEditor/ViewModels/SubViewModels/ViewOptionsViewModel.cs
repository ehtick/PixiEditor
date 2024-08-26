﻿using Avalonia.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.ViewModels.SubViewModels;
#nullable enable
internal class ViewOptionsViewModel : SubViewModel<ViewModelMain>
{
    private bool gridLinesEnabled;

    public bool GridLinesEnabled
    {
        get => gridLinesEnabled;
        set => SetProperty(ref gridLinesEnabled, value);
    }

    public ViewOptionsViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    [Command.Basic("PixiEditor.View.ToggleGrid", "TOGGLE_GRIDLINES", "TOGGLE_GRIDLINES", Key = Key.OemTilde, Modifiers = KeyModifiers.Control,
        Icon = PixiPerfectIcons.Grid)]
    public void ToggleGridLines()
    {
        GridLinesEnabled = !GridLinesEnabled;
    }

    [Command.Basic("PixiEditor.View.ZoomIn", 1, "ZOOM_IN", "ZOOM_IN", CanExecute = "PixiEditor.HasDocument", Key = Key.OemPlus,
        Icon = PixiPerfectIcons.ZoomIn, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.View.Zoomout", -1, "ZOOM_OUT", "ZOOM_OUT", CanExecute = "PixiEditor.HasDocument", Key = Key.OemMinus,
        Icon = PixiPerfectIcons.ZoomOut, AnalyticsTrack = true)]
    public void ZoomViewport(double zoom)
    {
        ViewportWindowViewModel? viewport = Owner.WindowSubViewModel.ActiveWindow as ViewportWindowViewModel;
        if (viewport is null)
            return;
        viewport.ZoomViewportTrigger.Execute(this, zoom);
    }
}