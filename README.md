# Age of Guilds

A Unity project hosted on GitHub.

## Getting Started

### Prerequisites

- [Unity](https://unity.com/download) (check `ProjectSettings/ProjectVersion.txt` for the required version)
- [Git](https://git-scm.com/)
- [Git LFS](https://git-lfs.com/) — required for large binary assets (textures, audio, models, etc.)

### Cloning the Repository

```bash
# 1. Install Git LFS (once per machine)
git lfs install

# 2. Clone the repository
git clone https://github.com/Jelly-Labs/age-of-guilds.git

# 3. Open the project in Unity Hub
#    File → Open Project → select the cloned folder
```

### Pushing Your Unity Project

If you are copying an existing Unity project into this repository:

```bash
# 1. Copy your Unity project files into the cloned folder
#    (Assets/, ProjectSettings/, Packages/ are the essential folders)

# 2. Stage and commit
git add .
git commit -m "Initial Unity project"
git push
```

> **Note:** Large binary files (images, audio, 3D models, etc.) are automatically
> tracked by Git LFS thanks to the `.gitattributes` file included in this repo.
> Make sure `git lfs install` has been run before your first push.

## Project Structure

| Folder | Description |
|---|---|
| `Assets/` | All game assets and source code |
| `Packages/` | Unity Package Manager manifest |
| `ProjectSettings/` | Unity project settings |

## License

See [LICENSE](LICENSE) for details.
<img width="1920" height="1080" alt="Deck" src="https://github.com/user-attachments/assets/7d2d430b-24df-4dc0-81c6-fcb4b6ad2529" />

<br>
This is a temporary repo for Age of Guilds game. The game is being developed in Unity engine, and is using the Unity's built in tool for source control. This repos is only for purposes of the Colosseum hackaton. If you need updates or access to the original Unity organization please contact jovan@jellylabs.org
