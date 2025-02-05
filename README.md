| Mujoco | Unity | NetMQ |
|:-:|:-:|:-:|
| <a href="https://github.com/google-deepmind/mujoco"><img src="./Assets/src/images.jpg" alt="mujoco" width="300"></a> | <a href="https://mujoco.readthedocs.io/en/stable/unity.html"><img src="./Assets/src/Unity_2021.svg" alt="unity" width="300"></a> | <a href="https://github.com/GlitchEnzo/NuGetForUnity"><img src="./Assets/src/8075215.png" alt="nuget" width="300"></a> |


# Table of Contents
- [Prerequisites](#prerequisites)
  - [Unity](#unity)
  - [Unity Project setup](#unity-project-setup)
  - [Unity Plugins](#unity-plugins)
    - [Mujoco](#mujoco)
    - [NuGetForUnity](#nugetforunity)
    - [NetMQ](#netmq)
    - [Newtonsoft.Json](#newtonsoftjson)
  - [Unity Scripts](#unity-scripts)
  - [Unity Assets](#unity-assets)
- [Unity Scenes](#unity-scenes)
- [FAQs](#faqs)

## Prerequisites

### Unity
Install [Unity Hub](https://unity.com/download). The workspace version is `6000.0.32f1`.

### Clone the repository
First, clone the repository and initialize the submodule (This assumes you already have `git` and `ssh` setup).
```bash
git clone git@github.com:Simsreal/simulator.git
cd simulator
git submodule update --init --recursive
```

In Unity Hub, go to `Projects`. Click `Add -> Add project from disk.`, select the path to the cloned `simulator` folder.

## Setup
Double click on the added `simulator` in Unity Hub to open the project. When prompted, click `Ignore` to setup the Unity Plugins without entering safe mode if you are opening the project for the first time.

Follow the instructions below to setup the Unity Plugins.

### Unity Plugins

#### Mujoco
1. In Unity editor, go to `Window -> Package Manager`
2. Click `+` button and select `Install package from disk`
3. Select the path as `mujoco/unity/package.json`.
4. Click `Install` button to install mujoco `3.2.7`.

<!-- If you are on Linux, setup the `.so` DLL as well:
```bash
wget https://github.com/google-deepmind/mujoco/releases/download/3.2.7/mujoco-3.2.7-linux-x86_64.tar.gz
mkdir -p ~/.mujoco
tar -xvzf mujoco-3.2.7-linux-x86_64.tar.gz -C ~/.mujoco
``` -->

#### NuGetForUnity
1. In Unity editor, go to `Window -> Package Manager`
2. Click + button on the upper-left of a window, and select `Add package from git URL`
3. Enter the following URL and click Add button
`https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity`

#### NetMQ
1. In menu, click `Nuget -> Manage NuGet Packages`
2. Search for `NetMQ` and click `Install` button to install `NetMQ`.

#### Newtonsoft.Json
1. In menu, click `Nuget -> Manage NuGet Packages`
2. Search for `Newtonsoft.Json` and click `Install` button to install `Newtonsoft.Json`.

### MJCF Import
1. In menu, click `Assets -> Import MJCF scene`
2. Select `simulator/Assets/MJCF/humanoid.xml` and click `Open` button
3. A humanoid model should be shown in the scene.

### Unity Scripts
All the scripts are located in `Assets/Scripts`.

To enable Mujoco API usage:
1. In menu, click `Edit -> Project Settings`
2. Click `Player` tab
3. In platform settings, click `Other Settings`
4. Search for `allow unsafe code` and check the box.
To apply Mujoco scripts
1. In Project folder, go to `Assets->Scripts->Simulation`
2. Drag and drop `control.cs` to the `humanoid` game object.


### Unity Assets
* [Starter Assets](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526) (free)
* [Animal pack deluxe](https://assetstore.unity.com/packages/3d/characters/animals/animal-pack-deluxe-99702)
* [The Visual Engine](https://assetstore.unity.com/packages/tools/utilities/the-visual-engine-286827?srsltid=AfmBOooEvsmJ4lYwBSmDCvyxRAC9RLq3f43LRQoHwi4ART23U_QAzOFR)


## Unity Scenes

## FAQs
### DllNotFoundException on Linux
In Unity Editor, Click `Assets -> Reimport All`.

### Humanoid is not colliding with Unity Assets
1. Check if `collider` has been added to Unity Asset
2. If not, add `collider` to Unity Asset
3. Right-click on the Unity Asset and select `Add a matching MuJoCo geom`

### Unable to Establish NetMQ Connection Between *Simsreal* and *Simulator*

1. Please check the network configuration in `zmq_communicator.cs` and ensure that the IP addresses are correct.

2. Verify your firewall configuration to allow the following connections:
   - Ports: 5556, 5557;
   - Programs: Python, Unity, Unity Editor.

#### Notes Specific to WSL2
If you're using *WSL2* with NAT to run *Simsreal*, ensure that inbound connections from WSL are allowed. By default, Windows firewall blocks all inbound connections from WSL.

1. Press `WIN + X`, then press `A` to open a terminal with administrator privileges.
2. Check the network interface name of WSL using the following command:
   ```powershell
   Get-NetAdapter | Where-Object Name -like "*WSL*" | Select-Object Name
   ```
   Typically, the interface name will be `vEthernet (WSL)` or `vEthernet (WSL (Hyper-V firewall))`.
3. Use the following PowerShell command to allow all inbound connections from WSL:
	```powershell
	New-NetFirewallRule -Name WSLAllowAllInbound `
		-DisplayName "Allow WSL Inbound" `
		-Direction Inbound `
		-InterfaceAlias "vEthernet (WSL)" `
		-Action Allow
	```
	**Note:** Replace `"vEthernet (WSL)"` with the actual adapter name you found in the previous step.
