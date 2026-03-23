using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Map
{
    /// <summary>
    /// 地形类型
    /// </summary>
    public enum TerrainType
    {
        Grass,      // 草地（默认，可通行）
        Dirt,       // 泥地（可通行）
        Stone,      // 石地（可通行）
        Water,      // 水域（不可通行，减速）
        Lava,       // 岩浆（不可通行，受伤）
    }

    /// <summary>
    /// 障碍物类型
    /// </summary>
    public enum ObstacleType
    {
        None,           // 无障碍
        Rock,           // 岩石（圆形碰撞）
        Tree,           // 树木（圆形碰撞）
        Wall,           // 墙壁（方形碰撞）
        Fence,          // 栅栏（方形碰撞）
        Bush,           // 灌木（圆形碰撞，可穿过）
    }

    /// <summary>
    /// 装饰物类型
    /// </summary>
    public enum DecorationType
    {
        None,
        GrassPatch,     // 草丛
        Flower,         // 花朵
        Pebble,         // 鹅卵石
        Crack,          // 地面裂纹
        Mushroom,       // 蘑菇
    }

    /// <summary>
    /// 单个地图块的运行时数据
    /// </summary>
    [System.Serializable]
    public class ChunkData
    {
        public Vector2Int chunkCoord;       // 块坐标
        public TerrainType terrain;         // 地形类型
        public ObstacleType[] obstacles;    // 该块内的障碍物
        public DecorationType[] decorations;// 该块内的装饰物
        public float[] obstaclePositionsX;
        public float[] obstaclePositionsY;
        public bool isGenerated;

        public ChunkData(Vector2Int coord)
        {
            chunkCoord = coord;
            terrain = TerrainType.Grass;
            obstacles = new ObstacleType[0];
            decorations = new DecorationType[0];
            obstaclePositionsX = new float[0];
            obstaclePositionsY = new float[0];
            isGenerated = false;
        }
    }
}
