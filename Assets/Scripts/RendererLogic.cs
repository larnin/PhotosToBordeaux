using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using NRand;
using System.Linq;

public class RendererLogic : MonoBehaviour
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
    class TileOffset
    {
        public Vector3 offset;
        public Quaternion rotation;
        public Vector3 scale;

        public Matrix4x4 toMat(TileOffset add, Vector3 finalScale)
        {
            return Matrix4x4.Scale(finalScale) * Matrix4x4.Translate(offset) * Matrix4x4.Rotate(rotation) * Matrix4x4.Translate(add.offset) * Matrix4x4.Rotate(add.rotation) * Matrix4x4.Scale(add.scale);
        }
    }

    [Serializable]
    class TileInfo
    {
        public Mesh mesh;
        public Material material;
        public TileOffset offset;
    }

    [Serializable]
    class BiomeTiles
    {
        [SerializeField] List<TileInfo> m_fullObj;
        [SerializeField] List<TileInfo> m_sideObj;
        [SerializeField] List<TileInfo> m_sideDownObj;
        [SerializeField] List<TileInfo> m_cornerObj;
        [SerializeField] List<TileInfo> m_interCornerObj;

        public List<TileInfo> get(TileType t)
        {
            switch (t)
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

        public TileInfo getOne(TileType t, IRandomGenerator gen)
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
            switch (b)
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
    class RendererInfos
    {
        public Mesh mesh;
        public Material material;
        public Matrix4x4[] batch;
    }
    
    [SerializeField] Biomes m_biomes;
    [SerializeField] Vector3 m_generalScale;
    List<RendererInfos> m_renderers = new List<RendererInfos>();
    SubscriberList m_subscriberList = new SubscriberList();

    bool m_generated = false;

    private void Awake()
    {
        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(onGenerationEnd));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Update()
    {
        foreach (var batch in m_renderers)
            Graphics.DrawMeshInstanced(batch.mesh, 0, batch.material, batch.batch);
    }

    class TileGenData
    {
        public TileInfo tile;
        public List<TileOffset> batch = new List<TileOffset>();
    }

    void onGenerationEnd(GenerationFinishedEvent e)
    {
        if (!LevelMap.instance.canUseInstancing)
            return;

        var gen = new StaticRandomGenerator<DefaultRandomGenerator>();
        var genData = new List<TileGenData>();

        var tiles = LevelMap.instance.tiles;

        for(int i = 0; i < tiles.width; i++)
            for(int j = 0; j < tiles.height; j++)
            {
                var local = tiles.getLocal(i, j, tiles.get(i, j));

                for (int b = 0; b <= (int)local.get(1, 1); b++)
                {
                    Biome biome = (Biome)b;
                    if (!local.have(biome))
                        continue;
                    drawCorner(i, j, local.get(0, 1), local.get(1, 0), local.get(0, 0), biome, 0, gen, genData);
                    drawCorner(i, j, local.get(1, 0), local.get(2, 1), local.get(2, 0), biome, -90, gen, genData);
                    drawCorner(i, j, local.get(2, 1), local.get(1, 2), local.get(2, 2), biome, -180, gen, genData);
                    drawCorner(i, j, local.get(1, 2), local.get(0, 1), local.get(0, 2), biome, -270, gen, genData);
                }
            }

        createBatch(genData);
    }

    void drawCorner(int x, int y, Biome left, Biome top, Biome topLeft, Biome current, float rot, IRandomGenerator gen, List<TileGenData> genData)
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
        
        var obj = m_biomes.get(current).getOne(type, gen);
        
        var data = genData.Find(value => { return value.tile == obj; });
        if(data == null)
        {
            data = new TileGenData();
            data.tile = obj;
            genData.Add(data);
        }
        var tile = new TileOffset();
        tile.offset = new Vector3(2*x, 0, 2*y);
        tile.rotation = Quaternion.Euler(0, rot, 0);
        tile.scale = Vector3.one;
        data.batch.Add(tile);
    }

    void createBatch(List<TileGenData> genData)
    {
        const int batchMax = 1023;
        m_renderers.Clear();

        foreach(var data in genData)
        {
            if(data.batch.Count <= batchMax)
            {
                var infos = new RendererInfos();
                infos.mesh = data.tile.mesh;
                infos.material = data.tile.material;
                infos.batch = data.batch.Select(x => x.toMat(data.tile.offset, m_generalScale)).ToArray();
                m_renderers.Add(infos);
            }
            else
            {
                int count = data.batch.Count;
                for(int i = 0; i < count; i+= 1023)
                {
                    var infos = new RendererInfos();
                    infos.mesh = data.tile.mesh;
                    infos.material = data.tile.material;

                    if (i + batchMax < count)
                    {
                        infos.batch = new Matrix4x4[batchMax];
                        for (int ii = 0; ii < batchMax; ii++)
                            infos.batch[ii] = data.batch[i + ii].toMat(data.tile.offset, m_generalScale);
                    }
                    else
                    {
                        infos.batch = new Matrix4x4[count - i];
                        for(int ii = 0; ii < count - i; ii++)
                            infos.batch[ii] = data.batch[i + ii].toMat(data.tile.offset, m_generalScale);
                    }
                    m_renderers.Add(infos);
                }
            }
        }
    }
}
