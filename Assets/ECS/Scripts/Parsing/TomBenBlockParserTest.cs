using System.Collections.Generic;
using System.IO;
using ECS.wave_spawning;
using UnityEngine;

namespace wave_spawning
{
    /// <summary>
    /// Sets up the parser and tests it on a file.
    /// </summary>
    public class TomBenBlockParserTest : MonoBehaviour
    {
        [SerializeField] private string m_FilePath;
        private List<ParsedBlock> m_ParsedBlocks;

        [SerializeField] private List<UnitAuthoring> m_ParsedUnits;
        [SerializeField] private List<ClusterAuthoring> m_ParsedClusters;
        [SerializeField] private List<WaveAuthoring> m_ParsedWaves;

        private void Awake()
        {
            // Defensive programming to ensure the file exists
            if (!File.Exists(m_FilePath))
                Debug.LogError($"File at path {m_FilePath} does not exist! Please set a valid file path.");

            // Stores the file text
            string fileText = File.ReadAllText(m_FilePath);

            // Creates a new parser and passes in the file text
            TomBenBlockParser parser = new TomBenBlockParser();
            m_ParsedBlocks = parser.ParseBlocksFromFile(fileText);

            foreach (ParsedBlock block in m_ParsedBlocks)
            {
                ParsedBlockHeader header = parser.GetBlockHeader(block);
                //If it's a unit block then parse it as one
                if (header.m_Type == ParsedBlockType.UnitType)
                    m_ParsedUnits.Add(parser.ParseUnitBlock(block, header));
                else if (header.m_Type == ParsedBlockType.Cluster)
                    m_ParsedClusters.Add(parser.ParseClusterBlock(block, header));
                else if (header.m_Type == ParsedBlockType.Wave)
                    m_ParsedWaves.Add(parser.ParseWaveBlock(block, header));

                Debug.Log($"<b>{header.m_RawText}</b>");
                Debug.Log($"\t Type: {header.m_Type}");
                Debug.Log($"\t ID: {header.m_ID}");
                Debug.Log($"\t Name: {header.m_Name}");
            }
        }
    }
}