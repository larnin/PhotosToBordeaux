using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using NRand;

public class GeneratorLogic : MonoBehaviour
{
    enum TileType
    {
        Full = 0,
        Side = 1,
        SideDown = 2,
        Corner = 3,
        InterCorner = 4
    }

    [Serializable]
    class BiomeTiles
    {
        [SerializeField] List<GameObject> m_fullObj;
        [SerializeField] List<GameObject> m_sideObj;
        [SerializeField] List<GameObject> m_sideDownObj;
        [SerializeField] List<GameObject> m_cornerObj;
        [SerializeField] List<GameObject> m_interCornerObj;

        public List<GameObject> get(TileType t)
        {
            switch(t)
            {
                case TileType.Full:
                    return m_fullObj;
                case TileType.Side:
                    return m_sideObj;
                case TileType.SideDown:
                    return m_sideDownObj;
                case TileType.Corner:
                    return m_cornerObj;
                case TileType.InterCorner:
                    return m_interCornerObj;
                default:
                    return null;
            }
        }

        public GameObject getOne(TileType t, IRandomGenerator gen)
        {
            var biomes = get(t);
            if (biomes == null || biomes.Count == 0)
                return null;
            return biomes[new UniformIntDistribution(biomes.Count - 1).Next(gen)];
        }
    }

    [Serializable]
    class Biomes
    {
        [SerializeField] BiomeTiles m_seaTiles;
        [SerializeField] BiomeTiles m_waterTiles;
        [SerializeField] BiomeTiles m_sandTiles;
        [SerializeField] BiomeTiles m_landTiles;
        [SerializeField] BiomeTiles m_forestTiles;

        public BiomeTiles get(Biome b)
        {
            switch(b)
            {
                case Biome.Sea:
                    return m_seaTiles;
                case Biome.Water:
                    return m_waterTiles;
                case Biome.Sand:
                    return m_sandTiles;
                case Biome.Land:
                    return m_landTiles;
                case Biome.Forest:
                    return m_forestTiles;
                default:
                    return null;
            }
        }
    }

    [Serializable]
    class ImportantPoint
    {
        public string name;
        public GameObject prefab;
        public Biome biome;
    }

    [Serializable]
    class GeneratorProperties
    {
        public Transform zoneParent;
        public int width;
        public int height;
        public float perlinElevationScale;
        public float perlinElevationScale2;
        public float perlinElevationScale3;
        public float perlinElevationAmplitude2;
        public float perlinElevationAmplitude3;
        public float waterHeight;
        public float OceanHeight;
        public float perlinForestScale;
        public float perlinForestScale2;
        public float perlinForestAmplitude2;
        public float forestMinHeight;
        public float forestValue;
        public float perlinSandScale;
        public float sandMaxHeight;
        public float sandValue;
        public int importantPointCount;
        public int maxImportantPointsTry;
        public float minImportantPointDistance;
    }

    const float perlinRange = 1000000;

    [SerializeField] GeneratorProperties m_properties;
    [SerializeField] Biomes m_biomes;
    [SerializeField] List<ImportantPoint> m_importantPoints;
    [SerializeField] GameObject m_bordeauxPrefab;

    void Start()
    {
        generate((uint)new StaticRandomGenerator<DefaultRandomGenerator>().Next());
        Event<GenerationFinishedEvent>.Broadcast(new GenerationFinishedEvent());
    }
    
    void generate(uint seed)
    {
        MT19937 generator = new MT19937(seed);

        var elevation = generateElevation(generator);
        var tiles = generateBiomes(elevation, generator);

        draw(tiles, generator);

        LevelMap.instance.tiles = tiles;

        placeImportantPoints(generator);
        placeBordeaux(generator);
        placeSpawnPoint();
    }

    Vector2 generateOffset(float range, IRandomGenerator gen)
    {
        return new UniformVector2SquareDistribution(-range, range, -range, range).Next(gen);
    }

    Matrix<float> generateElevation(IRandomGenerator gen)
    {
        var perlinOffset = generateOffset(perlinRange, gen);
        var perlin2Offset = generateOffset(perlinRange, gen);
        var perlin3Offset = generateOffset(perlinRange, gen);

        Matrix<float> elevation = new Matrix<float>(m_properties.width, m_properties.height, 0);

        for(int i = 0; i < m_properties.width; i++)
            for(int j = 0; j < m_properties.height; j++)
            {
                elevation.set(i, j, Mathf.PerlinNoise(i * m_properties.perlinElevationScale + perlinOffset.x, j * m_properties.perlinElevationScale + perlinOffset.y) * 2 - 1
                                  + (Mathf.PerlinNoise(i * m_properties.perlinElevationScale2 + perlin2Offset.x, j * m_properties.perlinElevationScale2 + perlin2Offset.y) * 2 - 1) * m_properties.perlinElevationAmplitude2
                                  + (Mathf.PerlinNoise(i * m_properties.perlinElevationScale3 + perlin3Offset.x, j * m_properties.perlinElevationScale3 + perlin3Offset.y) * 2 - 1) * m_properties.perlinElevationAmplitude3);
            }
        return elevation;
    }

