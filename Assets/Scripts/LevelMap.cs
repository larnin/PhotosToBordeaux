using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum Biome
{
    Sea = 0,
    Water = 1,
    Sand = 2,
    Land = 3,
    Forest = 4
}

public class ImportantPointInfos
{
    public int x;
    public int y;
    public string name;
}

public class LevelMap
{
    static LevelMap m_instance;
    public static LevelMap instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new LevelMap();
            return m_instance;
        }
    }

    public Matrix<Biome> tiles = new Matrix<Biome>(1, 1, Biome.Sea);
    public List<ImportantPointInfos> importantPoints = new List<ImportantPointInfos>();
    public ImportantPointInfos bordeaux = new ImportantPointInfos();
    public Vector2 startPos;
    public float startRotation;
    public Texture2D minimap;
    public float time;
    public int seed;
}
