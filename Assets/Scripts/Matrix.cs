using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Matrix<T>
{
    List<T> m_values = new List<T>();
    public int width { get; private set; }
    public int height { get; private set; }

    public Matrix(int _width, int _height, T value = default(T))
    {
        for (int i = 0; i < _width; i++)
            for (int j = 0; j < _height; j++)
                m_values.Add(value);

        width = _width;
        height = _height;
    }

    public T get(int x, int y)
    {
        return m_values[x + y * width];
    }

    public void set(int x, int y, T value)
    {
        m_values[x + y * width] = value;
    }

    public Matrix<T> getLocal(int x, int y, T value = default(T))
    {
        Matrix<T> mat = new Matrix<T>(3, 3, value);

        for(int i = -1; i <= 1; i++)
            for(int j = -1; j <= 1; j++)
            {
                if (x + i < 0 || y + j < 0 || x + i >= width || y + j >= height)
                    continue;
                mat.set(i + 1, j + 1, get(x + i, y + j));
            }

        return mat;
    }

    public bool have(T value)
    {
        foreach (var it in m_values)
            if (EqualityComparer<T>.Default.Equals(it, value))
                return true;
        return false;
    }
}
