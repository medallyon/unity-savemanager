using System.Collections.Generic;

namespace Medallyon
{
    public interface ISaveable
    {
        /// <summary>
        /// Runs once when the game is started for the very first time. Use this to initialize your save data.
        /// </summary>
        public void OnFirstLoad();

        /// <summary>
        /// This function is called when saved data is restored. For primitive types, data is restored automatically. Use this function to restore non-primitive types, such as
        /// <see cref="Dictionary{TKey,TValue}" />s. Access data using the same variable names through this object's indexer.
        /// </summary>
        /// <param name="data">
        /// A Dictionary mapping the name of a Field or Property in this class marked with the
        /// <see cref="SaveAttribute" /> to its serialized value.
        /// </param>
        public void OnRestore(Dictionary<string, object> data);
    }
}
