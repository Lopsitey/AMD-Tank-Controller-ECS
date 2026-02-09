#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ECS.wave_spawning;
using UnityEngine;

#endregion

namespace wave_spawning
{
    [Serializable]
    public class ParsedBlock
    {
        public string m_Header;
        public string m_Body;
    }

    /// <summary>
    /// Actually does the parsing.
    /// </summary>
    public class TomBenBlockParser
    {
        // The current state of the block parser - e.g. in a header, in a body, or not even in a block at all.
        private BlockParserState m_State;

        // The buffer of text currently being read - the text at the end of the buffer is always the most relevant
        // For example, if it ended with _ben then you would stop reading that block, etc...
        private string m_Buffer;

        private void ClearBuffer() => m_Buffer = string.Empty;

        private void ChangeState(BlockParserState newState)
        {
            m_State = newState;
            ClearBuffer();
        }

        /// <summary>
        /// Checks whether the buffer ends with a specific string. This is used to determine when to change state in the block parser.
        /// </summary>
        /// <param name="str">The string to be compared with the end of the buffer.</param>
        /// <returns></returns>
        private bool BufferEndsWith(string str) => m_Buffer.EndsWith(str);

        /// <summary>
        /// Checks whether the buffer ends with any of a given set of strings. This is used to determine when to change state in the block parser.
        /// </summary>
        /// <param name="tokens">The params keyword here allows for multiple separate string arguments. These are then converted into a string array.</param>
        /// <returns>A tuple - (true, the token it ended with), if successful, (false, null) if none were found.</returns>
        private (bool, string) BufferEndsWithAny(params string[] tokens)
        {
            foreach (var token in tokens)
            {
                if (BufferEndsWith(token))
                    return (true, token);
            }

            //Todo: Potentially use the out keyword here instead of a tuple - could improve readability depending on usage.
            return (false, null);
        }

        private void ResetParser()
        {
            // Outside a block by default
            m_State = BlockParserState.OutsideBlock;
            ClearBuffer();
        }

        /// <summary>
        /// Removes the amount of characters in the end param from the text.
        /// </summary>
        /// <param name="text">The text to be trimmed.</param>
        /// <param name="end">The amount of characters to be removed from the text.</param>
        /// <returns>A substring with the last end.Length characters removed.</returns>
        private string TrimFromEnd(string text, string end) => text[..^end.Length];
        // The "0.." syntax starts at the beginning of the string.
        // ^end.Length means “end.Length characters from the end”.
        // Could also be done with text.Substring(0, text.Length - end.Length)

        private void HandleOutsideBlockState(char c, ref ParsedBlock tempBlock)
        {
            //Matches against the end of the buffer
            (bool matched, string token) = BufferEndsWithAny("wave", "cluster", "type");

            //Continues if there is no match
            if (!matched) return;

            //Switch to the relevant state
            ChangeState(BlockParserState.InsideHeader);

            // Just cleared the buffer add the matched token back in to be stored for later
            m_Buffer += token;
        }

        private void HandleInsideHeaderState(char c, ref ParsedBlock tempBlock)
        {
            //If the Tom brace hasn't been hit then you're not at the end of the header yet
            if (!BufferEndsWith("_Tom")) return;

            //Otherwise get the entire header, excluding the _Tom at the end
            string header = TrimFromEnd(m_Buffer, "_Tom");

            //Set the current block's header to this
            tempBlock.m_Header = header;

            //Change to the body state
            ChangeState(BlockParserState.InsideBody);
        }

        /// <summary>
        /// Reads the body of the block until it hits the _Ben token
        /// </summary>
        /// <param name="c"></param>
        /// <param name="tempBlock"></param>
        /// <returns>True when the body has been read - essentially meaning the entire block has been read.</returns>
        private bool HandleInsideBodyState(char c, ref ParsedBlock tempBlock)
        {
            //If the Ben brace hasn't been hit then you're not at the end of the body yet
            if (!BufferEndsWith("_Ben")) return false;

            //Otherwise get the entire body, excluding the _ben at the end
            string body = TrimFromEnd(m_Buffer, "_ben");

            //Set the current block's body to this
            tempBlock.m_Body = body;

            //Change back to the outside block state, ready for the next block
            ChangeState(BlockParserState.OutsideBlock);

            return true;
        }

