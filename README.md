[![openupm](https://img.shields.io/npm/v/com.medallyon.savemanager?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.medallyon.savemanager/)

# Unity SaveManager
A custom Unity SaveManager that allows easy saving &amp; loading of Variables in any of your Scripts.

## Installation

### Method A - OpenUPM

1. Install the [`openupm-cli`](https://github.com/openupm/openupm-cli#installation).
2. Open a new terminal or cmd in your project root directory.
3. Run `openupm install com.medallyon.savemanager`.
4. Go back to Unity and allow it to install the package into your project.

<details>
  <summary><h3>Method B - <code>manifest.json</code></h3></summary>
  
  1. Open the `manifest.json` file found in your project root directory under `Packages > manifest.json`
  2. Add `"com.medallyon.savemanager": "https://github.com/medallyon/unity-savemanager.git",` to the `dependencies` object.
  3. Go back to Unity and allow it to install the package into your project.
  
</details>

<details>
  <summary><h3>Method C - Install via GitHub</h3></summary>
  
  1. In the Unity Editor, open the **Package Manager** window (`Window > Package Manager`).
  2. Click the ➕ sign in the top-left corner of the Package Manager and select **Add package from git URL...**
  3. Insert `https://github.com/medallyon/unity-savemanager.git` and click **Add**.
  
</details>

## Usage
You can use this SaveManager library out-of-the-box as part of your Scripts. For examples, see the [`Samples`](https://github.com/medallyon/unity-savemanager/tree/master/Samples) folder.

### The `[Save]` Attribute
You can attach the `[Save]` Attribute to any Field or Property in your Scripts. The SaveManager is able to store and load primitive types (`int`, `string`, `bool`, etc.) without any extra processing.

For non-primitive types, such as your own custom classes, you will be required to implement the `ISaveable` interface in your desired MonoBehaviours. Read on to find out how to get started with this.

### The `ISaveable` Interface
The `ISaveable` Interface requires you to add the `public void OnRestore(bool isFirstLoad, Dictionary<string, object> data) { }` method. The `OnRestore` method is called on every MonoBehaviour that implements this Interface.

### The `OnRestore` Method

<details>
  <summary><h4>The <code>bool isFirstLoad</code> Parameter</h4></summary>
  
  `isFirstLoad` denotes whether a Data file was found and loaded. If this evaluates to `true`, the `data` parameter will not contain any entries, and it usually means that this is the first time that the game was started. If this evaluates to `false`, the `data` parameter will contain entries for all variables that are decorated with the `[Save]` Attribute in this MonoBehaviour.
  
</details>

<details>
  <summary><h4>The <code>Dictionary&lt;string, object&gt; data</code> Parameter</h4></summary>
  
  The `data` parameter is a Dictionary that maps the name (`string`) of the `Save`d Field or Property to its value (`object`). You will have to cast the value into the desired type. For complex values, you may have to do more processing using methods from the `Newtonsoft.Json` Assembly, such as `JsonConvert.DeserializeObject<T>`. More samples will follow illustrating how to cast this data properly.
  
</details>

<details>
  <summary><h4>Accessing Variables and their Values through <code>data</code></h4></summary>
  
  Since the `data` parameter is a Dictionary, we can use its indexer to access Variables via their names. Here, the C# `nameof` keyword comes in handy. See the following example on how to access the saved `_playerID` variable:

```cs
public class MySaveableComponent : MonoBehaviour, ISaveable
{
    [Save] private Guid _playerID = Guid.Empty;

    // This happens before 'Start' is called
    public void OnRestore(bool isFirstLoad, Dictionary<string, object> data)
    {
        // If the '_playerID' Variable was never saved, use a desired default Value
        if (isFirstLoad)
        {
            _playerID = Guid.NewGuid();
            return;
        }
        
        // Get the name of the saved Variable as a string ("_playerID")
        string variableName = nameof(_playerID);
        
        // Access the value of the saved Variable through the Dictionary's indexer
        string variableValue = (string)data[variableName];
        
        // Parse the string Value as a 'Guid' and set the Value for '_playerID'
        _playerID = Guid.Parse(variableValue);
    }
}
```

The contents of `OnRestore` in the above example could also be condensed into a single statement:

```cs
_playerID = isFirstLoad ? Guid.NewGuid() : Guid.Parse((string)data[nameof(_playerID)]);
```
  
</details>

## Contributing
Any Contributions are welcome through Pull Requests. More details will follow.
