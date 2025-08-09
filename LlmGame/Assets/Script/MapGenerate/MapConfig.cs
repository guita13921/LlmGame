using System.Collections.Generic;
using OneLine;
using UnityEngine;

namespace Map
{
    [CreateAssetMenu]
    public class MapConfig : ScriptableObject
    {
        public List<NodeBlueprint> nodeBlueprints;
        [Tooltip("Nodes that will be used on layers with Randomize Nodes > 0")]
        public List<NodeType> randomNodes = new List<NodeType>
            {NodeType.Mystery, NodeType.Store, NodeType.Treasure, NodeType.MinorEnemy, NodeType.RestSite};
        public int GridWidth => Mathf.Max(numOfPreBossNodes.max, numOfStartingNodes.max);

        [OneLineWithHeader]
        public IntMinMax numOfPreBossNodes;
        [OneLineWithHeader]
        public IntMinMax numOfStartingNodes;

        [Tooltip("Increase this number to generate more paths")]
        public int extraPaths;

        [Header("Enemy Difficulty Distribution")]
        [Range(0,100)] public int minorEasyPercent = 30;
        [Range(0,100)] public int minorNormalPercent = 30;
        [Range(0,100)] public int minorHardPercent = 40;
        [Range(0,100)] public int eliteEasyPercent = 50;
        [Range(0,100)] public int eliteHardPercent = 50;

        public List<MapLayer> layers;
    }
}