﻿using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class ByteExtractor
{
    public int Position { get; private set; }
    
    private byte[] _data;
    
    public ByteExtractor(byte[] data)
    {
        _data = data;
    }
    
    public VecD GetVecD()
    {
        double x = BitConverter.ToDouble(_data, Position);
        double y = BitConverter.ToDouble(_data, Position + sizeof(double));
        
        Position += sizeof(double) * 2;
        
        return new VecD(x, y);
    }
    
    public Color GetColor()
    {
        byte r = _data[Position];
        byte g = _data[Position + 1];
        byte b = _data[Position + 2];
        byte a = _data[Position + 3];
        
        Position += 4;
        
        return new Color(r, g, b, a);
    }
    
    public int GetInt()
    {
        int value = BitConverter.ToInt32(_data, Position);
        
        Position += sizeof(int);
        
        return value;
    }

    public List<VecD> GetVecDList()
    {
        int count = GetInt();
        List<VecD> points = new List<VecD>();
        
        for (int i = 0; i < count; i++)
        {
            points.Add(GetVecD());
        }
        
        return points;
    }

    public Matrix3X3 GetMatrix3X3()
    {
        double[] values = new double[9];
        
        for (int i = 0; i < 9; i++)
        {
            values[i] = BitConverter.ToDouble(_data, Position);
            Position += sizeof(double);
        }
        
        float[] valuesFloat = values.Select(x => (float)x).ToArray();
        return new Matrix3X3(valuesFloat);
    }

    public double GetDouble()
    {
        double value = BitConverter.ToDouble(_data, Position);
        
        Position += sizeof(double);
        
        return value;
    }
}
