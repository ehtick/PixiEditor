﻿using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class ChangeBrightness_UpdateableChange : UpdateableChange
{
    private readonly Guid layerGuid;
    private readonly float correctionFactor;
    private readonly int strokeWidth;
    private readonly List<VecI> positions = new();
    private readonly bool repeat;
    private int frame;
    private int lastAppliedPointIndex = -1;
    private bool squareBrush;

    private List<VecI> ellipseLines;
    private RectI squareRect;

    private CommittedChunkStorage? savedChunks;

    [GenerateUpdateableChangeActions]
    public ChangeBrightness_UpdateableChange(Guid layerGuid, VecI pos, float correctionFactor, int strokeWidth,
        bool square,
        bool repeat, int frame)
    {
        this.layerGuid = layerGuid;
        this.correctionFactor = correctionFactor;
        this.strokeWidth = strokeWidth;
        this.repeat = repeat;
        this.frame = frame;
        this.squareBrush = square;
        positions.Add(pos);

        squareRect = new RectI(0, 0, strokeWidth, strokeWidth);
        if (!square)
        {
            ellipseLines =
                EllipseHelper.SplitEllipseIntoLines(
                    (EllipseHelper.GenerateEllipseFromRect(squareRect, 0)));
        }
    }

    [UpdateChangeMethod]
    public void Update(VecI pos)
    {
        if (positions.Count > 0)
        {
            var bresenham = BresenhamLineHelper.GetBresenhamLine(positions[^1], pos);
            positions.AddRange(bresenham);
        }
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, layerGuid, false))
            return false;
        ImageLayerNode node = target.FindMemberOrThrow<ImageLayerNode>(layerGuid);
        var layerImage = node.GetLayerImageAtFrame(frame);
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, layerImage, layerGuid, false);
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        ImageLayerNode node = target.FindMemberOrThrow<ImageLayerNode>(layerGuid);

        var layerImage = node.GetLayerImageAtFrame(frame);
        int queueLength = layerImage.QueueLength;

        for (int i = Math.Max(lastAppliedPointIndex, 0); i < positions.Count; i++)
        {
            VecI pos = positions[i];
            if (squareBrush)
            {
                ChangeBrightness(squareRect, pos + new VecI(-strokeWidth / 2), correctionFactor, repeat,
                    layerImage);
            }
            else
            {
                ChangeBrightness(ellipseLines, pos + new VecI(-strokeWidth / 2), correctionFactor, repeat,
                    layerImage);
            }
        }

        var affected = layerImage.FindAffectedArea(queueLength);

        lastAppliedPointIndex = positions.Count - 1;

        return new LayerImageArea_ChangeInfo(layerGuid, affected);
    }

    private static void ChangeBrightness(
        List<VecI> circleLines, VecI offset, float correctionFactor, bool repeat,
        ChunkyImage layerImage)
    {
        for (var i = 0; i < circleLines.Count - 1; i++)
        {
            VecI left = circleLines[i];
            VecI right = circleLines[i + 1];
            int y = left.Y;

            for (VecI pos = new VecI(left.X, y); pos.X <= right.X; pos.X++)
            {
                layerImage.EnqueueDrawPixel(
                    pos + offset,
                    (commitedColor, upToDateColor) =>
                    {
                        Color newColor = ColorHelper.ChangeColorBrightness(repeat ? upToDateColor : commitedColor,
                            correctionFactor);
                        return ColorHelper.ChangeColorBrightness(newColor, correctionFactor);
                    },
                    BlendMode.Src);
            }
        }
    }

    private static void ChangeBrightness(
        RectI square, VecI offset, float correctionFactor, bool repeat,
        ChunkyImage layerImage)
    {
        for (int i = square.X; i < square.X + square.Width; i++)
        {
            for (int j = square.Y; j < square.Y + square.Height; j++)
            {
                layerImage.EnqueueDrawPixel(
                    new VecI(i, j) + offset,
                    (commitedColor, upToDateColor) =>
                    {
                        Color newColor = ColorHelper.ChangeColorBrightness(repeat ? upToDateColor : commitedColor,
                            correctionFactor);
                        return ColorHelper.ChangeColorBrightness(newColor, correctionFactor);
                    },
                    BlendMode.Src);
            }
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        var layer = target.FindMemberOrThrow<ImageLayerNode>(layerGuid);
        ignoreInUndo = false;

        if (savedChunks is not null)
            throw new InvalidOperationException("Trying to apply while there are saved chunks");

        var layerImage = layer.GetLayerImageAtFrame(frame);

        if (!firstApply)
        {
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, layerImage, layerGuid, false);
            foreach (VecI pos in positions)
            {
                if (squareBrush)
                {
                    ChangeBrightness(squareRect, pos + new VecI(-strokeWidth / 2), correctionFactor, repeat,
                        layerImage);
                }
                else
                {
                    ChangeBrightness(ellipseLines, pos + new VecI(-strokeWidth / 2), correctionFactor, repeat,
                        layerImage);
                }
            }
        }

        var affArea = layerImage.FindAffectedArea();
        savedChunks = new CommittedChunkStorage(layerImage, affArea.Chunks);
        layerImage.CommitChanges();
        if (firstApply)
            return new None();
        return new LayerImageArea_ChangeInfo(layerGuid, affArea);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected =
            DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, layerGuid, false, frame, ref savedChunks);
        return new LayerImageArea_ChangeInfo(layerGuid, affected);
    }
}
