using System.Linq;
using NetGore;
using NetGore.RPGComponents;

namespace NetGore.RPGComponents
{
    /// <summary>
    /// An enum containing the type of collections for Stats.
    /// </summary>
    public enum StatCollectionType : byte
    {
        Base = 0,
        Modified = 1,
        Requirement = 2
    }
}