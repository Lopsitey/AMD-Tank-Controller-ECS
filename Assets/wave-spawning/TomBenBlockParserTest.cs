#region

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

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
        private string TrimFromEnd(string text, string end) => text[0..^end.Length];
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
        /// Reads the header of the block and parses it into a ParsedBlockHeader object, which contains variables with the converted data
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        /// <exception cref="UnityException"></exception>
        public ParsedBlockHeader GetBlockHeader(in ParsedBlock block)
        {
            //This is the regex pattern to match the header - feel free to look it up at regexer.com
            string pattern = @"(type|wave|cluster)\s*\-\s*(\d+)\s*(?:\((.+)\))?";

            //Gets all subgroups of the header which match the pattern - e.g. the type, id, and name (if it exists)
            var matches = Regex.Match(block.m_Header, pattern);

            //If there is no match, then the header is in the wrong format because the regex didn't match
            if (!matches.Success)
                throw new UnityException(
                    "Failed to parse block header! Ensure it is in the format 'type/cluster/wave - id (name)'");

            return null;
        }

        /// <summary>
        /// Uses a switch expression to convert the type string from the header into a ParsedBlockType enum.
        /// </summary>
        /// <param name="type">The type to be converted</param>
        /// <returns>The converted type or an error if it could not be converted.</returns>
        /// <exception cref="UnityException"></exception>
        public ParsedBlockType GetBlockTypeFromString(string type) => type switch
        {
            "wave" => ParsedBlockType.Wave,
            "cluster" => ParsedBlockType.Cluster,
            "type" => ParsedBlockType.UnitType,
            // Runs if none of the above cases are hit thus, the type is invalid - essentially a default case
            _ => throw new UnityException($"Invalid block type {type} found in header!")
        };

        /// <summary>
        /// This is a very simple block parser which takes in a string of text and parses it into blocks.
        /// Each block has a header and a body, and the parser uses the state machine to determine whether it is currently parsing a header or a body.
        /// The parser returns a list of ParsedBlock objects, which contain the header and body of each block.
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        public List<ParsedBlock> ParseBlocksFromFile(string fileContent)
        {
            //Moves back into the outside block state and clears the buffer, ready for the new block
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
        [SerializeField] public string m_FilePath;
        [SerializeField] public List<ParsedBlock> m_ParsedBlocks;

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
        }
    }
    
    public enum BlockParserState
    {
        OutsideBlock,
        InsideHeader,
        InsideBody
    }
    
    public enum ParsedBlockType
    {
        UnitType,
        Cluster,
        Wave
    }

    public class ParsedBlockHeader
    {
        public string m_RawText;
        public ParsedBlockType m_Type;
        public int m_ID;
        public string m_Name;
    }
}