    Matrix<Biome> generateBiomes(Matrix<float> elevation, IRandomGenerator gen)
    {
        Matrix<Biome> tiles = new Matrix<Biome>(m_properties.width, m_properties.height, Biome.Sea);

        var perlinOffset = generateOffset(perlinRange, gen);
        var perlin2Offset = generateOffset(perlinRange, gen);
        var perlin3Offset = generateOffset(perlinRange, gen);

        for (int i = 0; i < m_properties.width; i++)
            for (int j = 0; j < m_properties.height; j++)
            {
                float height = elevation.get(i, j);
                float forestValue = Mathf.PerlinNoise(i * m_properties.perlinForestScale + perlinOffset.x, j * m_properties.perlinForestScale + perlinOffset.y) * 2 - 1
                                 + (Mathf.PerlinNoise(i * m_properties.perlinForestScale2 + perlin2Offset.x, j * m_properties.perlinForestScale2 + perlin2Offset.y) * 2 - 1) * m_properties.perlinForestAmplitude2;
                float sandValue = Mathf.PerlinNoise(i * m_properties.perlinSandScale + perlin3Offset.x, j * m_properties.perlinSandScale + perlin3Offset.y) * 2 - 1;

                if (height < m_properties.OceanHeight)
                    tiles.set(i, j, Biome.Sea);
                else if (height < m_properties.waterHeight)
                    tiles.set(i, j, Biome.Water);
                else if (height < m_properties.sandMaxHeight && sandValue > m_properties.sandValue)
                    tiles.set(i, j, Biome.Sand);
                else if (height > m_properties.forestMinHeight && forestValue > m_properties.forestValue)
                    tiles.set(i, j, Biome.Forest);
                else tiles.set(i, j, Biome.Land);
            }

        return tiles;
    }

    void drawPlaceHolder(Matrix<Biome> tiles, IRandomGenerator gen)
    {
        for (int i = 0; i < m_properties.width; i++)
            for (int j = 0; j < m_properties.height; j++)
            {
                var prefab = m_biomes.get(tiles.get(i, j)).getOne(TileType.Full, gen);
                var obj = Instantiate(prefab, m_properties.zoneParent);
                obj.transform.localPosition = new Vector3(i, 0, j);
            }
    }

    void draw(Matrix<Biome> tiles, IRandomGenerator gen)
    {
        for (int i = 0; i < m_properties.width; i++)
            for (int j = 0; j < m_properties.height; j++)
            {
                var local = tiles.getLocal(i, j, tiles.get(i, j));

                for(int b = 0; b <= (int) local.get(1, 1); b++)
                {
                    Biome biome = (Biome)b;
                    if (!local.have(biome))
                        continue;
                    drawCorner(i, j, local.get(0, 1), local.get(1, 0), local.get(0, 0), biome, 0, gen);
                    drawCorner(i, j, local.get(1, 0), local.get(2, 1), local.get(2, 0), biome, -90, gen);
                    drawCorner(i, j, local.get(2, 1), local.get(1, 2), local.get(2, 2), biome, -180, gen);
                    drawCorner(i, j, local.get(1, 2), local.get(0, 1), local.get(0, 2), biome, -270, gen);
                }
            }
    }
    
    void drawCorner(int x, int y, Biome left, Biome top, Biome topLeft, Biome current, float rot, IRandomGenerator gen)
    {
        TileType type = TileType.Full;

        if (left >= current && top >= current && topLeft >= current)
            type = TileType.Full;
        else if (left >= current && top >= current)
            type = TileType.InterCorner;
        else if (top >= current)
            type = TileType.Side;
        else if (left >= current)
            type = TileType.SideDown;
        else type = TileType.Corner;

        var prefab = m_biomes.get(current).getOne(type, gen);
        var obj = Instantiate(prefab, m_properties.zoneParent);
        obj.transform.localPosition = new Vector3(2*x, 0, 2*y);
        obj.transform.Rotate(new Vector3(0, rot));
    }