        /// <summary>
        /// This takes in a block header and parses it into a ParsedBlockHeader object, which contains the type, id and name of the block.
        /// </summary>
        /// <param name="block">The block to be parsed</param>
        /// <returns></returns>
        /// <exception cref="UnityException">If the header was corrupt or invalid</exception>
        public ParsedBlockHeader GetBlockHeader(in ParsedBlock block)
        {
            //Matches all subgroups using the regex pattern to match the data, for more info go to regexer.com
            var match = Regex.Match(block.m_Header,
                @"(type|cluster|wave)\s*\-\s*(\d+)\s*(?:\((.+)\))?");

            //If the match fails then the header is invalid
            if (!match.Success)
                throw new UnityException("Invalid block header");

            // Instantiates a ParsedBlockHeader with the relevant data
            // The type and id are taken from the regex groups
            return new ParsedBlockHeader
            {
                // This stores the entire header text
                m_RawText = block.m_Header,
                m_Type = GetBlockTypeFromString(match.Groups[1].Value),
                m_ID = int.Parse(match.Groups[2].Value),
                // The name is null if the name doesn't exist as it is optional
                m_Name = match.Groups[3].Success ? match.Groups[3].Value : null
            };
        }

        /// <summary>
        /// This converts the type string from the header into a ParsedBlockType enum
        /// </summary>
        /// <param name="type">The type to be converted</param>
        /// <returns>The converted type</returns>
        /// <exception cref="UnityException">An error if the type did not match any of the enum types</exception>
        public ParsedBlockType GetBlockTypeFromString(string type) => type switch
        {
            "wave" => ParsedBlockType.Wave,
            "cluster" => ParsedBlockType.Cluster,
            "type" => ParsedBlockType.UnitType,
            //This is essentially the default case for the switch statement - if the type did not match any of the above then it is invalid
            _ => throw new UnityException($"Invalid block type {type}")
        };

        /// <summary>
        /// This parses the block data into a unit authoring object.
        /// It does this by using the ID and name from the header and splitting the body to fill out the rest of the data.
        /// </summary>
        /// <param name="block">The current block being parsed.</param>
        /// <param name="header">The header of the block being parsed.</param>
        /// <returns>The unit with the parsed data applied.</returns>
        public UnitAuthoring ParseUnitBlock(in ParsedBlock block, in ParsedBlockHeader header)
        {
            //Instantiate a new UnitAuthoring to be filled with the parsed data
            UnitAuthoring unit = new UnitAuthoring
            {
                id = header.m_ID,
                name = header.m_Name
            };

            // Gets all the rules from the body and splits them by interrobang
            var rules = block.m_Body.Split("!?");

            // Iterates through every rule, splitting them by the => to get the key and value
            foreach (var rule in rules)
            {
                // If there is no rule after splitting then skip
                if (rule.Trim().Length <= 0)
                    continue;

                // Rules are in the key value format so should always have a length of 2
                string[] splitRule = rule.Trim().Split("=>");
                Debug.Assert(splitRule.Length == 2);

                // Separates the key and value
                string key = splitRule[0];
                string value = splitRule[1];

                // Assigns the relevant values using the key
                if (key == "damage") unit.damage = float.Parse(value);
                else if (key == "speed") unit.speed = float.Parse(value);
                else if (key == "health") unit.health = float.Parse(value);
                //Errors if the key is weird
                else Debug.LogWarning($"Unknown key {key} in unit block with id {header.m_ID}");
            }

            return unit;
        }

        /// <summary>
        /// Works essentially the same as the unit block parser but with a for loop to assign values to the cluster array rather than individual values.
        /// </summary>
        /// <param name="block">The current block being parsed.</param>
        /// <param name="header">The header of the block being parsed.</param>
        /// <returns>The cluster containing the array of parsed rules.</returns>
        public ClusterAuthoring ParseClusterBlock(in ParsedBlock block, in ParsedBlockHeader header)
        {
            var rules = block.m_Body.Split("!?");

            //Could be an array but a list works better in a foreach and means that you don't have to mess with the size before usage
            List<ClusterRuleAuthoring> validRules = new List<ClusterRuleAuthoring>();

            //Using a for loop here so the index can be used to assign the rules to the correct place in the array in the cluster
            foreach (var rule in rules)
            {
                if (rule.Trim().Length <= 0)
                    continue;

                string[] splitRule = rule.Trim().Split(":");
                Debug.Assert(splitRule.Length == 2,
                    $"Key value pair unexpected length in cluster block with id {header.m_ID}");

                string key = splitRule[0];
                string value = splitRule[1];

                var newRule = new ClusterRuleAuthoring()
                {
                    unitId = int.Parse(key),
                    amount = int.Parse(value)
                };

                validRules.Add(newRule);
            }

            return new ClusterAuthoring
            {
                id = header.m_ID,
                name = header.m_Name,
                clusterRules = validRules.ToArray()
            };
        }

