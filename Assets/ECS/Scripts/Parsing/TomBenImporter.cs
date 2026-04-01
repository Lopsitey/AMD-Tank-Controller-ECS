using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.AssetImporters;
#endif

// The version number is important for Unity to know when to reimport assets.
// Changing this automatically triggers a reimport of all assets of this type.
namespace wave_spawning
{
#if UNITY_EDITOR
    [ScriptedImporter(1, "TomBen")] 
    public class TomBenImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            // Stores the file text using the asset path provided by the context.
            string fileContent = File.ReadAllText(ctx.assetPath);

            // Creates the ScriptableObject asset
            // ReSharper disable once InconsistentNaming
            var waveSO = ScriptableObject.CreateInstance<TomBenWaveData>();
            
            // The parser which reads the file and extracts the relevant data 
            TomBenBlockParser parser = new TomBenBlockParser();
            var parsedBlocks = parser.ParseBlocksFromFile(fileContent);
            
            // For passing the data from the parser to the SO
            List<UnitAuthoring> units = new List<UnitAuthoring>();
            List<ClusterAuthoring> clusters = new List<ClusterAuthoring>();
            List<WaveAuthoring> waves = new List<WaveAuthoring>();

            // Iterates through the blocks using their headers to decide what to do with them
            foreach (var block in parsedBlocks)
            {
                var header = parser.GetBlockHeader(block);

                //If it's a unit block then parse it as one
                if (header.m_Type == ParsedBlockType.UnitType)
                    units.Add(parser.ParseUnitBlock(block, header));
            
                else if (header.m_Type == ParsedBlockType.Cluster)
                    clusters.Add(parser.ParseClusterBlock(block, header));
            
                else if (header.m_Type == ParsedBlockType.Wave)
                    waves.Add(parser.ParseWaveBlock(block, header)); 
            }

            // Assign the lists to the SO
            waveSO.Units = units.ToArray();
            waveSO.Clusters = clusters.ToArray();
            waveSO.Waves = waves.ToArray();
            
            // This is important as it essentially saves the asset to disk
            // It can also be used to add sub assets to the main asset
            ctx.AddObjectToAsset("root", waveSO);
            // The string parameter us tge name of the object being added to the asset - doesn't show unless you have multiple objs 
            // This sets the main object of the asset to be the scriptable object variable
            // Again, more obviously useful if adding multiple objects to the main asset but needed anyway for the editor to recognise it
            ctx.SetMainObject(waveSO);
        }
    }
#endif
}