| Mujoco | Unity | NetMQ |
|:-:|:-:|:-:|
| <a href="https://github.com/google-deepmind/mujoco"><img src="./Assets/src/images.jpg" alt="mujoco" width="300"></a> | <a href="https://mujoco.readthedocs.io/en/stable/unity.html"><img src="./Assets/src/Unity_2021.svg" alt="unity" width="300"></a> | <a href="https://github.com/GlitchEnzo/NuGetForUnity"><img src="./Assets/src/8075215.png" alt="nuget" width="300"></a> |


## Prerequisites

### Mujoco Submodule
```bash
git submodule update --init --recursive
```

### Unity Plugins

#### Mujoco
1. In Unity editor, go to `Window -> Package Manager`
2. Click `+` button and select `Install package from disk`
3. Select the path as `mujoco/unity/package.json`.
4. Click `Install` button.

#### NetMQ
1. Install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)
2. In `NuGetForUnity` settings, install `NetMQ`.

## Launch Unity
