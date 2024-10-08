﻿using ChunkyImageLib;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Clipboard;

public record struct DataImage
{
    public string? Name { get; set; }
    public Surface Image { get; set; }
    public VecI Position { get; set; }
    
    public DataImage(Surface image, VecI position) : this(null, image, position) { }

    public DataImage(string? name, Surface image, VecI position)
    {
        Name = name;
        Image = image;
        Position = position;
    }
}
