﻿using Drawie.Backend.Core.Surfaces.Vector;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

public record class Selection_ChangeInfo(VectorPath NewPath) : IChangeInfo;
