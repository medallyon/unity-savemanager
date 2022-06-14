using System.Collections.Generic;

namespace Medallyon
{
    public interface ISaveable
    {
        /// <summary>
        /// This function is called when saved data is restored. For primitive types, data is restored automatically. Use this function to restore non-primitive types, such as <see cref="Dictionary{TKey,TValue}" />s. Access data using the same variable names through this object's indexer.
        /// </summary>
        /// <param name="isFirstLoad">Whether this is the very first time that this method is invoked. Explicitly check this to ensure you can process the data you expect.</param>
        /// <param name="data">A Dictionary mapping the name of a Field or Property in this class marked with the <see cref="SaveAttribute" /> to its serialized value. This will be empty if <paramref name="isFirstLoad" /> evaluates to true.</param>
        public void OnRestore(bool isFirstLoad, Dictionary<string, object> data);
    }
}
