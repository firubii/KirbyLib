using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Mint
{
    /// <summary>
    /// A representation of variable information as found in Mint Objects.
    /// </summary>
    public class MintVariable
    {
        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name;
        /// <summary>
        /// The type of the variable.<br/>
        /// Used by the game to know how much memory to allocate for the variable.
        /// </summary>
        public string Type;
        /// <summary>
        /// The type of the variable.<br/>
        /// Note: Used starting in Basil 7.2.0
        /// </summary>
        public uint TypeHash;
        /// <summary>
        /// Bitflags that tell the Mint VM how to treat this variable.
        /// </summary>
        public byte Flags;

        public MintVariable(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public MintVariable(uint type, string name)
        {
            TypeHash = type;
            Name = name;
        }
    }
}
