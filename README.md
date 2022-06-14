# Unity SaveManager
A custom Unity SaveManager that allows easy saving &amp; loading of Variables in any of your Scripts.

## Installation

### Method A - OpenUPM

1. Install the [`openupm-cli`](https://github.com/openupm/openupm-cli#installation).
2. Open a new terminal or cmd in your project root directory.
3. Run `openupm install com.medallyon.savemanager`.
4. Go back to Unity and allow it to install the package into your project.

### Method B - `manifest.json`

<details>
  <summary>Installation Instructions for `manifest.json`</summary>
  
  1. Open the `manifest.json` file found in your project root directory under `Packages > manifest.json`
  2. Add `"com.medallyon.savemanager": "https://github.com/medallyon/unity-savemanager.git",` to the `dependencies` object.
  3. Go back to Unity and allow it to install the package into your project.
  
</details>

### Method C - Install via GitHub

<details>
  <summary>Installation Instructions for installing via GitHub</summary>
  
  1. In the Unity Editor, open the **Package Manager** window (`Window > Package Manager`).
  2. Click the ➕ sign in the top-left corner of the Package Manager and select **Add package from git URL...**
  3. Insert `https://github.com/medallyon/unity-savemanager.git` and click **Add**.
  
</details>
