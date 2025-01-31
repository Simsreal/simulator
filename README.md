| Mujoco | Unity | NetMQ |
|:-:|:-:|:-:|
| <a href="https://github.com/google-deepmind/mujoco"><img src="./Assets/src/images.jpg" alt="mujoco" width="300"></a> | <a href="https://mujoco.readthedocs.io/en/stable/unity.html"><img src="./Assets/src/Unity_2021.svg" alt="unity" width="300"></a> | <a href="https://github.com/GlitchEnzo/NuGetForUnity"><img src="./Assets/src/8075215.png" alt="nuget" width="300"></a> |


## Prerequisites

### Unity
Install [Unity Hub](https://unity.com/download). The workspace version is `6000.0.32f1`.

### Unity Project setup
First, clone the repository and initialize the submodule (This assumes you already have `git` and `ssh` setup).
```bash
git clone git@github.com:Simsreal/simulator.git
cd simulator
git submodule update --init --recursive
```

In Unity Hub, go to `Projects`. Click `Add -> Add project from disk.`, select the path to the cloned `simulator` folder.

### Unity Plugins

#### Mujoco
1. In Unity editor, go to `Window -> Package Manager`
2. Click `+` button and select `Install package from disk`
3. Select the path as `mujoco/unity/package.json`.
4. Click `Install` button.

If you are on Linux, setup the `.so` DLL as well:
```bash
wget https://github.com/google-deepmind/mujoco/releases/download/3.2.7/mujoco-3.2.7-linux-x86_64.tar.gz
mkdir -p ~/.mujoco
tar -xvzf mujoco-3.2.7-linux-x86_64.tar.gz -C ~/.mujoco
```

#### NetMQ
1. Install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)
2. In `NuGetForUnity` settings, install `NetMQ`, `Newtonsoft.Json`.


## Unity Assets
* [Animal pack deluxe](https://assetstore.unity.com/packages/3d/characters/animals/animal-pack-deluxe-99702)
* [Starter Assets](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526)
* [The Visual Engine](https://assetstore.unity.com/packages/tools/utilities/the-visual-engine-286827?srsltid=AfmBOooEvsmJ4lYwBSmDCvyxRAC9RLq3f43LRQoHwi4ART23U_QAzOFR)
    * Paid, not needed for now. (120$ usd)

## Unity Scripts
All the scripts are located in `Assets/Scripts`.

## FAQs
### DllNotFoundException on Linux
In Unity Editor, Click `Assets -> Reimport All`.

### Humanoid is not colliding with Unity Assets
1. Check if `collider` has been added to Unity Asset
2. If not, add `collider` to Unity Asset
3. Right-click on the Unity Asset and select `Add a matching MuJoCo geom`