        public WaveAuthoring ParseWaveBlock(in ParsedBlock block, in ParsedBlockHeader header)
        {
            WaveAuthoring wave = new WaveAuthoring
            {
                id = header.m_ID,
                name = header.m_Name
            };

            //Split, Trim, and Filter empty strings
            string[] ruleStrings = block.m_Body.Split("!?");
            List<string> validRuleStrings = new List<string>();
            foreach (var rule in ruleStrings)
            {
                var trim = rule.Trim();
                if (trim.Length <= 0) continue;
                validRuleStrings.Add(trim);
            }
            ruleStrings = validRuleStrings.ToArray();
            Debug.Assert(ruleStrings.Length <= 0, $"No valid rules found in wave block with ID: {header.m_ID}");
            
            wave.waveRules = new WaveRuleAuthoring[ruleStrings.Length];

            // Iterates through individual rules, parsing and storing them in the wave's array of rules
            for (int i = 0; i < ruleStrings.Length; ++i)
                wave.waveRules[i] = ParseSingleWaveRule(ruleStrings[i]);

            return wave;
        }

        /// <summary>
        /// Compact Regex parser that handles the parsing of a single wave rule.
        /// It looks for the type and ID at the start of the string, and then looks for the optional timer and population cap.
        /// </summary>
        /// <param name="ruleText">The wave to be parsed.</param>
        /// <returns>The converted data.</returns>
        private WaveRuleAuthoring ParseSingleWaveRule(string ruleText)
        {
            var rule = new WaveRuleAuthoring
            {
                triggerPopulationCap = -1,
                triggerTimeSinceLastSpawn = -1
            };
            
            // If it starts with (used ^) T or C (Group 1), followed by ID digits (used \d) (Group 2)
            var mainMatch = Regex.Match(ruleText, @"^([TC])(\d+)");

            // Looks for <float> anywhere in the string
            var timeMatch = Regex.Match(ruleText, @"<(\d+)>");

            // Looks for [int] anywhere in the string
            var popMatch = Regex.Match(ruleText, @"\[(\d+)\]");

            if (!mainMatch.Success)
            {
                Debug.LogWarning($"Invalid wave rule format: {ruleText}");
                return rule;
            }

            // Sets the type using char and the ID num from the initial match
            rule.type = (mainMatch.Groups[1].Value == "T") ? WaveRuleType.Unit : WaveRuleType.Cluster;
            rule.clusterOrTypeId = int.Parse(mainMatch.Groups[2].Value);

            // Sets the timer if found, otherwise it remains -1
            if (timeMatch.Success)
                rule.triggerTimeSinceLastSpawn = float.Parse(timeMatch.Groups[1].Value);

            // Same here for the pop cap
            if (popMatch.Success)
                rule.triggerPopulationCap = int.Parse(popMatch.Groups[1].Value);

            return rule;
        }

        /// <summary>
        /// This is a very simple block parser which takes in a string of text and parses it into blocks.
        /// Each block has a header and a body, and the parser uses the state machine to determine whether it is currently parsing a header or a body.
        /// The parser returns a list of ParsedBlock objects, which contain the header and body of each block.
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        public List<ParsedBlock> ParseBlocksFromFile(string fileContent)
        {
            //Resets the parser state to outside the block and clears the buffer, ready for the new block to be read in 
            ResetParser();

            List<ParsedBlock> parsedBlocks = new List<ParsedBlock>();
            ParsedBlock tempBlock = new ParsedBlock();
            foreach (char c in fileContent)
            {
                m_Buffer += c;
                switch (m_State)
                {
                    case BlockParserState.OutsideBlock:
                        HandleOutsideBlockState(c, ref tempBlock);
                        break;
                    case BlockParserState.InsideHeader:
                        HandleInsideHeaderState(c, ref tempBlock);
                        break;
                    case BlockParserState.InsideBody:
                        bool canAddBlock = HandleInsideBodyState(c, ref tempBlock);

                        if (canAddBlock)
                        {
                            //Add the finished block to the total list, ready to be output.
                            parsedBlocks.Add(tempBlock);
                            //Reset the temp block for the next block to be read in.
                            tempBlock = new ParsedBlock();
                        }

                        break;
                }
            }

            return parsedBlocks;
        }
    }

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

    public class ParsedBlockHeader
    {
        public string m_RawText;
        public ParsedBlockType m_Type;
        public int m_ID;
        public string m_Name;
    }

    public enum ParsedBlockType
    {
        UnitType,
        Cluster,
        Wave
    }

    public enum BlockParserState
    {
        OutsideBlock,
        InsideHeader,
        InsideBody
    }
}