    void placeImportantPoints(IRandomGenerator gen)
    {
        var tiles = LevelMap.instance.tiles;
        LevelMap.instance.importantPoints.Clear();

        var dx = new UniformIntDistribution(2, m_properties.width - 3);
        var dy = new UniformIntDistribution(2, m_properties.height - 3);
        var dPoint = new UniformIntDistribution(m_importantPoints.Count - 1);
        var dRot = new UniformIntDistribution(3);
        
        for(int i = 0; i < m_properties.importantPointCount; i++)
        {
            for(int j = 0; j < m_properties.maxImportantPointsTry; j++)
            {
                var x = dx.Next(gen);
                var y = dy.Next(gen);
                bool ok = true;
                foreach(var p in LevelMap.instance.importantPoints)
                {
                    if (new Vector2(x - p.x, y - p.y).magnitude > m_properties.minImportantPointDistance)
                        continue;

                    ok = false;
                    break;
                }
                if (!ok)
                    continue;

                var localMap = tiles.getLocal(x, y);
                if (localMap.get(0, 0) != localMap.get(0, 1) || localMap.get(0, 0) != localMap.get(0, 1) ||
                   localMap.get(0, 0) != localMap.get(0, 1) || localMap.get(0, 0) != localMap.get(0, 1) || localMap.get(0, 0) != localMap.get(0, 1) ||
                   localMap.get(0, 0) != localMap.get(0, 1) || localMap.get(0, 0) != localMap.get(0, 1) || localMap.get(0, 0) != localMap.get(0, 1))
                    continue;

                var point = m_importantPoints[dPoint.Next(gen)];
                if (point.biome != localMap.get(0, 0))
                    continue;

                var obj = Instantiate(point.prefab, m_properties.zoneParent);
                obj.transform.localPosition = new Vector3(2 * x, 0, 2 * y);
                obj.transform.localRotation = Quaternion.Euler(0, dRot.Next(gen) * 90, 0);

                ImportantPointInfos iP = new ImportantPointInfos();
                iP.x = x;
                iP.y = y;
                iP.name = point.name;
                LevelMap.instance.importantPoints.Add(iP);

                break;
            }
        }
    }

    void placeBordeaux(IRandomGenerator gen)
    {
        var tiles = LevelMap.instance.tiles;

        var dx = new UniformIntDistribution(2, m_properties.width - 3);
        var dy = new UniformIntDistribution(2, m_properties.height - 3);
        var dRot = new UniformIntDistribution(3);

        bool bdxSet = false;
        for (int j = 0; j < m_properties.maxImportantPointsTry; j++)
        {
            var x = dx.Next(gen);
            var y = dy.Next(gen);
            bool ok = true;
            foreach (var p in LevelMap.instance.importantPoints)
            {
                if (new Vector2(x - p.x, y - p.y).magnitude > m_properties.minImportantPointDistance)
                    continue;

                ok = false;
                break;
            }
            if (!ok)
                continue;

            var localMap = tiles.getLocal(x, y);
            if (localMap.get(0, 0) != localMap.get(0, 1) || localMap.get(0, 0) != localMap.get(0, 2) ||
                localMap.get(0, 0) != localMap.get(1, 0) || localMap.get(0, 0) != localMap.get(1, 1) || localMap.get(1, 2) != localMap.get(0, 1) ||
                localMap.get(0, 0) != localMap.get(2, 0) || localMap.get(0, 0) != localMap.get(2, 1) || localMap.get(2, 2) != localMap.get(0, 1))
                continue;
            
            if (localMap.get(0, 0) != Biome.Land)
                continue;

            var obj = Instantiate(m_bordeauxPrefab, m_properties.zoneParent);
            obj.transform.localPosition = new Vector3(2 * x, 0, 2 * y);
            obj.transform.localRotation = Quaternion.Euler(0, dRot.Next(gen) * 90, 0);

            ImportantPointInfos iP = new ImportantPointInfos();
            iP.x = x;
            iP.y = y;
            iP.name = "bordeaux";
            LevelMap.instance.bordeaux = iP;
            bdxSet = true;

            break;
        }

        if(!bdxSet)
        {
            var x = dx.Next(gen);
            var y = dy.Next(gen);

            var obj = Instantiate(m_bordeauxPrefab, m_properties.zoneParent);
            obj.transform.localPosition = new Vector3(2 * x, 0, 2 * y);
            obj.transform.localRotation = Quaternion.Euler(0, dRot.Next(gen) * 90, 0);

            ImportantPointInfos iP = new ImportantPointInfos();
            iP.x = x;
            iP.y = y;
            iP.name = "bordeaux";
            LevelMap.instance.bordeaux = iP;
        }
    }

    void placeSpawnPoint()
    {
        const float offset = 3;
        Vector2[] points = new Vector2[] 
        { new Vector2(3, 3)
        , new Vector2(offset, m_properties.height - offset - 1)
        , new Vector2(m_properties.width - offset - 1, offset)
        , new Vector2(m_properties.width - offset - 1, m_properties.height - offset - 1)};

        var bestPos = new Vector2(LevelMap.instance.bordeaux.x, LevelMap.instance.bordeaux.y);
        var bdxPos = bestPos;
        foreach(var p in points)
            if ((bestPos - bdxPos).sqrMagnitude < (p - bdxPos).sqrMagnitude)
                bestPos = p;

        LevelMap.instance.startPos = bestPos;

        Vector2 dir = new Vector2(m_properties.width / 2, m_properties.height / 2) - bestPos;
        LevelMap.instance.startRotation = - Vector2.SignedAngle(new Vector2(0, 1), dir);
    }
}
